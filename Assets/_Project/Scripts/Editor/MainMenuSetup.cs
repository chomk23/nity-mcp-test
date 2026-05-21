#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ForTheCompany.Systems;
using ForTheCompany.Core;

namespace ForTheCompany.EditorTools
{
    /// <summary>
    /// 메뉴: "For The Company > Create Main Menu Scene" 클릭 시
    /// MainMenuScene.unity 자동 생성 + Camera + MainMenuController + GameSession 배치
    /// + Build Settings에 MainMenuScene 0번 / FacilityScene 1번으로 등록.
    /// </summary>
    public static class MainMenuSetup
    {
        private const string ScenePath = "Assets/_Project/Scenes/MainMenuScene.unity";
        private const string FacilityScenePath = "Assets/_Project/Scenes/FacilityScene.unity";

        [MenuItem("For The Company/Create Main Menu Scene")]
        public static void CreateMainMenuScene()
        {
            if (File.Exists(ScenePath))
            {
                if (!EditorUtility.DisplayDialog("기존 MainMenuScene 발견",
                    $"{ScenePath} 가 이미 있습니다. 덮어쓸까요?",
                    "덮어쓰기", "취소"))
                    return;
            }

            // 현재 씬 저장 묻기
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            // 새 빈 씬 생성
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 카메라 (OnGUI만 쓰지만 씬에 카메라는 있어야 경고 안 남)
            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.02f, 0.08f, 1f);
            cam.orthographic = true;
            camGO.tag = "MainCamera";

            // AudioListener (Camera에 자동으로 안 붙으면 추가)
            if (camGO.GetComponent<AudioListener>() == null)
                camGO.AddComponent<AudioListener>();

            // MainMenuController
            var ctrlGO = new GameObject("MainMenuController");
            ctrlGO.AddComponent<MainMenuController>();

            // GameSession (DontDestroyOnLoad — 시작 시 정보 보존)
            var sessionGO = new GameObject("GameSession");
            sessionGO.AddComponent<GameSession>();

            // 디렉터리 보장
            string dir = Path.GetDirectoryName(ScenePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // 씬 저장
            EditorSceneManager.SaveScene(newScene, ScenePath);
            Debug.Log($"[MainMenuSetup] MainMenuScene 저장: {ScenePath}");

            // Build Settings 갱신
            RegisterBuildScenes();

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("MainMenuScene 생성 완료",
                $"'{ScenePath}'를 생성했습니다.\n\n" +
                "Build Settings에 다음 순서로 등록:\n" +
                "  0: MainMenuScene\n" +
                "  1: FacilityScene\n\n" +
                "▶ 테스트: 이 씬에서 Play 누르거나, File → Build Settings → Build And Run.",
                "확인");
        }

        private static void RegisterBuildScenes()
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();

            // MainMenuScene 0번
            if (File.Exists(ScenePath))
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));

            // FacilityScene 1번
            if (File.Exists(FacilityScenePath))
                scenes.Add(new EditorBuildSettingsScene(FacilityScenePath, true));
            else
                Debug.LogWarning($"[MainMenuSetup] {FacilityScenePath}가 없습니다 — 메뉴에서 시작 시 씬 로드 실패할 수 있음");

            // 나머지 기존 씬도 살리되 비활성화 (보존용)
            foreach (var existing in EditorBuildSettings.scenes)
            {
                if (existing.path == ScenePath || existing.path == FacilityScenePath) continue;
                scenes.Add(new EditorBuildSettingsScene(existing.path, false));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[MainMenuSetup] Build Settings 갱신 완료");
        }
    }
}
#endif
