#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ForTheCompany.EditorTools
{
    /// <summary>
    /// 메뉴: "For The Company → Setup Sci-Fi Walls"
    /// 시설의 모든 벽(외곽 Wall_*, 내부 W_*) 머터리얼을 단색 sci-fi 회색으로 교체.
    /// 텍스처 stretch 문제 없는 깔끔한 모던 톤 — URP/Lit + Metallic 0.3 + Smoothness 0.4.
    /// 벽 위치/구조는 안 건드림.
    /// </summary>
    public static class SciFiWallsSetup
    {
        private const string WallMatPath = "Assets/_Project/Settings/Mat_SciFiWall.mat";
        private const string SettingsDir = "Assets/_Project/Settings";

        [MenuItem("For The Company/Setup Sci-Fi Walls")]
        public static void Setup()
        {
            // 머터리얼이 없으면 생성, 있으면 속성 갱신
            if (!Directory.Exists(SettingsDir)) Directory.CreateDirectory(SettingsDir);

            Material wallMat = AssetDatabase.LoadAssetAtPath<Material>(WallMatPath);
            if (wallMat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                wallMat = new Material(shader);
                AssetDatabase.CreateAsset(wallMat, WallMatPath);
                Debug.Log($"[SciFiWalls] 새 머터리얼 생성: {WallMatPath}");
            }

            // 단색 sci-fi 회색 — 깔끔한 모던 톤
            ConfigureMaterial(wallMat);
            EditorUtility.SetDirty(wallMat);

            // 모든 벽 GameObject에 적용
            int outerCount = 0;
            int innerCount = 0;
            var renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
            foreach (var r in renderers)
            {
                if (r == null) continue;
                string name = r.name;
                bool isOuter = name == "Wall_North" || name == "Wall_East"
                    || name == "Wall_South" || name == "Wall_West";
                bool isInner = name.StartsWith("W_");
                if (!isOuter && !isInner) continue;

                Undo.RecordObject(r, "Change wall material");
                r.sharedMaterial = wallMat;
                EditorUtility.SetDirty(r);

                if (isOuter) outerCount++;
                else innerCount++;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Sci-Fi Walls 머터리얼 적용 완료",
                $"새 단색 sci-fi 회색 머터리얼을 {outerCount + innerCount}개 벽에 적용.\n\n" +
                "• 색상: 어두운 회색-블루 (0.18, 0.20, 0.25)\n" +
                "• 약간의 metallic + smoothness — Bloom과 어울리는 톤\n" +
                "• 텍스처 stretch 없는 깔끔한 모던 톤\n\n" +
                "다른 색감 원하면: Project 창 → Assets/_Project/Settings/Mat_SciFiWall.mat 선택 → Inspector에서 Color 조정.",
                "확인");
        }

        private static void ConfigureMaterial(Material m)
        {
            // URP/Lit 셰이더의 표준 property name
            if (m.HasProperty("_BaseColor"))
                m.SetColor("_BaseColor", new Color(0.18f, 0.20f, 0.25f, 1f));
            if (m.HasProperty("_Color"))
                m.SetColor("_Color", new Color(0.18f, 0.20f, 0.25f, 1f));
            if (m.HasProperty("_Metallic"))
                m.SetFloat("_Metallic", 0.3f);
            if (m.HasProperty("_Smoothness"))
                m.SetFloat("_Smoothness", 0.4f);
            if (m.HasProperty("_Glossiness"))
                m.SetFloat("_Glossiness", 0.4f);
        }
    }
}
#endif
