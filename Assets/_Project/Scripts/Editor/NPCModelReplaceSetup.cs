#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ForTheCompany.Player;

namespace ForTheCompany.EditorTools
{
    /// <summary>
    /// 메뉴: "For The Company → Replace NPCs with Kenney Characters"
    /// Kenney Blocky Characters .fbx를 자식으로 추가 + CharacterBobbing(procedural 걷기) 자동 부착.
    /// </summary>
    public static class NPCModelReplaceSetup
    {
        private const string BasePath = "Assets/kenney_blocky-characters_20/Models/FBX format/";

        // 캐릭터 모델 크기 — 1.0 = 원본, 0.7 = 70%로 작게, 0.5 = 절반
        // Kenney Blocky 원본이 좀 큰 편이라 0.7 권장
        private const float ModelScale = 0.7f;

        // NPC 이름 → Kenney character fbx 매핑
        private static readonly (string targetName, string fbxPath, string role)[] Mappings =
        {
            ("NPC_Researcher",      BasePath + "character-i.fbx", "연구원"),
            ("NPC_NetworkAdmin",    BasePath + "character-e.fbx", "네트워크관리자"),
            ("NPC_FacilityManager", BasePath + "character-c.fbx", "시설관리자"),
            ("Player",              BasePath + "character-q.fbx", "조사관(플레이어)"),
        };

        private const string GuardName = "GuardNPC";
        private const string GuardFbxPath = BasePath + "character-j.fbx"; // 경비원

        [MenuItem("For The Company/Replace NPCs with Kenney Characters")]
        public static void Replace()
        {
            int replaced = 0;
            foreach (var (targetName, fbxPath, role) in Mappings)
            {
                var go = GameObject.Find(targetName);
                if (go == null)
                {
                    Debug.LogWarning($"[NPCReplace] '{targetName}' GameObject 없음 — 스킵");
                    continue;
                }
                if (ReplaceModel(go, fbxPath, role)) replaced++;
            }

            // GuardNPC 사전 생성 + Kenney character 적용
            var guardGO = GameObject.Find(GuardName);
            if (guardGO == null)
            {
                guardGO = new GameObject(GuardName);
                guardGO.transform.position = new Vector3(0f, 1.1f, 4f);
                guardGO.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // 평소 player 향함
                guardGO.AddComponent<GuardNPC>();
                Undo.RegisterCreatedObjectUndo(guardGO, "Create GuardNPC");
                Debug.Log("[NPCReplace] GuardNPC GameObject 사전 생성");
            }
            else
            {
                // 이미 있으면 rotation만 보정 (평소 player 향함)
                Undo.RecordObject(guardGO.transform, "Guard rotation");
                guardGO.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
            CleanCapsuleMesh(guardGO);
            if (ReplaceModel(guardGO, GuardFbxPath, "경비원")) replaced++;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("NPC 모델 교체 완료",
                $"{replaced}명 NPC를 Kenney Blocky Character로 교체.\n\n" +
                "각 NPC에 CharacterBobbing 컴포넌트가 자동 부착 — 이동 시\n" +
                "위아래 흔들림 + 좌우 기울임으로 걷는 느낌 표현.\n\n" +
                "매핑은 임의 배치 (character-a, -d, -h, -l, -p):\n" +
                "Preview는 Assets/kenney_blocky-characters_20/Previews/ 에서 확인.\n" +
                "다른 캐릭터로 바꾸려면 코드의 fbxPath 변경.",
                "확인");
        }

        private static bool ReplaceModel(GameObject parent, string fbxPath, string role)
        {
            // 기존 CharacterModel 자식 제거
            var oldModel = parent.transform.Find("CharacterModel");
            if (oldModel != null)
                Undo.DestroyObjectImmediate(oldModel.gameObject);

            // 기존 캡슐 메시 비활성화
            CleanCapsuleMesh(parent);

            // .fbx asset 로드 (Unity는 fbx를 GameObject prefab으로 취급)
            var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbxAsset == null)
            {
                Debug.LogError($"[NPCReplace] fbx 없음: {fbxPath}");
                return false;
            }
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, parent.transform);
            if (inst == null) return false;
            inst.name = "CharacterModel";

            // 발이 y=0 닿도록 — Kenney 캐릭터는 pivot이 발이므로 부모 y 만큼 내림
            inst.transform.localPosition = new Vector3(0f, -parent.transform.position.y, 0f);
            // character-j (경비원)는 default forward가 반대라 자식 model에 Y180 보정
            bool isGuard = parent.GetComponent<GuardNPC>() != null;
            inst.transform.localRotation = isGuard
                ? Quaternion.Euler(0f, 180f, 0f)
                : Quaternion.identity;
            inst.transform.localScale = Vector3.one * ModelScale;

            // CharacterBobbing 자동 부착 — 부모(NPC GameObject)에
            var bob = parent.GetComponent<CharacterBobbing>();
            if (bob == null) bob = parent.AddComponent<CharacterBobbing>();
            bob.model = inst.transform;

            Undo.RegisterCreatedObjectUndo(inst, "Add Kenney character model");
            Debug.Log($"[NPCReplace] {parent.name} ({role}) → {fbxAsset.name} + Bobbing");
            return true;
        }

        private static void CleanCapsuleMesh(GameObject parent)
        {
            var mr = parent.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;
            var mf = parent.GetComponent<MeshFilter>();
            if (mf != null) mf.sharedMesh = null;
        }
    }
}
#endif
