using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ForTheCompany.Systems
{
    /// <summary>
    /// 배경 음악(BGM) 매니저 — 씬별 트랙 자동 전환 + 부드러운 페이드.
    /// 트랙 파일: Assets/_Project/Audio/Resources/BGM/{name}.mp3
    /// - main_menu (메인 메뉴 + 인트로)
    /// - gameplay (FacilityScene 게임 플레이)
    /// - ending (게임 종료 시 — Outcome 변경 시)
    /// 트랙이 없으면 무음으로 진행 (게임 정상 작동).
    /// DontDestroyOnLoad + Resources.Load 동적 로드.
    /// </summary>
    public class BgmManager : MonoBehaviour
    {
        public static BgmManager Instance { get; private set; }

        private AudioSource source;
        private AudioClip currentClip;
        private string currentTrackName;
        private Coroutine fadeRoutine;

        // 트랙 이름 (Resources/BGM/{name}.mp3)
        private const string TrackMainMenu = "BGM/main_menu";
        private const string TrackGameplay = "BGM/gameplay";
        private const string TrackEnding   = "BGM/ending";

        // 기본 BGM 볼륨 (SFX보다 약간 낮게 — 효과음을 가리지 않도록)
        // AudioListener.volume(마스터)과 곱해져 최종 출력
        private const float BgmVolume = 0.5f;
        private const float FadeDuration = 1.5f;

        // 게임 종료 상태 감지용
        private Core.RunOutcome lastOutcome = Core.RunOutcome.Ongoing;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("BgmManager");
            DontDestroyOnLoad(go);
            go.AddComponent<BgmManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;

            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.volume = 0f; // 페이드 인으로 시작
            source.spatialBlend = 0f;
            source.priority = 256; // 낮은 우선순위 (덜 중요한 트랙)
            source.ignoreListenerPause = true;

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            // 첫 씬은 BeforeSceneLoad에선 모름 — Start에서 결정
        }

        private void Start()
        {
            // 시작 씬 기준 첫 트랙 선택
            string sceneName = SceneManager.GetActiveScene().name;
            PlayTrackForScene(sceneName);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayTrackForScene(scene.name);
        }

        private void Update()
        {
            // 게임 종료 상태(승/패) 감지 → 엔딩 트랙으로 전환
            var s = Core.GameSession.Instance;
            if (s != null && s.Outcome != lastOutcome)
            {
                lastOutcome = s.Outcome;
                if (s.Outcome != Core.RunOutcome.Ongoing)
                    Play(TrackEnding);
            }
        }

        private void PlayTrackForScene(string sceneName)
        {
            if (sceneName == "MainMenuScene")
            {
                lastOutcome = Core.RunOutcome.Ongoing; // 메뉴 돌아오면 리셋
                Play(TrackMainMenu);
            }
            else if (sceneName == "FacilityScene")
            {
                lastOutcome = Core.RunOutcome.Ongoing;
                Play(TrackGameplay);
            }
        }

        /// <summary>지정 트랙 재생 (이미 같은 트랙이면 무시). 부드러운 페이드 전환.</summary>
        public void Play(string trackName)
        {
            if (string.IsNullOrEmpty(trackName)) return;
            if (currentTrackName == trackName && source.isPlaying) return;

            var clip = Resources.Load<AudioClip>(trackName);
            if (clip == null)
            {
                // 트랙 파일 없음 — 무음으로 진행 (게임 정상 작동)
                Debug.LogWarning($"[BGM] '{trackName}' 트랙 없음 — Assets/_Project/Audio/Resources/{trackName}.mp3 필요. 무음 진행.");
                return;
            }

            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(CrossFadeTo(clip, trackName));
        }

        private IEnumerator CrossFadeTo(AudioClip newClip, string newTrackName)
        {
            // 1) 현재 재생 중이면 페이드 아웃
            if (source.isPlaying && source.volume > 0.01f)
            {
                float startVol = source.volume;
                float t = 0f;
                while (t < FadeDuration * 0.5f)
                {
                    t += Time.unscaledDeltaTime;
                    source.volume = Mathf.Lerp(startVol, 0f, t / (FadeDuration * 0.5f));
                    yield return null;
                }
            }

            // 2) 새 클립으로 교체
            source.Stop();
            source.clip = newClip;
            currentClip = newClip;
            currentTrackName = newTrackName;
            source.volume = 0f;
            source.Play();

            // 3) 페이드 인
            float t2 = 0f;
            while (t2 < FadeDuration)
            {
                t2 += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(0f, BgmVolume, t2 / FadeDuration);
                yield return null;
            }
            source.volume = BgmVolume;
            fadeRoutine = null;
            Debug.Log($"[BGM] '{newTrackName}' 재생 중");
        }

        /// <summary>BGM 즉시 정지 (페이드 X)</summary>
        public void Stop()
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            source.Stop();
            currentClip = null;
            currentTrackName = null;
        }
    }
}
