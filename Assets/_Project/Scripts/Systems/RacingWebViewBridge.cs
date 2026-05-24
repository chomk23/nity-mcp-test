using UnityEngine;
using UnityEngine.SceneManagement;

namespace ForTheCompany.Systems
{
    /// <summary>
    /// UnityWebBrowser Canvas의 표시/숨김을 CanvasGroup.alpha로 제어한다.
    /// Canvas는 항상 활성 상태(CEF 프로세스 유지) — show=alpha 1, hide=alpha 0.
    /// 이로써 플레이어가 콘솔에 도착할 때쯤이면 CEF가 이미 로드되어 PRESS START가 즉시 반응.
    /// 씬에 "RacingWebViewCanvas"가 없으면 외부 브라우저(Application.OpenURL)로 폴백.
    /// </summary>
    public class RacingWebViewBridge : MonoBehaviour
    {
        public static RacingWebViewBridge Instance { get; private set; }

        [Tooltip("RawImage + WebBrowserUIBasic 컴포넌트가 붙은 Canvas root. 비워두면 자동으로 'RacingWebViewCanvas' 이름의 GameObject를 찾는다.")]
        public GameObject canvasRoot;

        private CanvasGroup canvasGroup;
        private object cachedClient; // WebBrowserClient (reflection)

        public bool IsAvailable => canvasRoot != null;
        public bool IsShowing { get; private set; }

        private bool jsRegistered;

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
            // FacilityScene 이외 씬에서는 WebView Canvas가 없으므로 스킵
            if (SceneManager.GetActiveScene().name != "FacilityScene") return;
            if (FindFirstObjectByType<RacingWebViewBridge>() != null) return;
            var go = new GameObject("RacingWebViewBridge");
            go.AddComponent<RacingWebViewBridge>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;

            if (canvasRoot == null)
            {
                var allCanvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
                foreach (var c in allCanvases)
                {
                    if (c != null && c.gameObject.name == "RacingWebViewCanvas")
                    {
                        canvasRoot = c.gameObject;
                        break;
                    }
                }
            }

            if (canvasRoot != null)
            {
                // 항상 active 유지 (CEF pre-warm), CanvasGroup으로 시각적/입력 차단
                canvasRoot.SetActive(true);
                canvasGroup = canvasRoot.GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = canvasRoot.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                IsShowing = false;
                Debug.Log("[RacingWebView] Canvas 발견 — CEF 백그라운드 로드 시작");
            }
            else
            {
                Debug.LogWarning("[RacingWebView] 'RacingWebViewCanvas' GameObject를 못 찾음 — 외부 브라우저 폴백");
            }
        }

        /// <summary>
        /// Editor 시점에 박힌 절대경로(C:/.../Assets/StreamingAssets/security-race.html)는
        /// 빌드본에서 작동 안 함. CEF 초기화 후 런타임 streamingAssetsPath로 다시 LoadUrl.
        /// </summary>
        private void Start()
        {
            if (canvasRoot == null) return;
            StartCoroutine(EnsureCorrectUrlLoaded());
        }

        private System.Collections.IEnumerator EnsureCorrectUrlLoaded()
        {
            // CEF가 초기화될 때까지 잠시 대기 (빌드본은 첫 실행 시 1~2초 걸림)
            yield return new WaitForSeconds(1.5f);

            string url = "file:///" + System.IO.Path.Combine(
                Application.streamingAssetsPath, "security-race.html").Replace("\\", "/");
            LoadUrlOnClient(url);
        }

        private void LoadUrlOnClient(string url)
        {
            var client = GetOrFindClient();
            if (client == null)
            {
                Debug.LogWarning("[RacingWebView] WebBrowserClient를 찾을 수 없음 — LoadUrl 스킵");
                return;
            }
            try
            {
                var loadUrl = client.GetType().GetMethod("LoadUrl",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                    null, new System.Type[] { typeof(string) }, null);
                if (loadUrl != null)
                {
                    loadUrl.Invoke(client, new object[] { url });
                    Debug.Log($"[RacingWebView] 런타임 LoadUrl 호출: {url}");
                }
                else
                {
                    Debug.LogWarning("[RacingWebView] LoadUrl 메서드를 찾을 수 없음");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[RacingWebView] LoadUrl 실패: {e.Message}");
            }
        }

        public void Show()
        {
            if (canvasRoot == null)
            {
                Debug.LogWarning("[RacingWebView] RacingWebViewCanvas를 찾을 수 없음 — 외부 브라우저로 폴백");
                FallbackOpenExternal();
                return;
            }
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            IsShowing = true;
        }

        public void Hide()
        {
            if (canvasRoot == null) return;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            IsShowing = false;
            // 다음 진입 시 인트로 화면으로 시작하도록 페이지 리로드
            ReloadPage();
        }

        /// <summary>
        /// WebBrowserClient.ExecuteJs("location.reload()")로 HTML 페이지 초기화.
        /// reflection으로 ExecuteJs 메서드 찾음.
        /// </summary>
        private void ReloadPage()
        {
            var client = GetOrFindClient();
            if (client == null) return;
            try
            {
                var executeJs = client.GetType().GetMethod("ExecuteJs",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                    null, new System.Type[] { typeof(string) }, null);
                if (executeJs != null)
                {
                    executeJs.Invoke(client, new object[] { "location.reload();" });
                    Debug.Log("[RacingWebView] 페이지 리로드 (location.reload)");
                    return;
                }
                // fallback: LoadUrl
                var loadUrl = client.GetType().GetMethod("LoadUrl",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                    null, new System.Type[] { typeof(string) }, null);
                if (loadUrl != null)
                {
                    string url = "file:///" +
                        System.IO.Path.Combine(Application.streamingAssetsPath, "security-race.html")
                        .Replace("\\", "/");
                    loadUrl.Invoke(client, new object[] { url });
                    Debug.Log("[RacingWebView] 페이지 리로드 (LoadUrl)");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[RacingWebView] 페이지 리로드 실패: {e.Message}");
            }
        }

        private object GetOrFindClient()
        {
            if (cachedClient != null) return cachedClient;
            if (canvasRoot == null) return null;
            var allBehaviours = canvasRoot.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var b in allBehaviours)
            {
                if (b == null) continue;
                if (b.GetType().Name != "WebBrowserUIBasic") continue;
                var clientField = b.GetType().GetField("browserClient",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var client = clientField?.GetValue(b);
                if (client != null)
                {
                    cachedClient = client;
                    return client;
                }
            }
            return null;
        }

        /// <summary>
        /// HTML에서 uwb.ExecuteJsMethod("OnRaceFinished", rank) 호출 시 실행될 콜백 등록
        /// </summary>
        public void RegisterRaceFinishedCallback(System.Action<int> callback)
        {
            if (canvasRoot == null || callback == null) return;
            if (jsRegistered) return; // 이미 등록됨 — ArgumentException 방지
            try
            {
                var client = GetOrFindClient();
                if (client == null)
                {
                    Debug.LogWarning("[RacingWebView] WebBrowserClient를 찾을 수 없음");
                    return;
                }
                var methods = client.GetType().GetMethods();
                foreach (var m in methods)
                {
                    if (m.Name != "RegisterJsMethod") continue;
                    if (!m.IsGenericMethodDefinition) continue;
                    var ps = m.GetParameters();
                    if (ps.Length != 2) continue;
                    var generic = m.MakeGenericMethod(typeof(int));
                    generic.Invoke(client, new object[] { "OnRaceFinished", callback });
                    jsRegistered = true;
                    Debug.Log("[RacingWebView] JS 메서드 'OnRaceFinished' 등록 완료");
                    return;
                }
                Debug.LogWarning("[RacingWebView] RegisterJsMethod<T> 메서드를 찾을 수 없음");
            }
            catch (System.Exception e)
            {
                var inner = e.InnerException;
                if (inner != null)
                    Debug.LogWarning($"[RacingWebView] JS 메서드 등록 실패: {inner.GetType().Name}: {inner.Message}\n{inner.StackTrace}");
                else
                    Debug.LogWarning($"[RacingWebView] JS 메서드 등록 실패: {e.Message}");
            }
        }

        public static void FallbackOpenExternal()
        {
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "security-race.html");
            string url = "file:///" + filePath.Replace("\\", "/");
            Application.OpenURL(url);
        }
    }
}
