using System;
using UnityEngine;
using ForTheCompany.Core;

namespace ForTheCompany.Managers
{
    public enum EncounterPhase { None, SkillCheck, Dialogue, Boss, Resolved }

    public class EncounterController : MonoBehaviour
    {
        public static EncounterController Instance { get; private set; }

        public MapNode ActiveNode { get; private set; }
        public EncounterPhase Phase { get; private set; } = EncounterPhase.None;

        public string Title { get; private set; }
        public string Body { get; private set; }
        public string Result { get; private set; }
        public int RewardClues { get; private set; }

        public int RollLast { get; private set; }
        public int RollTarget { get; private set; }
        public int RollBonus { get; private set; }
        public bool RollSuccess { get; private set; }
        public bool HasRolled { get; private set; }

        public string[] DialogueChoices { get; private set; }
        public int[] DialogueClueRewards { get; private set; }
        public string[] DialogueOutcomes { get; private set; }

        public bool IsActive => Phase != EncounterPhase.None && Phase != EncounterPhase.Resolved;

        public event Action OnEncounterOpened;
        public event Action OnEncounterClosed;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Open(MapNode node)
        {
            if (IsActive) return;
            ActiveNode = node;
            Title = node.displayName;
            Result = "";
            RewardClues = 0;
            HasRolled = false;

            switch (node.kind)
            {
                case NodeKind.Start:
                    Body = "임무 시작 지점이다. 진입만으로 단서를 얻는다.";
                    Phase = EncounterPhase.SkillCheck;
                    RollTarget = 0;
                    RollBonus = 0;
                    // Auto resolve as success
                    HasRolled = true;
                    RollSuccess = true;
                    RewardClues = 1;
                    Result = "주변 지형을 파악했다. (+1 단서)";
                    Phase = EncounterPhase.Resolved;
                    break;

                case NodeKind.SecurityPuzzle:
                    SetupSkillCheck(node);
                    Phase = EncounterPhase.SkillCheck;
                    break;

                case NodeKind.Dialogue:
                case NodeKind.Event:
                    SetupDialogue(node);
                    Phase = EncounterPhase.Dialogue;
                    break;

                case NodeKind.Boss:
                    Body = "최종 회의실. 지금까지의 단서를 모아 산업스파이를 지목하라.";
                    Phase = EncounterPhase.Boss;
                    break;
            }

            OnEncounterOpened?.Invoke();
        }

        private void SetupSkillCheck(MapNode node)
        {
            var s = GameSession.Instance;
            int hack = s != null ? s.playerHacking : 3;
            int sec = s != null ? s.playerSecurity : 3;
            int inv = s != null ? s.playerInvestigation : 3;

            switch (node.nodeId)
            {
                case 1:
                    Body = "보안 통제실 — 권한이 없는 단말에 접근해야 한다.\n[보안 +" + sec + "] 필요";
                    RollBonus = sec;
                    RollTarget = 8;
                    break;
                case 2:
                    Body = "서버실 — 암호화된 로그를 분석한다.\n[해킹 +" + hack + "] 필요";
                    RollBonus = hack;
                    RollTarget = 9;
                    break;
                case 5:
                    Body = "외곽 주차장 — 차량 출입 기록을 추적한다.\n[조사 +" + inv + "] 필요";
                    RollBonus = inv;
                    RollTarget = 8;
                    break;
                default:
                    Body = "스킬 체크 (보너스 +" + sec + ")";
                    RollBonus = sec;
                    RollTarget = 8;
                    break;
            }
        }

        private void SetupDialogue(MapNode node)
        {
            switch (node.nodeId)
            {
                case 3:
                    Body = "연구실의 분위기가 묘하게 차갑다. 어떻게 접근할까?";
                    DialogueChoices = new[] { "정공법 — 회의 요청", "압박 — 책임 추궁", "측면 — 동료에게 묻기" };
                    DialogueClueRewards = new[] { 1, 2, 1 };
                    DialogueOutcomes = new[]
                    {
                        "공식 인터뷰로 깔끔한 진술 확보. (+1 단서)",
                        "감정적 반응이 새 단서를 끌어냈지만 의심도 1 상승. (+2 단서)",
                        "동료에게서 작은 단서 (+1 단서)"
                    };
                    break;
                case 4:
                    Body = "창고에서 사라진 물품 목록을 발견했다. 누구에게 들이대볼까?";
                    DialogueChoices = new[] { "시설관리자", "연구원", "네트워크관리자" };
                    DialogueClueRewards = new[] { 2, 1, 1 };
                    DialogueOutcomes = new[]
                    {
                        "시설관리자가 누군가 야간 출입한 사실을 흘렸다. (+2 단서)",
                        "연구원은 별 관심 없다는 반응이다. (+1 단서)",
                        "네트워크관리자가 출입 카드 로그를 슬쩍 보여줬다. (+1 단서)"
                    };
                    break;
                default:
                    Body = "대화 인카운터";
                    DialogueChoices = new[] { "신중하게", "강하게", "조용히" };
                    DialogueClueRewards = new[] { 1, 2, 1 };
                    DialogueOutcomes = new[] { "단서 +1", "단서 +2", "단서 +1" };
                    break;
            }
        }

        public void Roll()
        {
            if (Phase != EncounterPhase.SkillCheck || HasRolled) return;
            int dice = UnityEngine.Random.Range(1, 11); // 1-10
            RollLast = dice + RollBonus;
            RollSuccess = RollLast >= RollTarget;
            HasRolled = true;
            if (RollSuccess)
            {
                RewardClues = 2;
                Result = $"성공! 주사위 {dice} + 보너스 {RollBonus} = {RollLast} ≥ {RollTarget}. (+2 단서)";
            }
            else
            {
                RewardClues = 0;
                Result = $"실패. 주사위 {dice} + 보너스 {RollBonus} = {RollLast} < {RollTarget}. (+0 단서)";
            }
            Phase = EncounterPhase.Resolved;
        }

        public void PickDialogueChoice(int index)
        {
            if (Phase != EncounterPhase.Dialogue) return;
            if (DialogueChoices == null || index < 0 || index >= DialogueChoices.Length) return;
            RewardClues = DialogueClueRewards[index];
            Result = DialogueOutcomes[index];
            Phase = EncounterPhase.Resolved;
        }

        public void Confirm()
        {
            if (Phase != EncounterPhase.Resolved) return;

            var s = GameSession.Instance;
            if (s != null && ActiveNode != null)
            {
                if (!s.clearedNodeIds.Contains(ActiveNode.nodeId))
                    s.clearedNodeIds.Add(ActiveNode.nodeId);
                s.totalClues += RewardClues;
                s.LastEncounterRewardClues = RewardClues;
            }

            Phase = EncounterPhase.None;
            var closedNode = ActiveNode;
            ActiveNode = null;

            if (OverworldManager.Instance != null)
                OverworldManager.Instance.RefreshAll();

            OnEncounterClosed?.Invoke();

            if (closedNode != null && closedNode.kind == NodeKind.Boss)
            {
                // Boss node clearing means accusation phase opens
                // Handled by Boss UI directly via accusation
            }
        }
    }
}
