using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ForTheCompany.Player;
using ForTheCompany.Events;

namespace ForTheCompany.Managers
{
    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }

        public List<PlayerStats> players = new List<PlayerStats>();
        public int currentPlayerIndex = 0;
        public int turnNumber = 1;

        public bool IsBusy { get; private set; }

        public event Action OnTurnChanged;

        public PlayerStats CurrentPlayer =>
            (players.Count > 0 && currentPlayerIndex < players.Count) ? players[currentPlayerIndex] : null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (CurrentPlayer != null) CurrentPlayer.RefillAP();
            OnTurnChanged?.Invoke();
            Debug.Log($"[Turn {turnNumber}] {CurrentPlayer?.data?.playerName} 시작.");
        }

        private void Update()
        {
            if (IsBusy) return;
            if (EventManager.Instance != null && EventManager.Instance.HasActive) return;
            if (ForTheCompany.Systems.AccusationSystem.Instance != null &&
                ForTheCompany.Systems.AccusationSystem.Instance.IsMenuOpen) return;
            if (GameStateManager.Instance != null && GameStateManager.Instance.IsGameOver) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)
                EndTurn();
        }

        public void EndTurn()
        {
            if (IsBusy) return;
            if (players.Count == 0) return;
            if (EventManager.Instance != null && EventManager.Instance.HasActive) return;

            StartCoroutine(EndTurnRoutine());
        }

        private IEnumerator EndTurnRoutine()
        {
            IsBusy = true;

            if (NPCRoster.Instance != null)
                yield return StartCoroutine(NPCRoster.Instance.RunAllNPCsTurn());

            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            if (currentPlayerIndex == 0) turnNumber++;

            if (CurrentPlayer != null) CurrentPlayer.RefillAP();

            OnTurnChanged?.Invoke();
            Debug.Log($"[Turn {turnNumber}] {CurrentPlayer?.data?.playerName} 차례.");

            EventManager.Instance?.TryTriggerRandom();

            IsBusy = false;
        }
    }
}
