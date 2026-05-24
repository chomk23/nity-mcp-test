#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ForTheCompany.EditorTools
{
    /// <summary>
    /// 메뉴: "For The Company → Setup Sci-Fi Floors"
    /// 시설 전체 바닥에 ScifiOfficeLite 바닥 prefab들을 grid로 배치.
    /// SciFiFacility(가구)는 건드리지 않음 — 별도 "SciFiFloors" GameObject.
    /// </summary>
    public static class SciFiFloorsSetup
    {
        private const string ParentName = "SciFiFloors";
        private const string BasePath = "Assets/ScifiOfficeLite/Prefabs/Carpets and Floors/";

        // 시설 전체 베이스 바닥 (45x36 시설을 3x2 grid로 커버)
        private const string P_EpoxyGround = BasePath + "20m Epoxy Ground.prefab";
        // 방별 카펫 (분위기 차별화)
        private const string P_Carpet5     = BasePath + "Carpet 5.prefab";
        private const string P_Carpet8     = BasePath + "Carpet 8.prefab";
        private const string P_Carpet9     = BasePath + "Carpet 9.prefab";
        private const string P_Carpet10    = BasePath + "Carpet 10.prefab";
        private const string P_Carpet11    = BasePath + "Carpet 11.prefab";
        private const string P_Carpet12    = BasePath + "Carpet 12.prefab";
        private const string P_Carpet13    = BasePath + "Carpet 13.prefab";

        [MenuItem("For The Company/Setup Sci-Fi Floors")]
        public static void Setup()
        {
            var existing = GameObject.Find(ParentName);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("기존 SciFiFloors 발견",
                    $"'{ParentName}'가 이미 있습니다. 삭제하고 새로 만들까요?",
                    "재생성", "취소")) return;
                Undo.DestroyObjectImmediate(existing);
            }

            var parent = new GameObject(ParentName);
            Undo.RegisterCreatedObjectUndo(parent, "Create SciFiFloors");
            int n = 0;

            // ═══ 기존 방 색깔 바닥 비활성화 (베이스 Epoxy가 보이도록) ═══
            string[] oldRoomFloors = {
                "Floor_Research", "Floor_Server", "Floor_Security",
                "Floor_Lounge", "Floor_CardKey", "Floor_Storage",
                "Floor_DataCenter", "Floor_Power"
            };
            int disabled = 0;
            foreach (var floorName in oldRoomFloors)
            {
                var floorGO = GameObject.Find(floorName);
                if (floorGO != null && floorGO.activeSelf)
                {
                    Undo.RecordObject(floorGO, "Disable old room floor");
                    floorGO.SetActive(false);
                    disabled++;
                }
            }
            Debug.Log($"[SciFiFloors] 기존 방 색깔 바닥 {disabled}개 비활성화");

            // ═══ 시설 전체 베이스 바닥 (20m Epoxy 3×3 grid, 시설 전체 커버) ═══
            // y = 0.02 — 비활성화된 기존 바닥 자리에 베이스 깔림
            for (int xi = -1; xi <= 1; xi++)
            {
                for (int zi = -1; zi <= 1; zi++)
                {
                    float x = xi * 15f;
                    float z = zi * 13f;
                    n += Place(P_EpoxyGround, new Vector3(x, 0.02f, z), 0f, parent,
                        $"Floor_Base_{xi+1}_{zi+1}");
                }
            }

            // ═══ 방별 카펫 (분위기 차별화, Epoxy 위 y=0.04) ═══
            // 연구실 (-17, 11) — Carpet 8 (차분한 톤)
            n += Place(P_Carpet8, new Vector3(-17f, 0.04f, 11f), 0f, parent, "Floor_Research");
            // 서버실 (0, 11) — Carpet 12 (서버실용)
            n += Place(P_Carpet12, new Vector3(0f, 0.04f, 11f), 0f, parent, "Floor_Server");
            // 보안통제실 (13, 11) — Carpet 13 (보안용)
            n += Place(P_Carpet13, new Vector3(13f, 0.04f, 11f), 0f, parent, "Floor_Security");
            // 휴게실 (-13, 0) — Carpet 10 (편안한 톤)
            n += Place(P_Carpet10, new Vector3(-13f, 0.04f, 0f), 0f, parent, "Floor_Lounge");
            // 카드키 구역 (18, 2) — Carpet 11
            n += Place(P_Carpet11, new Vector3(18f, 0.04f, 2f), 0f, parent, "Floor_Cardkey");
            // 창고 (-20, -14) — Carpet 5 (창고용 거친 톤)
            n += Place(P_Carpet5, new Vector3(-20f, 0.04f, -14f), 0f, parent, "Floor_Storage");
            // 데이터센터 (3, -14) — Carpet 12
            n += Place(P_Carpet12, new Vector3(3f, 0.04f, -14f), 0f, parent, "Floor_DataCenter");
            // 전력실 (18, -11) — Carpet 9 (전력 경고 톤)
            n += Place(P_Carpet9, new Vector3(18f, 0.04f, -11f), 0f, parent, "Floor_Power");

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Sci-Fi Floors 배치 완료",
                $"총 {n}개 바닥 prefab 배치 + 기존 방 바닥 {disabled}개 비활성화.\n\n" +
                "• 기존 방 색깔 바닥 (Floor_Server 등) 비활성화 — 베이스가 보이도록\n" +
                "• 시설 전체 베이스: 20m Epoxy Ground 9장 (3×3 grid, 시설 전체 커버)\n" +
                "• 각 방 카펫: 8장 (방마다 다른 톤, 베이스 위에 깔림)\n\n" +
                "SciFiFacility(가구)는 영향 안 받음 — 별도 GameObject 'SciFiFloors'에 묶음.\n" +
                "기존 방 바닥을 다시 보고 싶으면 Hierarchy에서 Facility/Floor_* 재활성화.",
                "확인");

            Selection.activeGameObject = parent;
        }

        private static int Place(string prefabPath, Vector3 pos, float yRot, GameObject parent, string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[SciFiFloors] prefab 없음: {prefabPath}");
                return 0;
            }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
            if (go == null) return 0;
            go.name = name;
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, yRot, 0f);
            Undo.RegisterCreatedObjectUndo(go, "Place SciFi floor");
            return 1;
        }
    }
}
#endif
