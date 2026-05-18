using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using ForTheCompany.Systems;

namespace ForTheCompany.Managers
{
    public enum GameResult { None, Win, LoseWrongAccuse, LoseDataZero, LoseTimeOver }

    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        [Header("Tuning")]
        public int turnLimit = 12;

        public GameResult Result { get; private set; } = GameResult.None;
        public string ResultMessage { get; private set; } = "";
        public bool IsGameOver => Result != GameResult.None;

        public event Action OnGameOver;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (!IsGameOver)
            {
                CheckLoseConditions();
                return;
            }

            var kb = Keyboard.current;
            if (kb != null && kb.rKey.wasPressedThisFrame)
                Restart();
        }

        private void CheckLoseConditions()
        {
            if (FacilityState.Instance != null && FacilityState.Instance.dataIntegrity <= 0)
            {
                SetResult(GameResult.LoseDataZero, "데이터가 모두 유출되었습니다.");
                return;
            }

            if (TurnManager.Instance != null && TurnManager.Instance.turnNumber > turnLimit)
            {
                SetResult(GameResult.LoseTimeOver, $"제한 턴 {turnLimit}을 초과했습니다.");
            }
        }

        public void DeclareWin(string msg)
        {
            SetResult(GameResult.Win, msg);
        }

        public void DeclareLose(GameResult cause, string msg)
        {
            SetResult(cause, msg);
        }

        private void SetResult(GameResult r, string msg)
        {
            if (IsGameOver) return;
            Result = r;
            ResultMessage = msg;
            Debug.Log($"[GameOver] {r} - {msg}");
            OnGameOver?.Invoke();
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
