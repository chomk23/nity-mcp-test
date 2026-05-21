using UnityEngine;
using ForTheCompany.Managers;
using ForTheCompany.Systems;

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
            bool firstTime = !HasBeenTalkedTo;

            string msg;
            if (firstTime)
            {
                msg = GetFirstTalkLine();
                HasBeenTalkedTo = true;
            }
            else
            {
                msg = GetRepeatTalkLine();
            }

            // NPC가 플레이어를 향해 회전 (대화 자세)
            FacePlayer();

            // DialogueSystem이 있으면 하단 대화창으로 표시, 종료 시 보상/단계 진행
            var ds = DialogueSystem.Instance;
            if (ds != null)
            {
                int capturedReward = reward;
                bool capturedFirstTime = firstTime;
                ds.StartDialogue(DisplayName, msg, transform,
                    () => ApplyTalkRewards(capturedReward, capturedFirstTime));
                Debug.Log($"[Interact] {DisplayName}: 대화 시작 (보상 +{reward}, 대화 종료 후 적용)");
                return ""; // PlayerInteractor의 Toast는 띄우지 않음
            }

            // fallback: DialogueSystem 없으면 즉시 적용
            ApplyTalkRewards(reward, firstTime);
            Debug.Log($"[Interact] {DisplayName}: {msg} (+{reward} 단서)");
            return $"{DisplayName}: {msg}\n+{reward} 단서";
        }

        /// <summary>대화 종료 후 단서/의심도/퀘스트 단계 진행 적용</summary>
        private void ApplyTalkRewards(int reward, bool firstTime)
        {
            var session = ForTheCompany.Core.GameSession.Instance;
            if (session != null)
            {
                session.totalClues += reward;
                session.LastEncounterRewardClues = reward;
            }

            // 의심도 시스템:
            // - 무고한 NPC 첫 대화 → 진짜 스파이 의심도 +2 (단서 제공)
            // - 진짜 스파이 본인 대화 → 자신 의심도 +1 (회피 발언 자체가 미묘한 의심 신호)
            var realSpy = NPCRoster.Instance != null ? NPCRoster.Instance.Spy : null;
            if (Actor != null && Actor.isSpy)
            {
                Actor.AddSuspicion(1);
            }
            else if (realSpy != null && firstTime)
            {
                realSpy.AddSuspicion(2);
            }

            // 스토리 모드: 현재 단계에 맞는 NPC라면 단계 진행
            var quest = QuestManager.Instance;
            if (quest != null && Actor != null && Actor.data != null)
                quest.TryAdvanceByRole(Actor.data.role);
        }

        private string GetFirstTalkLine()
        {
            if (Actor == null || Actor.data == null) return "별다른 단서는 없습니다.";
            int spyRole = ResolveSpyRole();
            int myRole = (int)Actor.data.role;
            return ResolveLine(myRole, spyRole, firstTime: true);
        }

        private string GetRepeatTalkLine()
        {
            if (Actor == null || Actor.data == null) return "더 이상 드릴 정보는 없습니다.";
            int spyRole = ResolveSpyRole();
            int myRole = (int)Actor.data.role;
            return ResolveLine(myRole, spyRole, firstTime: false);
        }

        private int ResolveSpyRole()
        {
            var spy = NPCRoster.Instance != null ? NPCRoster.Instance.Spy : null;
            if (spy == null || spy.data == null) return -1;
            return (int)spy.data.role;
        }

        // RoleType: Security=0, Researcher=1, NetworkAdmin=2, FacilityManager=3
        // 3 spies × 3 NPC speakers = 9 first-talk + 9 repeat-talk = 18 lines.
        // 무고한 NPC는 진짜 스파이를 미묘하게 지목, 스파이는 회피/방어.
        private string ResolveLine(int speaker, int spy, bool firstTime)
        {
            switch (spy)
            {
                case 1: // Spy = 연구원
                    switch (speaker)
                    {
                        case 1: return firstTime
                            ? "USB 접근 기록 살펴봤는데... 별 거 아니에요. 연구는 잘 되고 있습니다."
                            : "정말 별 일 없어요. 평소처럼 일하고 있을 뿐이에요.";
                        case 2: return firstTime
                            ? "어제 새벽 2시쯤 연구실 서버에 관리자 권한으로 비정상적인 접근이 있었어요."
                            : "그 접근 로그를 다시 봤더니 평문 비밀번호까지 남아있더라구요. 위험해요.";
                        case 3: return firstTime
                            ? "연구원이 새벽에 카드키를 사용한 기록이 있네요. 특이하죠."
                            : "연구원이 새벽 3시까지 연구실에서 안 나왔어요. 야근치고는 너무 길죠.";
                    }
                    break;

                case 2: // Spy = 네트워크관리자
                    switch (speaker)
                    {
                        case 1: return firstTime
                            ? "네트워크 쪽이 좀 이상해요. 어제 외부로 큰 데이터 전송이 있었던 것 같은데..."
                            : "사실 어제밤 제 USB 슬롯에 뭔가 꽂혀있던 것 같아요. 누가 데이터 빼간 거 아닌가...";
                        case 2: return firstTime
                            ? "로그는 깨끗합니다. 평소와 다를 게 없어요."
                            : "정말 평범한 하루였습니다. 의심받을 만한 일은 없어요.";
                        case 3: return firstTime
                            ? "네트워크관리자가 서버실에 평소보다 자주 들어가더라구요."
                            : "네트워크관리자 단말에서 외부 서버 IP로 트래픽이 나간 흔적을 봤어요.";
                    }
                    break;

                case 3: // Spy = 시설관리자
                    switch (speaker)
                    {
                        case 1: return firstTime
                            ? "어제 시설관리자가 CCTV를 점검한다고 잠시 끄고 다시 켰는데, 그 사이가 좀 길었어요."
                            : "카메라 끄는 동안 시설관리자가 어디 있었는지 아무도 못 봤어요.";
                        case 2: return firstTime
                            ? "시설관리자가 새벽에 보안실 카드키를 발급받았던데 무슨 일이래요?"
                            : "시설관리자 카드키로 새벽에 서버실 출입한 로그가 있어요. 권한 밖인데도요.";
                        case 3: return firstTime
                            ? "시설은 평소대로 운영 중입니다. 별 이상 없어요."
                            : "이미 다 말씀드린 것 같은데요. 정말 별 일 없습니다.";
                    }
                    break;
            }
            return firstTime ? "별다른 단서는 없습니다." : "더 이상 드릴 정보는 없습니다.";
        }

        /// <summary>대화 시작 시 NPC가 플레이어 방향으로 회전 (Y축만)</summary>
        private void FacePlayer()
        {
            var p = PlayerInteractor.Instance;
            if (p == null) return;
            Vector3 toPlayer = p.transform.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude < 0.01f) return;
            transform.rotation = Quaternion.LookRotation(toPlayer.normalized);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.4f, 0.9f, 1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
