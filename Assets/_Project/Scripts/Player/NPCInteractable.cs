using UnityEngine;

namespace ForTheCompany.Player
{
    [RequireComponent(typeof(NPCActor))]
    public class NPCInteractable : MonoBehaviour, IInteractable
    {
        [Header("Interaction")]
        public float interactRadius = 2.5f;
        public int firstTalkClueReward = 2;
        public int repeatTalkClueReward = 1;

        public bool HasBeenTalkedTo { get; private set; }
        public NPCActor Actor { get; private set; }

        public string DisplayName => Actor != null ? Actor.DisplayName : gameObject.name;

        public Vector3 InteractPosition => transform.position;
        public float InteractRadius => interactRadius;
        public bool CanInteract => ForTheCompany.Core.GameSession.Instance == null
            || ForTheCompany.Core.GameSession.Instance.Outcome == ForTheCompany.Core.RunOutcome.Ongoing;
        public string PromptText => HasBeenTalkedTo
            ? $"E: {DisplayName} 다시 대화"
            : $"E: {DisplayName}와 대화";

        public string LastResult { get; private set; }

        private void Awake()
        {
            Actor = GetComponent<NPCActor>();
        }

        void IInteractable.Interact()
        {
            LastResult = Talk();
        }

        public string Interact() => Talk();

        public string Talk()
        {
            int reward = HasBeenTalkedTo ? repeatTalkClueReward : firstTalkClueReward;

            string msg;
            if (!HasBeenTalkedTo)
            {
                msg = GetFirstTalkLine();
                HasBeenTalkedTo = true;
            }
            else
            {
                msg = GetRepeatTalkLine();
            }

            var session = ForTheCompany.Core.GameSession.Instance;
            if (session != null)
            {
                session.totalClues += reward;
                session.LastEncounterRewardClues = reward;
            }

            // Slight suspicion bump on the spy when talked to (subtle clue)
            if (Actor != null && Actor.isSpy)
                Actor.AddSuspicion(3);

            Debug.Log($"[Interact] {DisplayName}: {msg} (+{reward} 단서)");
            return $"{DisplayName}: {msg}\n+{reward} 단서";
        }

        private string GetFirstTalkLine()
        {
            if (Actor == null || Actor.data == null) return "별다른 단서는 없습니다.";

            switch ((int)Actor.data.role)
            {
                case 1: return "연구는 잘 진행 중입니다. 다만 어제밤 서버실에서 이상한 인기척이 있었어요.";
                case 2: return "로그를 살펴봤지만 평소와 큰 차이는 없네요. 그래도 USB 접근 기록이 약간 의심스러워요.";
                case 3: return "시설은 평소대로 운영 중입니다. 새벽 2시쯤 누군가 카드키를 사용한 흔적이 있어요.";
                default: return "별다른 단서는 없습니다.";
            }
        }

        private string GetRepeatTalkLine()
        {
            return "더 이상 드릴 정보는 없습니다.";
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.4f, 0.9f, 1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
