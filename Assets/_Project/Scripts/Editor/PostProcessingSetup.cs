#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ForTheCompany.EditorTools
{
    /// <summary>
    /// 메뉴: "For The Company → Setup Post Processing"
    /// URP Volume + Bloom/ColorAdjustments/Vignette + 카메라 PostProcessing 활성화 + Directional Light 폴리시.
    /// 현재 씬에 적용되며, MainMenuScene·FacilityScene 각각에서 한 번씩 실행하면 됨.
    /// </summary>
    public static class PostProcessingSetup
    {
        private const string ProfilePath = "Assets/_Project/Settings/FacilityPostProcessing.asset";
        private const string SettingsDir = "Assets/_Project/Settings";

        [MenuItem("For The Company/Setup Post Processing")]
        public static void Setup()
        {
            // 1) VolumeProfile asset 생성 또는 로드
            if (!Directory.Exists(SettingsDir)) Directory.CreateDirectory(SettingsDir);

            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
                Debug.Log($"[PostProcessing] Profile 생성: {ProfilePath}");
            }
            ConfigureProfile(profile);
            EditorUtility.SetDirty(profile);

            // 2) Global Volume GameObject (현재 씬)
            var volGO = GameObject.Find("Global Volume");
            if (volGO == null)
            {
                volGO = new GameObject("Global Volume");
                Debug.Log("[PostProcessing] Global Volume GameObject 생성");
            }
            var vol = volGO.GetComponent<Volume>();
            if (vol == null) vol = volGO.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.priority = 0f;
            vol.profile = profile;

            // 3) Camera에 PostProcessing 활성화
            var cam = Camera.main;
            if (cam != null)
            {
                var addData = cam.GetComponent<UniversalAdditionalCameraData>();
                if (addData == null) addData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
                addData.renderPostProcessing = true;
                addData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                Debug.Log("[PostProcessing] Camera renderPostProcessing=true, SMAA");
            }

            // 4) Directional Light 폴리시
            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            int directionalCount = 0;
            foreach (var l in lights)
            {
                if (l == null || l.type != LightType.Directional) continue;
                directionalCount++;
                l.color = new Color(1f, 0.96f, 0.88f); // 약간 따뜻한 흰색
                l.intensity = 1.0f;
                l.shadows = LightShadows.Soft;
                l.shadowStrength = 0.7f;
                // 라이트 각도 — 시설을 위에서 약간 측면으로 비추도록 (45° 정도)
                l.transform.rotation = Quaternion.Euler(50f, -40f, 0f);
            }
            Debug.Log($"[PostProcessing] Directional Light {directionalCount}개 설정");

            // 저장
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Post Processing 설정 완료",
                "현재 씬에 Global Volume + Camera + Directional Light 설정 완료.\n\n" +
                "✓ Bloom (네온/cyber 분위기)\n" +
                "✓ Color Adjustments (콘트라스트 +15, 채도 +20)\n" +
                "✓ Vignette (가장자리 살짝 어둡게)\n" +
                "✓ Soft Shadows (그림자)\n\n" +
                "MainMenuScene과 FacilityScene 양쪽 다 적용하려면 각 씬을 열고 이 메뉴를 한 번씩 실행하세요.",
                "확인");
        }

        private static void ConfigureProfile(VolumeProfile profile)
        {
            // Bloom — 라이트 부분 빛 번짐 (네온 느낌)
            if (!profile.TryGet<Bloom>(out var bloom))
                bloom = profile.Add<Bloom>(true);
            bloom.active = true;
            bloom.intensity.Override(0.6f);
            bloom.threshold.Override(1.0f);
            bloom.scatter.Override(0.65f);
            bloom.tint.Override(new Color(1f, 0.95f, 1f));

            // Color Adjustments — 콘트라스트 + 채도
            if (!profile.TryGet<ColorAdjustments>(out var color))
                color = profile.Add<ColorAdjustments>(true);
            color.active = true;
            color.postExposure.Override(0.1f);
            color.contrast.Override(15f);
            color.saturation.Override(20f);
            color.colorFilter.Override(new Color(0.96f, 0.96f, 1.0f)); // 약간 시원한 톤

            // Vignette — 가장자리 어둡게 (몰입감)
            if (!profile.TryGet<Vignette>(out var vignette))
                vignette = profile.Add<Vignette>(true);
            vignette.active = true;
            vignette.intensity.Override(0.32f);
            vignette.smoothness.Override(0.5f);
            vignette.color.Override(new Color(0.05f, 0.05f, 0.12f));

            // Tonemapping — HDR → LDR 자연스럽게 (ACES는 영화감 있음)
            if (!profile.TryGet<Tonemapping>(out var tone))
                tone = profile.Add<Tonemapping>(true);
            tone.active = true;
            tone.mode.Override(TonemappingMode.Neutral);
        }
    }
}
#endif
