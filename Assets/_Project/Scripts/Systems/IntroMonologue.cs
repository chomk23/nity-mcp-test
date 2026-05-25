using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ForTheCompany.Core;
using ForTheCompany.Player;

namespace ForTheCompany.Systems
{
    /// <summary>
    /// FacilityScene 첫 진입 시 오프닝 컷씬:
    /// 1) 검은 화면에서 페이드 인 (1.5초)
    /// 2) 플레이어가 시작 위치 뒤에서 시작 위치까지 자연스럽게 워크 (3.5초)
    /// 3) 동시에 보안조사관 1인칭 속마음 대화창 자동 시작
    /// 4) 컷씬 중 모든 인풋 차단 (RealtimePlayerController 비활성)
    /// 5) 대화 종료 시 인풋 정상화
    ///
    /// 한 런당 1회 (GameSession.hasShownIntroMonologue 플래그).
    /// </summary>
    public class IntroMonologue : MonoBehaviour
    {
        public static IntroMonologue Instance { get; private set; }

        // 컷씬 동안 다른 시스템이 활동 차단 여부 확인할 수 있게 정적 플래그
        public static bool IsCutsceneActive { get; private set; }

        private const float FadeDuration = 1.5f;
        private const float WalkDuration = 3.5f;
        private const float WalkBackDistance = 3f; // 뒤로 3m 이동 후 거기서 워크 시작
        private const float DialogueStartDelay = 0.4f; // 페이드 시작 후 대화 등장 시점

        private float fadeAlpha = 0f;
        private bool monologueEnded = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            EnsureSpawned();
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode) => EnsureSpawned();

        private static void EnsureSpawned()
        {
            if (SceneManager.GetActiveScene().name != "FacilityScene") return;
            if (FindFirstObjectByType<IntroMonologue>() != null) return;
            var go = new GameObject("IntroMonologue");
            go.AddComponent<IntroMonologue>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void Start()
        {
            var s = GameSession.Instance;
            if (s != null && s.hasShownIntroMonologue) return;
            StartCoroutine(RunCutscene());
        }

        private IEnumerator RunCutscene()
        {
            // PlayerInteractor가 spawn될 때까지 대기 (최대 1초)
            float waitT = 0f;
            while (PlayerInteractor.Instance == null && waitT < 1f)
            {
                waitT += Time.deltaTime;
                yield return null;
            }

            var pi = PlayerInteractor.Instance;
            if (pi == null) yield break;

            var playerT = pi.transform;
            var cc = pi.GetComponent<CharacterController>();
            var rtpc = pi.GetComponent<RealtimePlayerController>();
            if (cc == null || rtpc == null) yield break;

            // ── 1) 컷씬 시작: 인풋 차단, 페이드 검정으로 시작 ──
            IsCutsceneActive = true;
            fadeAlpha = 1f;
            rtpc.enabled = false;

            // 시작 위치 백업, 플레이어를 뒤(-Z)로 이동 + +Z 바라보게 회전
            Vector3 startPos = playerT.position;
            Vector3 backPos = startPos + new Vector3(0f, 0f, -WalkBackDistance);

            cc.enabled = false;
            playerT.position = backPos;
            playerT.rotation = Quaternion.LookRotation(Vector3.forward);
            cc.enabled = true;

            // GameSession 플래그 set (재진입 방지)
            var s = GameSession.Instance;
            if (s != null) s.hasShownIntroMonologue = true;

            // ── 2) 페이드 인 시작 (동시에 진행) ──
            StartCoroutine(FadeInRoutine());

            // 살짝 대기 후 대화창 + 워크 동시 시작
            yield return new WaitForSeconds(DialogueStartDelay);

            // ── 3) 속마음 대화 시작 ──
            StartMonologueDialogue();

            // ── 4) 플레이어 워크 (뒤 → 시작 위치) ──
            float t = 0f;
            while (t < WalkDuration)
            {
                t += Time.deltaTime;
                float pct = Mathf.Clamp01(t / WalkDuration);
                // smoothstep — 천천히 가속/감속
                float eased = pct * pct * (3f - 2f * pct);
                Vector3 targetPos = Vector3.Lerp(backPos, startPos, eased);
                Vector3 move = targetPos - playerT.position;
                cc.Move(move);
                yield return null;
            }

            // 워크 완료 — 위치 정확히 보정
            cc.enabled = false;
            playerT.position = startPos;
            cc.enabled = true;

            // ── 5) 대화 종료 대기 ──
            while (!monologueEnded) yield return null;

            // 컷씬 종료 — 인풋 정상화
            IsCutsceneActive = false;
            rtpc.enabled = true;
            Debug.Log("[IntroMonologue] 컷씬 종료 — 플레이어 조작 활성화");
        }

        private void StartMonologueDialogue()
        {
            var ds = DialogueSystem.Instance;
            if (ds == null) { monologueEnded = true; return; }

            var lines = new[]
            {
                "...출근하자마자 시설로 직행이라니. 무슨 사건이길래.",
                "보안조사관 7년차에 이런 긴급 호출은 처음이야.",
                "차세대 보안 칩 설계도 유출이라고 했지... 외부 침입이 아니라는 뜻이군.",
                "내부에 누군가가 있다. 셋 중 한 명일 거야.",
                "일단 중앙복도의 경비원에게 가서 자세한 브리핑부터 받아봐야겠어."
            };
            ds.StartDialogue("나", lines, null, () => monologueEnded = true);
        }

        private IEnumerator FadeInRoutine()
        {
            float t = 0f;
            while (t < FadeDuration)
            {
                t += Time.unscaledDeltaTime;
                fadeAlpha = Mathf.Lerp(1f, 0f, t / FadeDuration);
                yield return null;
            }
            fadeAlpha = 0f;
        }

        // 가장 위에 그려지도록 GUI.depth 매우 음수
        private void OnGUI()
        {
            if (fadeAlpha < 0.01f) return;
            GUI.depth = -10000;
            var c = new Color(0f, 0f, 0f, fadeAlpha);
            UITheme.DrawRect(new Rect(0, 0, Screen.width, Screen.height), c);
        }
    }
}
