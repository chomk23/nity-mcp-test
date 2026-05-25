using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ForTheCompany.Core;

namespace ForTheCompany.Systems
{
    /// <summary>
    /// FacilityScene 첫 진입 시 보안조사관(플레이어)의 속마음 대화를 자동 트리거.
    /// 대화창은 기존 DialogueSystem 그대로 사용 (speaker = "나").
    /// 경비원과의 첫 대화 전에 상황 몰입을 만들기 위한 짧은 인트로.
    /// GameSession.hasShownIntroMonologue 플래그로 한 런(StartNewRun)당 1회만 트리거.
    /// </summary>
    public class IntroMonologue : MonoBehaviour
    {
        public static IntroMonologue Instance { get; private set; }

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
            // 이미 이번 런에서 봤으면 스킵
            if (s != null && s.hasShownIntroMonologue) return;

            StartCoroutine(StartMonologueAfterDelay());
        }

        private IEnumerator StartMonologueAfterDelay()
        {
            // 씬 안정화 대기 (Camera 초기화, NPC 스폰 등)
            yield return new WaitForSeconds(0.8f);

            var ds = DialogueSystem.Instance;
            if (ds == null) yield break;
            // 다른 대화가 이미 떠있으면 무리 안 함
            if (ds.IsActive) yield break;

            var lines = new[]
            {
                "...출근하자마자 시설로 직행이라니. 무슨 사건이길래.",
                "보안조사관 7년차에 이런 긴급 호출은 처음이야.",
                "차세대 보안 칩 설계도 유출이라고 했지... 외부 침입이 아니라는 뜻이군.",
                "내부에 누군가가 있다. 셋 중 한 명일 거야.",
                "일단 중앙복도의 경비원에게 가서 자세한 브리핑부터 받아봐야겠어."
            };

            // speaker = "나" (보안조사관 1인칭 속마음)
            ds.StartDialogue("나", lines, null, OnMonologueEnded);

            var s = GameSession.Instance;
            if (s != null) s.hasShownIntroMonologue = true;
        }

        private void OnMonologueEnded()
        {
            Debug.Log("[IntroMonologue] 인트로 속마음 종료 — 플레이어가 경비원에게 갈 차례");
        }
    }
}
