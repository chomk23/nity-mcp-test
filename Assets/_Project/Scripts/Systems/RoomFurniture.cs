using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ForTheCompany.Systems
{
    /// <summary>
    /// 시설의 각 방에 가구·서버랙·책상·모니터 등을 primitive cube로 자동 배치.
    /// "산업시설" 느낌을 살리기 위한 시각 폴리시.
    /// 부모 "RoomFurniture" GameObject 아래에 자식으로 묶음.
    /// </summary>
    public class RoomFurniture : MonoBehaviour
    {
        private const string ParentName = "RoomFurniture";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            EnsureSpawned();
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureSpawned();
        }

        private static void EnsureSpawned()
        {
            if (SceneManager.GetActiveScene().name != "FacilityScene") return;
            if (GameObject.Find(ParentName) != null) return;

            var parent = new GameObject(ParentName);
            int count = 0;

            foreach (var piece in BuildFurniturePieces())
            {
                Spawn(parent.transform, piece);
                count++;
            }

            Debug.Log($"[RoomFurniture] 가구 {count}개 배치 완료");
        }

        private static void Spawn(Transform parent, FurniturePiece p)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = p.name;
            go.transform.SetParent(parent, false);
            go.transform.position = p.pos;
            go.transform.localScale = p.scale;

            // 플레이어 통과 가능하도록 Collider 제거
            var col = go.GetComponent<BoxCollider>();
            if (col != null) Object.Destroy(col);

            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                var mpb = new MaterialPropertyBlock();
                mpb.SetColor("_BaseColor", p.color);
                mr.SetPropertyBlock(mpb);
            }
        }

        private struct FurniturePiece
        {
            public string name;
            public Vector3 pos;
            public Vector3 scale;
            public Color color;

            public FurniturePiece(string name, Vector3 pos, Vector3 scale, Color color)
            {
                this.name = name; this.pos = pos; this.scale = scale; this.color = color;
            }
        }

        // 가구 색상 팔레트
        private static readonly Color C_Desk      = new Color(0.36f, 0.26f, 0.18f); // 갈색 책상
        private static readonly Color C_Monitor   = new Color(0.10f, 0.10f, 0.13f); // 어두운 모니터
        private static readonly Color C_Server    = new Color(0.08f, 0.08f, 0.10f); // 검은 서버랙
        private static readonly Color C_ServerLed = new Color(0.20f, 0.90f, 1.00f); // 시안 LED
        private static readonly Color C_Sofa      = new Color(0.30f, 0.32f, 0.40f); // 짙은 회색
        private static readonly Color C_Chair     = new Color(0.25f, 0.25f, 0.28f);
        private static readonly Color C_Box       = new Color(0.45f, 0.34f, 0.22f); // 종이박스
        private static readonly Color C_Transformer = new Color(0.22f, 0.22f, 0.24f); // 회색 금속
        private static readonly Color C_Cable     = new Color(0.18f, 0.18f, 0.20f);
        private static readonly Color C_Bookshelf = new Color(0.40f, 0.28f, 0.18f);
        private static readonly Color C_Vending   = new Color(0.35f, 0.20f, 0.50f); // 자판기

        private static IEnumerable<FurniturePiece> BuildFurniturePieces()
        {
            var list = new List<FurniturePiece>();

            // ─── 연구실 (서북, 파랑) — NPC 연구원 (-17, 11) ───
            list.Add(new FurniturePiece("Desk_Research", new Vector3(-17f, 0.4f, 12.5f),
                new Vector3(3f, 0.8f, 1.5f), C_Desk));
            list.Add(new FurniturePiece("Monitor_Research", new Vector3(-17f, 1.3f, 13f),
                new Vector3(1.2f, 0.7f, 0.1f), C_Monitor));
            list.Add(new FurniturePiece("Chair_Research", new Vector3(-17f, 0.5f, 11.5f),
                new Vector3(0.6f, 1f, 0.6f), C_Chair));
            list.Add(new FurniturePiece("Bookshelf_Research", new Vector3(-21f, 1.25f, 13.5f),
                new Vector3(0.5f, 2.5f, 1.8f), C_Bookshelf));

            // ─── 서버실 (북, 빨강) — NPC 네트워크관리자 (0, 11) ───
            for (int i = 0; i < 4; i++)
            {
                float x = -3f + i * 2f;
                list.Add(new FurniturePiece($"Server_Rack_{i}",
                    new Vector3(x, 1.25f, 13f),
                    new Vector3(1.4f, 2.5f, 0.9f), C_Server));
                // LED (서버랙 정면)
                list.Add(new FurniturePiece($"Server_Led_{i}",
                    new Vector3(x, 1.8f, 12.45f),
                    new Vector3(0.6f, 0.1f, 0.05f), C_ServerLed));
            }

            // ─── 데이터센터 (남, 청록) — clue data_traffic (3, -14) ───
            for (int i = 0; i < 3; i++)
            {
                float x = -1f + i * 2f;
                list.Add(new FurniturePiece($"DataCenter_Rack_{i}",
                    new Vector3(x, 1.25f, -16f),
                    new Vector3(1.4f, 2.5f, 0.9f), C_Server));
                list.Add(new FurniturePiece($"DataCenter_Led_{i}",
                    new Vector3(x, 1.7f, -15.45f),
                    new Vector3(0.6f, 0.08f, 0.05f), C_ServerLed));
            }
            // 트래픽 모니터 벽
            list.Add(new FurniturePiece("DataCenter_MonitorWall",
                new Vector3(6f, 1.6f, -11f),
                new Vector3(4f, 1f, 0.2f), C_Monitor));

            // ─── 휴게실 (남쪽, 초록) — RacingConsole (-13, 2) ───
            list.Add(new FurniturePiece("Sofa_Lounge",
                new Vector3(-15f, 0.4f, -3f),
                new Vector3(3.5f, 0.8f, 1.2f), C_Sofa));
            list.Add(new FurniturePiece("Table_Lounge",
                new Vector3(-15f, 0.35f, -1.5f),
                new Vector3(1.5f, 0.7f, 1.5f), C_Desk));
            list.Add(new FurniturePiece("Vending_Lounge",
                new Vector3(-17f, 1.1f, -4.5f),
                new Vector3(1.2f, 2.2f, 0.8f), C_Vending));

            // ─── 창고 (서남, 갈색) — clue storage_box (-20, -14) ───
            list.Add(new FurniturePiece("Pallet_Storage_1",
                new Vector3(-22f, 0.6f, -16f),
                new Vector3(1.5f, 1.2f, 1.5f), C_Box));
            list.Add(new FurniturePiece("Pallet_Storage_2",
                new Vector3(-19f, 0.6f, -17f),
                new Vector3(1.5f, 1.2f, 1.5f), C_Box));
            list.Add(new FurniturePiece("Pallet_Storage_3",
                new Vector3(-22f, 1.8f, -16f),
                new Vector3(1.2f, 1.0f, 1.2f), C_Box));

            // ─── 전력실 (동, 노랑) — NPC 시설관리자 (18, -11) ───
            list.Add(new FurniturePiece("Transformer_1",
                new Vector3(20f, 0.9f, -14f),
                new Vector3(1.8f, 1.8f, 1.8f), C_Transformer));
            list.Add(new FurniturePiece("Transformer_2",
                new Vector3(20f, 0.9f, -8f),
                new Vector3(1.8f, 1.8f, 1.8f), C_Transformer));
            list.Add(new FurniturePiece("CableBox_Power",
                new Vector3(17f, 0.3f, -10f),
                new Vector3(0.9f, 0.6f, 0.8f), C_Cable));

            // ─── 보안통제실 (북, 보라) — AccusationConsole 옆 ───
            list.Add(new FurniturePiece("MonitorWall_Security",
                new Vector3(13f, 1.6f, 13.5f),
                new Vector3(5f, 1.5f, 0.2f), C_Monitor));
            list.Add(new FurniturePiece("Desk_Security",
                new Vector3(13f, 0.4f, 11.5f),
                new Vector3(3f, 0.8f, 1.5f), C_Desk));
            list.Add(new FurniturePiece("Chair_Security",
                new Vector3(13f, 0.5f, 10.5f),
                new Vector3(0.6f, 1f, 0.6f), C_Chair));

            // ─── 카드키 구역 — clue cardkey_log (18, 2) ───
            list.Add(new FurniturePiece("Cardkey_Terminal",
                new Vector3(18f, 1.1f, 2f),
                new Vector3(0.8f, 2.2f, 0.6f), C_Vending));
            list.Add(new FurniturePiece("Cardkey_Terminal_LED",
                new Vector3(18f, 1.7f, 1.65f),
                new Vector3(0.4f, 0.15f, 0.05f), C_ServerLed));

            return list;
        }
    }
}
