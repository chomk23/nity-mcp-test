using UnityEngine;

namespace ForTheCompany.Player
{
    public interface IInteractable
    {
        Vector3 InteractPosition { get; }
        float InteractRadius { get; }
        bool CanInteract { get; }
        string PromptText { get; }
        void Interact();
    }
}
