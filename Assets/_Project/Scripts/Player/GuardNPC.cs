using UnityEngine;
using UnityEngine.SceneManagement;
using ForTheCompany.Systems;

namespace ForTheCompany.Player
{
    /// <summary>
    /// 경비원 NPC — 중앙복도에 자동 스폰. 스파이 후보가 아님 (NPCRoster 등록 X).
    /// 스토리 모드의 안내자 역할 — QuestManager 단계에 따라 다른 대사 출력하고 Briefing 단계를 다음으로 진행시킴.
    /// </summary>
    public class GuardNPC : MonoBehaviour, IInteractable
    {
        public static GuardNPC Instance { get; private set; }

        [Header("Identity")]
        public string displayName = "경비원";

        [Header("Interaction")]
        public float interactRadius = 2.5f;

        public string LastResult { get; private set; }

        // IInteractable
        public Vector3 InteractPosition => transform.position;
        public float InteractRadius => interactRadius;
        public bool CanInteract => ForTheCompany.Core.GameSession.Instance == null
            || ForTheCompany.Core.GameSession.Instance.Outcome == ForTheCompany.Core.RunOutcome.Ongoing;
        public string PromptText => $"E: {displayName}과 대화";

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
            if (SceneManager.GetActiveScene().name != "FacilityScene") return;
            if (FindFirstObjectByType<GuardNPC>() != null) return;

            // 중앙복도 시작 지점 근처에 경비원 배치
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "GuardNPC";
            go.transform.position = new Vector3(0f, 1.1f, 4f);
            go.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);

            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                var mpb = new MaterialPropertyBlock();
                mpb.SetColor("_BaseColor", new Color(0.3f, 0.3f, 0.85f)); // 진한 파랑 (보안)
                mr.SetPropertyBlock(mpb);
            }

            go.AddComponent<GuardNPC>();
            Debug.Log("[GuardNPC] 중앙복도에 경비원 배치 (0, 1.1, 4)");
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        public void Interact()
        {
            string msg = ResolveLine();
            Debug.Log($"[Guard] {msg}");

            // 플레이어를 향해 회전
            FacePlayer();

            var ds = DialogueSystem.Instance;
            if (ds != null)
            {
                LastResult = ""; // Toast 표시 안 함 — 대화창이 담당
                ds.StartDialogue(displayName, msg, transform,
                    () => QuestManager.Instance?.TryAdvance(QuestManager.Stage.Briefing));
            }
            else
            {
                // fallback
                LastResult = $"{displayName}: {msg}";
                QuestManager.Instance?.TryAdvance(QuestManager.Stage.Briefing);
            }
        }

        private void FacePlayer()
        {
            var p = PlayerInteractor.Instance;
            if (p == null) return;
            Vector3 toPlayer = p.transform.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude < 0.01f) return;
            transform.rotation = Quaternion.LookRotation(toPlayer.normalized);
        }

        /// <summary>현재 단계에 맞춰 다른 안내 대사</summary>
        private string ResolveLine()
        {
            var quest = QuestManager.Instance;
            if (quest == null) return "보안조사관님, 환영합니다.";

            switch (quest.CurrentStage)
            {
                case QuestManager.Stage.Briefing:
                    return
                        "보안조사관님, 어제 차세대 보안 칩 설계도가 외부로 유출됐습니다. " +
                        "용의자는 셋 — 연구원, 네트워크관리자, 시설관리자.\n" +
                        "먼저 연구실로 가서 연구원과 이야기해보세요. " +
                        "단서는 미니게임과 환경 조사로 추가 확보할 수 있습니다.";
                case QuestManager.Stage.MeetResearcher:
                    return "연구실의 연구원에게 먼저 가보세요. 서북쪽 파란 방입니다.";
                case QuestManager.Stage.RacingMission:
                    return "휴게실의 보안 레이싱 게임에서 1등을 노리세요. 남쪽 초록 방입니다.";
                case QuestManager.Stage.MeetNetworkAdmin:
                    return "다음은 서버실의 네트워크관리자와 대화하세요. 북쪽 빨간 방입니다.";
                case QuestManager.Stage.MeetFacilityManager:
                    return "마지막 용의자, 시설관리자에게 가보세요. 동쪽 노란 방입니다.";
                case QuestManager.Stage.Accusation:
                    return "이제 보안통제실의 빨간 콘솔에서 스파이를 지목할 시간입니다.";
                case QuestManager.Stage.Done:
                    return "수고하셨습니다.";
            }
            return "";
        }
    }
}
