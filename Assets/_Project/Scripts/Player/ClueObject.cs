using System;
using UnityEngine;
using ForTheCompany.Core;
using ForTheCompany.Systems;

namespace ForTheCompany.Player
{
    [Serializable]
    public class ClueData
    {
        public string id;
        public string roomName;
        public Vector3 worldPos = new Vector3(0f, 0.6f, 0f);
        public string objectLabel = "단서";
        public Color color = Color.yellow;
        public string promptVerb = "조사";

        [Header("Story Lock")]
        [Tooltip("이 단서를 풀려면 도달해야 하는 최소 QuestManager.Stage 값 (Briefing=0 ~ Accusation=5). " +
                 "현재 단계가 이 값 이상이어야 활성화됨.")]
        public int stageRequired = 0;
        [Tooltip("잠금 상태 시 표시될 단계 힌트 (예: '연구원 만난 후')")]
        public string unlockHint = "";

        [Header("Quiz")]
        public string quizQuestion;
        public string[] quizOptions;
        public int correctIndex;

        [Header("Reward")]
        public string successClue;
        public int clueReward = 2;
    }

    public class ClueObject : MonoBehaviour, IInteractable
    {
        public ClueData data;
        public bool Resolved { get; private set; }

        public Vector3 InteractPosition => transform.position;
        public float InteractRadius => 2.5f;

        /// <summary>QuestManager 단계가 stageRequired 이상이면 활성화</summary>
        public bool IsUnlocked
        {
            get
            {
                if (data == null) return true;
                var quest = QuestManager.Instance;
                if (quest == null) return true;
                return (int)quest.CurrentStage >= data.stageRequired;
            }
        }

        public bool CanInteract
        {
            get
            {
                if (Resolved) return false;
                if (!IsUnlocked) return false;
                var s = GameSession.Instance;
                if (s != null && s.Outcome != RunOutcome.Ongoing) return false;
                var quiz = ForTheCompany.Systems.SecurityQuizController.Instance;
                if (quiz != null && quiz.IsOpen) return false;
                return true;
            }
        }

        public string PromptText
        {
            get
            {
                if (Resolved) return $"이미 조사됨 ({data?.objectLabel})";
                if (!IsUnlocked)
                {
                    string hint = data != null && !string.IsNullOrEmpty(data.unlockHint)
                        ? data.unlockHint : "스토리 진행 필요";
                    return $"잠금 — {hint}";
                }
                string label = data != null ? data.objectLabel : "단서";
                return $"Space: {label} — 보안 미션 시작";
            }
        }

        public void Interact()
        {
            var ctrl = ForTheCompany.Systems.SecurityQuizController.Instance;
            if (ctrl == null || Resolved || !IsUnlocked) return;
            ctrl.Open(this);
        }

        public void MarkResolved()
        {
            Resolved = true;
            UpdateMeshColor();
        }

        // 단계 변경 시 색 자동 갱신
        private bool? _wasUnlockedCache;
        private void Update()
        {
            if (Resolved) return;
            bool unlocked = IsUnlocked;
            if (_wasUnlockedCache != unlocked)
            {
                _wasUnlockedCache = unlocked;
                UpdateMeshColor();
            }
        }

        private void UpdateMeshColor()
        {
            var mr = GetComponent<MeshRenderer>();
            if (mr == null || data == null) return;
            var mpb = new MaterialPropertyBlock();

            Color c;
            if (Resolved)
                c = Color.Lerp(data.color, new Color(0.3f, 0.3f, 0.3f), 0.55f);
            else if (!IsUnlocked)
                c = new Color(0.32f, 0.32f, 0.36f); // 잠금 — 어두운 회색
            else
                c = data.color;

            mpb.SetColor("_BaseColor", c);
            mr.SetPropertyBlock(mpb);
        }
    }
}
