using System;
using System.Collections.Generic;
using UnityEngine;
using ForTheCompany.Managers;
using ForTheCompany.Player;
using ForTheCompany.Systems;

namespace ForTheCompany.Events
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        [Header("Card Pool")]
        public List<EventCard> cardPool = new List<EventCard>();

        [Header("Tuning")]
        [Range(0f, 1f)] public float triggerChance = 0.6f;

        public EventCard ActiveCard { get; private set; }
        public string LastResult { get; private set; }

        public event Action<EventCard> OnEventStarted;
        public event Action<EventChoice> OnEventResolved;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool HasActive => ActiveCard != null;

        public void TryTriggerRandom()
        {
            if (HasActive) return;
            if (cardPool == null || cardPool.Count == 0) return;
            if (UnityEngine.Random.value > triggerChance) return;

            int idx = UnityEngine.Random.Range(0, cardPool.Count);
            ActiveCard = cardPool[idx];
            LastResult = null;
            OnEventStarted?.Invoke(ActiveCard);
            Debug.Log($"[Event] {ActiveCard.title}");
        }

        public void ResolveChoice(int choiceIndex)
        {
            if (!HasActive) return;
            if (choiceIndex < 0 || choiceIndex >= ActiveCard.choices.Count) return;

            var choice = ActiveCard.choices[choiceIndex];
            ApplyOutcome(choice);

            LastResult = choice.resultText;
            Debug.Log($"[Event] '{choice.label}' → {choice.resultText} " +
                      $"(HP{choice.hpDelta:+#;-#;0} AP{choice.apDelta:+#;-#;0} " +
                      $"의심{choice.suspicionDelta:+#;-#;0} 데이터{choice.dataIntegrityDelta:+#;-#;0})");

            var card = ActiveCard;
            ActiveCard = null;
            OnEventResolved?.Invoke(choice);
        }

        private void ApplyOutcome(EventChoice choice)
        {
            PlayerStats p = TurnManager.Instance != null ? TurnManager.Instance.CurrentPlayer : null;
            if (p != null && p.data != null)
            {
                if (choice.hpDelta != 0)
                    p.currentHP = Mathf.Clamp(p.currentHP + choice.hpDelta, 0, p.data.maxHP);
                if (choice.apDelta != 0)
                    p.currentAP = Mathf.Clamp(p.currentAP + choice.apDelta, 0, p.data.maxAP);
            }

            if (FacilityState.Instance != null)
                FacilityState.Instance.Modify(choice.suspicionDelta, choice.dataIntegrityDelta);
        }
    }
}
