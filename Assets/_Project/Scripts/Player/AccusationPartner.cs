using UnityEngine;
using UnityEngine.SceneManagement;
using ForTheCompany.Core;
using ForTheCompany.Managers;
using ForTheCompany.Systems;

namespace ForTheCompany.Player
{
    /// <summary>
    /// 보안통제실 보안수사관 동료 NPC.
    /// 평소엔 일반 대화, Accusation 단계 + 단서 충분(>=3)하면 산업스파이 지목 메뉴 활성.
    /// 보안통제실 내부에서 어슬렁(patrol). Kenney character-g 모델 사용.
    /// 기존 AccusationConsole 큐브 대체 — 같은 GameObject에 자식 모델 + 이 컴포넌트.
    /// </summary>
    public class AccusationPartner : MonoBehaviour, IInteractable
    {
        public static AccusationPartner Instance { get; private set; }

        [Header("Identity")]
        public string displayName = "보안수사관 동료";

        [Header("Interaction")]
        public float interactRadius = 2.5f;
        public int minimumClues = 3;

        [Header("Patrol")]
        public Vector3 patrolCenter = new Vector3(16f, 1.2f, 11f);
        public float patrolRadius = 2.5f;
        public float patrolSpeed = 1.2f;
        public float waitTimeAtPoint = 1.5f;

        public string LastResult { get; private set; }

        // IInteractable
        public Vector3 InteractPosition => transform.position;
        public float InteractRadius => interactRadius;
        public bool CanInteract => GameSession.Instance == null
            || GameSession.Instance.Outcome == RunOutcome.Ongoing;

        public string PromptText
        {
            get
            {
                var s = GameSession.Instance;
                int c = s != null ? s.totalClues : 0;
                if (!IsStoryReady())
                    return $"Space: {displayName}과 대화";
                if (c < minimumClues)
                    return $"Space: {displayName} — 단서 {c}/{minimumClues} (지목 불가)";
                var console = GetConsole();
                bool open = console != null && console.IsMenuOpen;
                return open ? "ESC: 닫기" : "Space: 산업스파이 지목 절차 시작";
            }
        }

        private bool IsStoryReady()
        {
            var quest = QuestManager.Instance;
            if (quest == null) return false;
            return quest.CurrentStage == QuestManager.Stage.Accusation;
        }

        // Patrol state
        private Vector3 currentTarget;
        private float waitUntil;
        private bool waiting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            EnsureSpawned();
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode) => EnsureSpawned();

        /// <summary>
        /// 씬에 직접 배치된 인스턴스가 있으면 그것 유지 (Editor 메뉴로 character-g 모델 부착 가능).
        /// 없으면 빈 GameObject로 fallback spawn.
        /// </summary>
        private static void EnsureSpawned()
        {
            if (SceneManager.GetActiveScene().name != "FacilityScene") return;
            if (FindFirstObjectByType<AccusationPartner>() != null) return;
            var go = new GameObject("AccusationPartner");
            go.transform.position = new Vector3(16f, 1.2f, 11f);
            go.AddComponent<AccusationPartner>();
            Debug.Log("[AccusationPartner] 보안통제실에 동적 spawn (모델 없음 — Editor 메뉴로 character-g 부착 필요)");
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            ChoosePatrolTarget();
        }

        private void Update()
        {
            // 다른 모달/대화 떠있으면 patrol 정지
            if (IsBlockedForPatrol()) return;

            if (waiting)
            {
                if (Time.time >= waitUntil)
                {
                    waiting = false;
                    ChoosePatrolTarget();
                }
                return;
            }

            Vector3 toTarget = currentTarget - transform.position;
            toTarget.y = 0f;
            float dist = toTarget.magnitude;

            if (dist < 0.15f)
            {
                waiting = true;
                waitUntil = Time.time + waitTimeAtPoint;
                return;
            }

            Vector3 dir = toTarget.normalized;
            transform.position += dir * patrolSpeed * Time.deltaTime;

            // 부드러운 회전 (CharacterBobbing이 이 회전 기반으로 걷기 표현)
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 8f * Time.deltaTime);
        }

        private void ChoosePatrolTarget()
        {
            Vector2 r = Random.insideUnitCircle * patrolRadius;
            currentTarget = patrolCenter + new Vector3(r.x, 0f, r.y);
            currentTarget.y = transform.position.y; // 높이 유지
        }

        private bool IsBlockedForPatrol()
        {
            var ds = DialogueSystem.Instance;
            if (ds != null && ds.IsActive) return true;
            var console = GetConsole();
            if (console != null && console.IsMenuOpen) return true;
            var sqc = SecurityQuizController.Instance;
            if (sqc != null && sqc.IsOpen) return true;
            return false;
        }

        private AccusationConsole GetConsole()
        {
            // 기존 AccusationConsole이 씬에 있으면 그것의 IsMenuOpen/Accuse 위임 (기존 FacilityHUD/PauseMenu와 호환)
            return FindFirstObjectByType<AccusationConsole>(FindObjectsInactive.Include);
        }

        public void Interact()
        {
            FacePlayer();

            // 지목 단계 + 단서 충분 → 지목 메뉴 토글
            if (IsStoryReady())
            {
                var s = GameSession.Instance;
                if (s != null && s.totalClues >= minimumClues)
                {
                    var console = GetConsole();
                    if (console != null)
                    {
                        // 기존 AccusationConsole이 IsMenuOpen·Accuse 처리 → FacilityHUD 모달 자동 표시
                        console.SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
                        return;
                    }
                }
            }

            // 일반 대화 (지목 불가 상태) 또는 동료 잡담
            StartGeneralDialogue();
        }

        private void StartGeneralDialogue()
        {
            var ds = DialogueSystem.Instance;
            if (ds == null) return;

            var quest = QuestManager.Instance;
            var s = GameSession.Instance;
            int c = s != null ? s.totalClues : 0;
            int curStage = quest != null ? (int)quest.CurrentStage : 0;

            string[] lines;

            if (quest != null && quest.CurrentStage == QuestManager.Stage.Accusation
                && c >= minimumClues)
            {
                // 이미 지목 가능한 상태 (이론상 안 옴 — Interact가 메뉴 먼저 띄움)
                lines = new[]
                {
                    "준비되시면 말씀해주세요. 지목 절차 바로 시작합니다."
                };
            }
            else if (quest != null && quest.CurrentStage == QuestManager.Stage.Accusation)
            {
                // 지목 단계인데 단서 부족
                lines = new[]
                {
                    "조사관님, 단서는 충분히 모으셨나요?",
                    $"현재 수집된 단서가 {c}건이군요. 최소 {minimumClues}건은 있어야 정확한 지목이 가능합니다.",
                    "환경 조사·NPC 대화로 단서를 더 모으고 오세요. 잘못된 지목은 되돌릴 수 없으니까요."
                };
            }
            else if (curStage <= (int)QuestManager.Stage.MeetResearcher)
            {
                // 게임 초반
                lines = new[]
                {
                    "고생 많으시네요, 조사관님.",
                    "저는 이번 사건 보조로 배정된 동료입니다. 보안통제실은 제가 지키고 있을게요.",
                    "용의자 셋 다 만나보시고 단서 모으신 다음 다시 오세요.",
                    "여기 빨간 콘솔이 산업스파이 지목 시스템입니다. 모든 조사 마무리되면 안내해드릴게요."
                };
            }
            else
            {
                // 진행 중
                lines = new[]
                {
                    "수사 잘 진행되고 계신가요?",
                    $"현재까지 단서 {c}건. 좋은 페이스입니다.",
                    "나머지 용의자들도 다 만나보시고, 환경 단서까지 모으신 다음 여기로 다시 오세요."
                };
            }

            ds.StartDialogue(displayName, lines, transform, null);
        }

        private void FacePlayer()
        {
            var p = PlayerInteractor.Instance;
            if (p == null) return;
            Vector3 toPlayer = p.transform.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude < 0.01f) return;
            transform.rotation = Quaternion.LookRotation(toPlayer.normalized);
            p.transform.rotation = Quaternion.LookRotation(-toPlayer.normalized);
        }
    }
}
