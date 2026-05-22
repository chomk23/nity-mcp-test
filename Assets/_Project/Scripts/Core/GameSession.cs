using System.Collections.Generic;
using UnityEngine;

namespace ForTheCompany.Core
{
    public enum RunOutcome { Ongoing, Win, Lose }

    public enum ClueSource { Environment, NPC, Minigame }

    [System.Serializable]
    public class ClueEntry
    {
        public string title;
        public string text;
        public ClueSource source;
        public float acquiredTime;
    }

    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }

        [Header("Run Progress")]
        public int currentNodeId = -1;
        public List<int> clearedNodeIds = new List<int>();
        public int totalClues = 0;
        public int spyRoleIndex = -1;

        public RunOutcome Outcome { get; private set; } = RunOutcome.Ongoing;
        public string OutcomeMessage { get; private set; } = "";

        [Header("Time Limit")]
        [Tooltip("총 제한 시간(초). 스토리 모드 기본 7분(420초).")]
        public float totalTime = 420f;
        public float TimeRemaining { get; private set; }
        public bool TimerActive { get; private set; }

        [Header("Player Stats")]
        public int playerHacking = 3;
        public int playerInvestigation = 3;
        public int playerSecurity = 3;

        public int LastEncounterRewardClues { get; set; }

        public List<ClueEntry> CollectedClues { get; } = new List<ClueEntry>();

        [Header("Cardkey")]
        public bool hasFacilityCardkey; // 단계 4(NetworkAdmin) 완료 시 발급 — 전력실 잠금 해제

        public static readonly string[] SpyRoleNames = { "연구원", "네트워크관리자", "시설관리자" };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartNewRun()
        {
            currentNodeId = -1;
            clearedNodeIds.Clear();
            totalClues = 0;
            spyRoleIndex = Random.Range(0, SpyRoleNames.Length);
            Outcome = RunOutcome.Ongoing;
            OutcomeMessage = "";
            LastEncounterRewardClues = 0;
            CollectedClues.Clear();
            hasFacilityCardkey = false;
            TimeRemaining = totalTime;
            TimerActive = true;
#if UNITY_EDITOR
            Debug.Log($"[Session] New run — spy role = {SpyRoleNames[spyRoleIndex]} (Editor 전용), time = {totalTime:F0}s");
#else
            Debug.Log($"[Session] New run — 스파이 무작위 배정됨, 시간 {totalTime:F0}초");
#endif
        }

        private void Update()
        {
            if (!TimerActive || Outcome != RunOutcome.Ongoing) return;
            TimeRemaining -= Time.deltaTime;
            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                TimerActive = false;
                string actualSpy = spyRoleIndex >= 0 && spyRoleIndex < SpyRoleNames.Length
                    ? SpyRoleNames[spyRoleIndex] : "?";
                DeclareLose($"시간 초과 — 산업스파이를 잡지 못했습니다. 실제 스파이는 {actualSpy}.");
            }
        }

        public void StopTimer() { TimerActive = false; }
        public void ResumeTimer() { if (Outcome == RunOutcome.Ongoing) TimerActive = true; }

        /// <summary>인벤토리에 표시될 단서 항목 추가 (출처별 분류)</summary>
        public void AddClue(string title, string text, ClueSource source)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(text)) return;
            CollectedClues.Add(new ClueEntry
            {
                title = title,
                text = text,
                source = source,
                acquiredTime = Time.time
            });
            Debug.Log($"[Clue] +1 [{source}] {title}");
        }

        public void DeclareWin(string message)
        {
            if (Outcome != RunOutcome.Ongoing) return;
            Outcome = RunOutcome.Win;
            OutcomeMessage = message;
        }

        public void DeclareLose(string message)
        {
            if (Outcome != RunOutcome.Ongoing) return;
            Outcome = RunOutcome.Lose;
            OutcomeMessage = message;
        }
    }
}
