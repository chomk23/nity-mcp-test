using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using ForTheCompany.Core;
using ForTheCompany.Player;

namespace ForTheCompany.Systems
{
    /// <summary>
    /// Security Race 미니게임은 외부 HTML(StreamingAssets/security-race.html)을 브라우저로 띄우고,
    /// 사용자가 게임 완료 후 Unity로 돌아와 등수를 입력하면 단서 보상을 지급한다.
    /// </summary>
    public class RacingMissionController : MonoBehaviour
    {
        public static RacingMissionController Instance { get; private set; }

        public enum Phase { Inactive, AwaitingRank, Finished }
        public Phase CurrentPhase { get; private set; } = Phase.Inactive;
        public bool IsOpen => CurrentPhase != Phase.Inactive;
        public bool HasCompleted { get; private set; }

        // Result fields
        public int FinalRank { get; private set; }
        public int ClueReward { get; private set; }
        public string ResultMessage { get; private set; }
        public float ResultShownAt { get; private set; }

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
            // FacilityScene 이외에는 RacingConsole 큐브가 불필요
            if (SceneManager.GetActiveScene().name != "FacilityScene") return;
            if (FindFirstObjectByType<RacingMissionController>() != null) return;
            var go = new GameObject("RacingMissionController");
            var ctrl = go.AddComponent<RacingMissionController>();
            ctrl.SpawnConsole();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        /// <summary>휴게실 안에 RacingConsole GameObject 자동 배치 (시안색 캐비닛)</summary>
        private void SpawnConsole()
        {
            if (FindFirstObjectByType<RacingConsole>() != null) return;

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "RacingConsole";
            go.transform.position = new Vector3(-13f, 0.75f, 2f);
            go.transform.localScale = new Vector3(1.4f, 1.5f, 0.8f);

            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                var mpb = new MaterialPropertyBlock();
                mpb.SetColor("_BaseColor", new Color(0.1f, 0.85f, 1f));
                mr.SetPropertyBlock(mpb);
            }

            go.AddComponent<RacingConsole>();
        }

        /// <summary>WebView Canvas 또는 외부 브라우저로 HTML 띄우고 결과 대기</summary>
        public void Launch()
        {
            if (HasCompleted || CurrentPhase != Phase.Inactive) return;

            // 1순위: Unity 임베드 WebView (RacingWebViewCanvas가 씬에 있으면)
            var bridge = RacingWebViewBridge.Instance;
            if (bridge != null && bridge.IsAvailable)
            {
                bridge.Show();
                bridge.RegisterRaceFinishedCallback(OnHtmlRaceFinished);
                Debug.Log("[Race] 임베드 WebView 활성화 + JS 브릿지 등록");
            }
            else
            {
                // 폴백: 외부 브라우저
                RacingWebViewBridge.FallbackOpenExternal();
                Debug.Log("[Race] 외부 브라우저로 폴백 (WebView Canvas 미구성)");
            }

            CurrentPhase = Phase.AwaitingRank;
        }

        /// <summary>HTML에서 uwb.ExecuteJsMethod("OnRaceFinished", rank) 호출 시 실행됨</summary>
        public void OnHtmlRaceFinished(int rank)
        {
            if (CurrentPhase != Phase.AwaitingRank) return;
            // 1등만 클리어 인정
            if (rank == 1)
            {
                ReportRank(1);
                Debug.Log("[Race] HTML에서 1등 신호 — 자동 클리어");
            }
            else
            {
                // 1등 못 했으면 그냥 모달 닫고 콘솔 재사용 가능하게
                Cancel();
                Debug.Log($"[Race] HTML에서 {rank}등 신호 — 재시도 가능");
            }
        }

        /// <summary>플레이어가 게임 후 Unity로 돌아와 등수 보고 (1/2/3)</summary>
        public void ReportRank(int rank)
        {
            if (CurrentPhase != Phase.AwaitingRank) return;
            if (rank < 1 || rank > 3) return;

            FinalRank = rank;
            ClueReward = rank == 1 ? 5 : rank == 2 ? 3 : 1;
            ResultMessage = rank switch
            {
                1 => $"🥇 1등! 모든 차를 제치고 우승. +{ClueReward} 단서 획득",
                2 => $"🥈 2등! 결승선 통과. +{ClueReward} 단서 획득",
                3 => $"🥉 3등! 완주했습니다. +{ClueReward} 단서 획득",
                _ => ""
            };

            if (GameSession.Instance != null)
            {
                GameSession.Instance.totalClues += ClueReward;
                GameSession.Instance.LastEncounterRewardClues = ClueReward;
                if (rank == 1)
                {
                    GameSession.Instance.AddClue(
                        "보안 레이싱 우승",
                        "휴게실 보안 의식 평가를 1등으로 통과. 차세대 보안 칩 보호 절차를 능숙히 수행함.",
                        ClueSource.Minigame, -1, "RACE");
                }
            }

            HasCompleted = true;
            CurrentPhase = Phase.Finished;
            ResultShownAt = Time.time;
            RacingWebViewBridge.Instance?.Hide();

            // 스토리 모드: 1등이면 RacingMission 단계 진행
            if (rank == 1)
                QuestManager.Instance?.TryAdvance(QuestManager.Stage.RacingMission);

            Debug.Log($"[Race] 등수 {rank}, 보상 +{ClueReward} 단서");
        }

        /// <summary>등수 입력 취소 (보상 없음, 콘솔은 다시 사용 가능)</summary>
        public void Cancel()
        {
            if (CurrentPhase != Phase.AwaitingRank) return;
            CurrentPhase = Phase.Inactive;
            RacingWebViewBridge.Instance?.Hide();
        }

        public void Acknowledge()
        {
            if (CurrentPhase == Phase.Finished)
                CurrentPhase = Phase.Inactive;
        }
    }
}
