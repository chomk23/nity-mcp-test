#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ForTheCompany.EditorTools
{
    /// <summary>
    /// 메뉴: "For The Company → Cover Wall Gaps"
    /// 시설 외곽 모서리 + 내부 벽 모서리에 Wall Pillar prefab 자동 배치 → 갭 시각적으로 가림.
    /// 좌표는 시설 사이즈 추정(약 45×36)에 기반. 사용자가 보고 미세 조정 가능.
    /// </summary>
    public static class WallGapCoverSetup
    {
        private const string ParentName = "WallPillars";
        private const string P_Pillar = "Assets/ScifiOfficeLite/Prefabs/Wall/Wall Pillar.prefab";
        private const string P_Pillar3 = "Assets/ScifiOfficeLite/Prefabs/Wall/Wall Pillar 3.prefab";

        [MenuItem("For The Company/Cover Wall Gaps")]
        public static void Setup()
        {
            var existing = GameObject.Find(ParentName);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("기존 WallPillars 발견",
                    $"'{ParentName}'가 이미 있습니다. 삭제하고 새로 만들까요?",
                    "재생성", "취소")) return;
                Undo.DestroyObjectImmediate(existing);
            }

            var parent = new GameObject(ParentName);
            Undo.RegisterCreatedObjectUndo(parent, "Create WallPillars");
            int n = 0;

            // ═══ 시설 외곽 모서리 4개 (큰 기둥) ═══
            // 시설 외곽 추정: x ∈ [-22, 22], z ∈ [-18, 14]
            float xMin = -22f, xMax = 22f;
            float zMin = -18f, zMax = 14f;
            n += Place(P_Pillar3, new Vector3(xMin, 0f, zMin), 0f, parent, "Corner_SW");
            n += Place(P_Pillar3, new Vector3(xMax, 0f, zMin), 0f, parent, "Corner_SE");
            n += Place(P_Pillar3, new Vector3(xMin, 0f, zMax), 0f, parent, "Corner_NW");
            n += Place(P_Pillar3, new Vector3(xMax, 0f, zMax), 0f, parent, "Corner_NE");

            // ═══ 내부 방 모서리 — 각 방 외곽 모서리에 작은 기둥 ═══
            // 방 좌표 (NPC/Clue 기준)
            Vector2[] roomCorners = new Vector2[]
            {
                // 연구실 (-17, 11) 주변 모서리
                new Vector2(-21f, 14f), new Vector2(-13f, 14f),
                new Vector2(-21f, 8f),  new Vector2(-13f, 8f),
                // 서버실 (0, 11) 주변
                new Vector2(-4f, 14f), new Vector2(4f, 14f),
                new Vector2(-4f, 8f),  new Vector2(4f, 8f),
                // 보안통제실 (13, 11) 주변
                new Vector2(9f, 14f),  new Vector2(17f, 14f),
                new Vector2(9f, 8f),   new Vector2(17f, 8f),
                // 휴게실 (-13, 0) 주변
                new Vector2(-17f, 4f), new Vector2(-9f, 4f),
                new Vector2(-17f, -4f), new Vector2(-9f, -4f),
                // 카드키 구역 (18, 2)
                new Vector2(15f, 5f),  new Vector2(21f, 5f),
                new Vector2(15f, -1f), new Vector2(21f, -1f),
                // 창고 (-20, -14)
                new Vector2(-23f, -11f), new Vector2(-17f, -11f),
                new Vector2(-23f, -17f), new Vector2(-17f, -17f),
                // 데이터센터 (3, -14)
                new Vector2(-4f, -11f), new Vector2(10f, -11f),
                new Vector2(-4f, -17f), new Vector2(10f, -17f),
                // 전력실 (18, -11)
                new Vector2(14f, -7f), new Vector2(22f, -7f),
                new Vector2(14f, -15f), new Vector2(22f, -15f)
            };

            foreach (var c in roomCorners)
            {
                n += Place(P_Pillar, new Vector3(c.x, 0f, c.y), 0f, parent, $"Pillar_{c.x}_{c.y}");
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("벽 갭 커버 완료",
                $"총 {n}개 Wall Pillar 배치.\n\n" +
                "• 시설 외곽 4 모서리: 큰 Wall Pillar 3\n" +
                "• 각 방 모서리 32개: 작은 Wall Pillar\n\n" +
                "좌표는 추정이라 일부 위치 어색할 수 있어요.\n" +
                "Hierarchy → WallPillars → 해당 기둥 선택 → Position 미세조정 가능.",
                "확인");

            Selection.activeGameObject = parent;
        }

        private static int Place(string prefabPath, Vector3 pos, float yRot, GameObject parent, string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[WallGapCover] prefab 없음: {prefabPath}");
                return 0;
            }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
            if (go == null) return 0;
            go.name = name;
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, yRot, 0f);
            Undo.RegisterCreatedObjectUndo(go, "Place wall pillar");
            return 1;
        }
    }
}
#endif
