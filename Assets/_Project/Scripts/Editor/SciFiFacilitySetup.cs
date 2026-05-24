#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ForTheCompany.EditorTools
{
    /// <summary>
    /// 메뉴: "For The Company → Setup Sci-Fi Facility"
    /// Assets/_Project/Prefabs/SciFiFacility.prefab을 씬에 인스턴스화.
    /// (사용자가 Hierarchy에서 SciFiFacility GameObject를 prefab으로 저장한 후 이 메뉴를 사용)
    /// </summary>
    public static class SciFiFacilitySetup
    {
        private const string ParentName = "SciFiFacility";
        private const string PrefabPath = "Assets/_Project/Prefabs/SciFiFacility.prefab";

        [MenuItem("For The Company/Setup Sci-Fi Facility")]
        public static void Setup()
        {
            // 기존 RoomFurniture(primitive 가구) 제거
            var oldFurniture = GameObject.Find("RoomFurniture");
            if (oldFurniture != null)
            {
                Undo.DestroyObjectImmediate(oldFurniture);
            }

            // 기존 SciFiFacility 있으면 재생성 확인
            var existing = GameObject.Find(ParentName);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("기존 SciFi 시설 발견",
                    $"'{ParentName}'가 이미 있습니다. 삭제하고 prefab으로 새로 생성할까요?",
                    "재생성", "취소")) return;
                Undo.DestroyObjectImmediate(existing);
            }

            // Prefab 로드
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("Prefab을 찾을 수 없음",
                    $"'{PrefabPath}'가 없습니다.\n\n" +
                    "1. Hierarchy의 SciFiFacility GameObject를\n" +
                    "2. Project 창의 Assets/_Project/Prefabs/ 폴더로 드래그\n" +
                    "3. \"Original Prefab\" 선택해서 저장\n" +
                    "4. 이 메뉴 다시 실행",
                    "확인");
                Debug.LogError($"[SciFiFacility] Prefab not found at {PrefabPath}");
                return;
            }

            // Prefab 인스턴스화
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (go == null)
            {
                Debug.LogError("[SciFiFacility] Prefab instantiation failed");
                return;
            }
            go.name = ParentName;
            Undo.RegisterCreatedObjectUndo(go, "Instantiate SciFiFacility");

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Sci-Fi Facility 생성 완료",
                $"'{PrefabPath}'를 씬에 인스턴스화했습니다.\n\n" +
                "수정 사항이 있으면:\n" +
                "1. Hierarchy에서 SciFiFacility 안의 객체 수정\n" +
                "2. Inspector 상단의 \"Overrides\" → \"Apply All\" 클릭하면\n" +
                "   변경 사항이 prefab에 자동 저장됨",
                "확인");

            Selection.activeGameObject = go;
        }
    }
}
#endif
