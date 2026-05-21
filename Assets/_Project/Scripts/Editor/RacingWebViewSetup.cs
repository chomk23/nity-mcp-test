#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ForTheCompany.EditorTools
{
    /// <summary>
    /// 메뉴: "For The Company > Create Racing WebView" 클릭 시
    /// 현재 씬에 RacingWebViewCanvas + WebBrowserUIBasic 자동 구성.
    /// UnityWebBrowser 패키지가 설치되어 있어야 함 (reflection으로 타입 조회).
    /// </summary>
    public static class RacingWebViewSetup
    {
        private const string CanvasName = "RacingWebViewCanvas";
        private const string BackgroundName = "Background";
        private const string WebViewName = "WebView";

        // UWB type names — reflection으로 찾음 (실제 namespace 기준)
        private const string UWB_UI_BASIC_TYPE = "VoltstroStudios.UnityWebBrowser.WebBrowserUIBasic";
        private const string UWB_CLIENT_TYPE = "VoltstroStudios.UnityWebBrowser.Core.WebBrowserClient";
        private const string UWB_RESOLUTION_TYPE = "VoltstroStudios.UnityWebBrowser.Shared.Resolution";
        private const string UWB_ENGINE_TYPE = "VoltstroStudios.UnityWebBrowser.Core.Engines.Engine";
        private const string UWB_COMM_TYPE = "VoltstroStudios.UnityWebBrowser.Communication.CommunicationLayer";
        private const string UWB_INPUT_TYPE = "VoltstroStudios.UnityWebBrowser.Input.WebBrowserInputHandler";
        private const string CEF_ENGINE_RESOURCE_NAME = "Cef Engine Configuration";
        private const string COMM_RESOURCE_NAME = "TCP Communication Layer";
        private const string INPUT_RESOURCE_NAME = "Input System Handler";

        [MenuItem("For The Company/Create Racing WebView Canvas")]
        public static void CreateRacingWebView()
        {
            // 기존 Canvas가 있으면 제거 후 새로 만들기
            var existing = GameObject.Find(CanvasName);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("기존 Canvas 발견",
                    $"'{CanvasName}'가 이미 있습니다. 삭제하고 새로 만들까요?",
                    "다시 만들기", "취소"))
                    return;
                UnityEngine.Object.DestroyImmediate(existing);
            }

            // 1) Canvas root + CanvasGroup (alpha로 show/hide 제어 — CEF 프로세스는 항상 실행)
            var canvasGO = new GameObject(CanvasName);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            var cg = canvasGO.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            // 2) Background (반투명 어두운 패널, 전체 화면)
            var bgGO = new GameObject(BackgroundName);
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.01f, 0.1f, 0.95f);
            bgImg.raycastTarget = false; // 클릭이 WebView로 전달되도록
            var bgRT = bgImg.rectTransform;
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            // 3) WebView RawImage
            var webGO = new GameObject(WebViewName);
            webGO.transform.SetParent(canvasGO.transform, false);
            var rawImage = webGO.AddComponent<RawImage>();
            var webRT = rawImage.rectTransform;
            webRT.anchorMin = new Vector2(0.5f, 0.5f);
            webRT.anchorMax = new Vector2(0.5f, 0.5f);
            webRT.pivot = new Vector2(0.5f, 0.5f);
            webRT.sizeDelta = new Vector2(1280f, 720f);
            webRT.anchoredPosition = Vector2.zero;

            // 4) WebBrowserUIBasic 컴포넌트 추가 (reflection으로)
            Type uiBasicType = FindUwbType(UWB_UI_BASIC_TYPE);
            if (uiBasicType == null)
            {
                Debug.LogWarning("[RacingWebViewSetup] UnityWebBrowser가 설치되지 않음 — RawImage까지만 구성. " +
                                 "패키지 설치 후 메뉴를 다시 실행하면 WebBrowserUIBasic이 자동 추가됩니다.");
            }
            else
            {
                var uiComp = webGO.AddComponent(uiBasicType);
                ConfigureBrowserClient(uiComp, uiBasicType);
                EditorUtility.SetDirty(uiComp); // jsMethodsEnable 등 serialize 강제
            }

            // 5) EventSystem 보장 (UWB의 IPointerEnterHandler 동작에 필수)
            EnsureEventSystem();

            // 6) Canvas는 켜둔 채로 유지 (CEF pre-warm). CanvasGroup.alpha=0이라 보이지 않음.
            //    플레이어가 콘솔에 접근할 때쯤이면 CEF가 이미 로드되어 클릭이 즉시 반응.

            Selection.activeGameObject = canvasGO;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            string url = GetSecurityRaceUrl();
            EditorUtility.DisplayDialog("RacingWebView 생성 완료",
                $"'{CanvasName}'를 현재 씬에 추가했습니다.\n\n" +
                (uiBasicType != null
                    ? $"Initial URL: {url}\n\nCanvasGroup.alpha=0으로 숨겨져 있지만 CEF는 백그라운드에서 미리 로드됩니다."
                    : "⚠ UnityWebBrowser 패키지가 아직 미설치. Package Manager에서 설치 후\n" +
                      "이 메뉴를 다시 실행하면 WebBrowserUIBasic이 자동 추가됩니다."),
                "확인");
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null) return;
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            // New Input System 사용 중이면 InputSystemUIInputModule, 아니면 StandaloneInputModule
            var inputModuleType = FindUwbType("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
            if (inputModuleType != null)
                esGO.AddComponent(inputModuleType);
            else
                esGO.AddComponent<StandaloneInputModule>();
            Debug.Log("[RacingWebViewSetup] EventSystem 생성 (UWB 마우스 입력에 필요)");
        }

        private static Type FindUwbType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName);
                if (t != null) return t;
            }
            return null;
        }

        private static void ConfigureBrowserClient(UnityEngine.Object uiComp, Type uiType)
        {
            try
            {
                // WebBrowserUIBasic.browserClient (public field) 조회
                var clientField = uiType.GetField("browserClient",
                    BindingFlags.Public | BindingFlags.Instance);
                if (clientField == null)
                {
                    Debug.LogWarning("[RacingWebViewSetup] browserClient 필드를 찾을 수 없음");
                    return;
                }

                var client = clientField.GetValue(uiComp);
                if (client == null)
                {
                    Type clientType = FindUwbType(UWB_CLIENT_TYPE);
                    if (clientType != null)
                    {
                        client = Activator.CreateInstance(clientType);
                        clientField.SetValue(uiComp, client);
                    }
                    else
                    {
                        Debug.LogWarning("[RacingWebViewSetup] WebBrowserClient 타입을 찾을 수 없음");
                        return;
                    }
                }

                // initialUrl 설정
                var urlField = client.GetType().GetField("initialUrl",
                    BindingFlags.Public | BindingFlags.Instance);
                if (urlField != null)
                {
                    string url = GetSecurityRaceUrl();
                    urlField.SetValue(client, url);
                    Debug.Log($"[RacingWebViewSetup] Initial URL 설정: {url}");
                }

                // engine 설정 (CEF Engine Configuration ScriptableObject)
                AssignScriptableObjectField(client, "engine", UWB_ENGINE_TYPE, CEF_ENGINE_RESOURCE_NAME);

                // communicationLayer 설정 (TCP Communication Layer)
                AssignScriptableObjectField(client, "communicationLayer", UWB_COMM_TYPE, COMM_RESOURCE_NAME);

                // jsMethodManager.jsMethodsEnable = true (JS bridge 활성화)
                var jsManagerField = client.GetType().GetField("jsMethodManager",
                    BindingFlags.Public | BindingFlags.Instance);
                var jsManager = jsManagerField?.GetValue(client);
                if (jsManager != null)
                {
                    var enableField = jsManager.GetType().GetField("jsMethodsEnable",
                        BindingFlags.Public | BindingFlags.Instance);
                    enableField?.SetValue(jsManager, true);
                    Debug.Log("[RacingWebViewSetup] jsMethodsEnable=true (JS bridge 활성)");
                }

                // inputHandler 설정 (WebBrowserUIBasic에 있음, client가 아님)
                Type inputType = FindUwbType(UWB_INPUT_TYPE);
                if (inputType != null)
                {
                    var inputAsset = Resources.Load(INPUT_RESOURCE_NAME, inputType);
                    if (inputAsset == null) inputAsset = Resources.Load("Old Input Handler", inputType);
                    if (inputAsset != null)
                    {
                        var inputField = uiType.GetField("inputHandler",
                            BindingFlags.Public | BindingFlags.Instance);
                        inputField?.SetValue(uiComp, inputAsset);
                        Debug.Log($"[RacingWebViewSetup] inputHandler 할당: {inputAsset.name}");
                    }
                }

                // resolution 설정 (1000x750)
                // private field "resolution" 또는 public property "Resolution" 둘 다 시도
                Type resolutionType = FindUwbType(UWB_RESOLUTION_TYPE);
                if (resolutionType != null)
                {
                    object resObj = null;
                    // Resolution(uint width, uint height) 생성자
                    var ctor = resolutionType.GetConstructor(new[] { typeof(uint), typeof(uint) });
                    if (ctor != null)
                    {
                        resObj = ctor.Invoke(new object[] { (uint)1280, (uint)720 });
                    }
                    else
                    {
                        resObj = Activator.CreateInstance(resolutionType);
                        var w = resolutionType.GetField("Width") ?? resolutionType.GetField("width");
                        var h = resolutionType.GetField("Height") ?? resolutionType.GetField("height");
                        w?.SetValue(resObj, (uint)1280);
                        h?.SetValue(resObj, (uint)720);
                    }

                    if (resObj != null)
                    {
                        // private field에 직접 (BindingFlags.NonPublic 포함)
                        var resField = client.GetType().GetField("resolution",
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (resField != null)
                        {
                            resField.SetValue(client, resObj);
                        }
                        else
                        {
                            // property setter
                            var resProp = client.GetType().GetProperty("Resolution",
                                BindingFlags.Public | BindingFlags.Instance);
                            resProp?.SetValue(client, resObj);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[RacingWebViewSetup] BrowserClient 설정 실패: {e.Message}");
            }
        }

        private static string GetSecurityRaceUrl()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "security-race.html");
            return "file:///" + path.Replace("\\", "/");
        }

        private static void AssignScriptableObjectField(object client, string fieldName,
            string assetTypeFullName, string resourceName)
        {
            try
            {
                Type assetType = FindUwbType(assetTypeFullName);
                if (assetType == null)
                {
                    Debug.LogWarning($"[RacingWebViewSetup] 타입 '{assetTypeFullName}'을 찾을 수 없음");
                    return;
                }
                var asset = Resources.Load(resourceName, assetType);
                if (asset == null)
                {
                    Debug.LogWarning($"[RacingWebViewSetup] Resources에서 '{resourceName}' 못 찾음. " +
                                     "패키지 설치 확인.");
                    return;
                }
                var field = client.GetType().GetField(fieldName,
                    BindingFlags.Public | BindingFlags.Instance);
                if (field == null)
                {
                    Debug.LogWarning($"[RacingWebViewSetup] 필드 '{fieldName}'을 찾을 수 없음");
                    return;
                }
                field.SetValue(client, asset);
                Debug.Log($"[RacingWebViewSetup] {fieldName} 할당: {asset.name}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[RacingWebViewSetup] {fieldName} 할당 실패: {e.Message}");
            }
        }
    }
}
#endif
