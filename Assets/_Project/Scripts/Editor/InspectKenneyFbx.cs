#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ForTheCompany.EditorTools
{
    /// <summary>
    /// 메뉴: "For The Company → Inspect Kenney FBX Structure"
    /// Kenney character-q.fbx의 자식 GameObject 트리를 콘솔에 출력 — procedural 팔다리 회전 가능 여부 확인용.
    /// </summary>
    public static class InspectKenneyFbx
    {
        private const string SamplePath =
            "Assets/kenney_blocky-characters_20/Models/FBX format/character-q.fbx";

        [MenuItem("For The Company/Inspect Kenney FBX Structure")]
        public static void Inspect()
        {
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(SamplePath);
            if (fbx == null)
            {
                Debug.LogError($"[Inspect] FBX 없음: {SamplePath}");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"=== {fbx.name} 자식 구조 ===");
            PrintHierarchy(fbx.transform, 0, sb);
            Debug.Log(sb.ToString());

            EditorUtility.DisplayDialog("Inspect 완료",
                "콘솔(Console 창)에 character-q.fbx 자식 구조를 출력했습니다.\n\n" +
                "그 내용을 복사해서 보내주세요. 자식 부품 이름(Head, Arm_L 등)을 알아야\n" +
                "procedural 팔다리 회전 코드를 짤 수 있습니다.",
                "확인");
        }

        private static void PrintHierarchy(Transform t, int depth, StringBuilder sb)
        {
            sb.AppendLine(new string(' ', depth * 2) + "- " + t.name);
            foreach (Transform child in t)
                PrintHierarchy(child, depth + 1, sb);
        }
    }
}
#endif
