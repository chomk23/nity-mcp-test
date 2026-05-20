using System;
using UnityEngine;
using ForTheCompany.Core;

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

        public bool CanInteract
        {
            get
            {
                if (Resolved) return false;
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
                string label = data != null ? data.objectLabel : "단서";
                string verb = data != null ? data.promptVerb : "조사";
                return $"E: {label} — 보안 미션 시작";
            }
        }

        public void Interact()
        {
            var ctrl = ForTheCompany.Systems.SecurityQuizController.Instance;
            if (ctrl == null || Resolved) return;
            ctrl.Open(this);
        }

        public void MarkResolved()
        {
            Resolved = true;
            var mr = GetComponent<MeshRenderer>();
            if (mr == null) return;
            var mpb = new MaterialPropertyBlock();
            Color c = data != null ? data.color : Color.gray;
            c = Color.Lerp(c, new Color(0.3f, 0.3f, 0.3f), 0.55f);
            mpb.SetColor("_BaseColor", c);
            mr.SetPropertyBlock(mpb);
        }
    }
}
