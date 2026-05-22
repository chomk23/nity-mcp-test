using UnityEngine;
using ForTheCompany.Core;

namespace ForTheCompany.Player
{
    public class RacingConsole : MonoBehaviour, IInteractable
    {
        public float interactRadius = 2.5f;

        public Vector3 InteractPosition => transform.position;
        public float InteractRadius => interactRadius;

        public bool CanInteract
        {
            get
            {
                var rmc = ForTheCompany.Systems.RacingMissionController.Instance;
                if (rmc == null) return false;
                if (rmc.HasCompleted) return false;
                if (rmc.IsOpen) return false;
                var s = GameSession.Instance;
                if (s != null && s.Outcome != RunOutcome.Ongoing) return false;
                return true;
            }
        }

        public string PromptText
        {
            get
            {
                var rmc = ForTheCompany.Systems.RacingMissionController.Instance;
                if (rmc != null && rmc.HasCompleted) return "이미 클리어 (보안 레이싱)";
                return "Space: 보안 레이싱 시작 (60초)";
            }
        }

        public void Interact()
        {
            ForTheCompany.Systems.RacingMissionController.Instance?.Launch();
        }
    }
}
