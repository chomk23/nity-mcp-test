using UnityEngine;
using ForTheCompany.Core;
using ForTheCompany.Managers;
using ForTheCompany.Systems;

namespace ForTheCompany.Player
{
    public class AccusationConsole : MonoBehaviour, IInteractable
    {
        [Header("Interaction")]
        public float interactRadius = 3f;
        public int minimumClues = 3;

        public bool IsMenuOpen { get; private set; }

        public Vector3 InteractPosition => transform.position;
        public float InteractRadius => interactRadius;

        // 완전 폐기 — AccusationPartner NPC가 모든 지목 로직 처리. 씬에 남아있어도 Awake에서 자동 삭제.
        public bool CanInteract => false;

        private void Awake()
        {
            // 빨간 콘솔 GameObject 자체를 즉시 삭제 (씬에 남아있어도 게임 시작 시 사라짐)
            Debug.Log("[AccusationConsole] deprecated — AccusationPartner NPC로 대체됨. GameObject 삭제.");
            Destroy(gameObject);
        }

        /// <summary>스토리 모드: QuestManager가 Accusation 단계여야 사용 가능</summary>
        private bool IsStoryReady()
        {
            var quest = QuestManager.Instance;
            if (quest == null) return true; // QuestManager 없으면 기존 동작
            return quest.CurrentStage == QuestManager.Stage.Accusation;
        }

        public string PromptText
        {
            get
            {
                var s = GameSession.Instance;
                int c = s != null ? s.totalClues : 0;
                if (!IsStoryReady())
                    return "Space: 지목 콘솔 — 모든 용의자 조사 후 활성화";
                if (c < minimumClues)
                    return $"Space: 지목 콘솔 — 단서 {c}/{minimumClues}";
                return IsMenuOpen ? "ESC: 닫기" : "Space: 산업스파이 지목";
            }
        }

        public void Interact()
        {
            var s = GameSession.Instance;
            if (s == null || s.Outcome != RunOutcome.Ongoing) return;
            if (!IsStoryReady()) return;
            if (s.totalClues < minimumClues) return;
            IsMenuOpen = !IsMenuOpen;
        }

        public void Close()
        {
            IsMenuOpen = false;
        }

        public void Accuse(int npcIndex)
        {
            var s = GameSession.Instance;
            var roster = NPCRoster.Instance;
            if (s == null || roster == null) return;
            if (s.Outcome != RunOutcome.Ongoing) return;
            if (npcIndex < 0 || npcIndex >= roster.npcs.Count) return;
            IsMenuOpen = false;

            var accused = roster.npcs[npcIndex];
            string chosenName = accused != null ? accused.DisplayName : "?";

            if (accused != null && accused.isSpy)
            {
                s.DeclareWin($"정확히 지목 — {chosenName}가 산업스파이였다.");
            }
            else
            {
                string actual = roster.Spy != null ? roster.Spy.DisplayName : "?";
                s.DeclareLose($"오인 — {chosenName}는 무고했다. 실제 스파이는 {actual}.");
            }
            Debug.Log($"[Accuse] chose={chosenName}, actualSpy={(roster.Spy != null ? roster.Spy.DisplayName : "?")}, result={s.Outcome}");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
