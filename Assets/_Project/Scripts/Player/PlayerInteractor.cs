using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ForTheCompany.Player
{
    public class PlayerInteractor : MonoBehaviour
    {
        public static PlayerInteractor Instance { get; private set; }

        [Header("Detection")]
        public float searchRadius = 4f;

        public IInteractable Nearest { get; private set; }
        public string LastInteractionResult { get; private set; }
        public float LastInteractionTime { get; private set; }

        private readonly List<IInteractable> cache = new List<IInteractable>();
        private float refreshTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            RefreshCache();
        }

        private void RefreshCache()
        {
            cache.Clear();
            var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var b in behaviours)
            {
                if (b is IInteractable i) cache.Add(i);
            }
        }

        private void Update()
        {
            refreshTimer -= Time.deltaTime;
            if (refreshTimer <= 0f)
            {
                refreshTimer = 1.5f;
                RefreshCache();
            }

            FindNearest();
            HandleInput();
        }

        private void FindNearest()
        {
            IInteractable best = null;
            float bestSqr = float.MaxValue;
            Vector3 myPos = transform.position;

            foreach (var n in cache)
            {
                if (n == null) continue;
                if (!n.CanInteract) continue;
                float sqr = (n.InteractPosition - myPos).sqrMagnitude;
                if (sqr > searchRadius * searchRadius) continue;
                if (sqr > n.InteractRadius * n.InteractRadius) continue;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = n;
                }
            }
            Nearest = best;
        }

        private void HandleInput()
        {
            if (Nearest == null) return;
            var kb = Keyboard.current;
            if (kb == null) return;
            if (!kb.eKey.wasPressedThisFrame) return;

            Nearest.Interact();

            if (Nearest is NPCInteractable npc && !string.IsNullOrEmpty(npc.LastResult))
            {
                LastInteractionResult = npc.LastResult;
                LastInteractionTime = Time.time;
            }
        }
    }
}
