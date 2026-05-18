using UnityEngine;
using UnityEngine.InputSystem;
using ForTheCompany.Managers;
using ForTheCompany.Player;

namespace ForTheCompany.Systems
{
    public class AccusationSystem : MonoBehaviour
    {
        public static AccusationSystem Instance { get; private set; }

        public bool IsMenuOpen { get; private set; }

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
            if (GameStateManager.Instance != null && GameStateManager.Instance.IsGameOver) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.qKey.wasPressedThisFrame)
                IsMenuOpen = !IsMenuOpen;

            if (kb.escapeKey.wasPressedThisFrame && IsMenuOpen)
                IsMenuOpen = false;
        }

        public void Accuse(NPCActor suspect)
        {
            if (suspect == null) return;
            IsMenuOpen = false;

            var gs = GameStateManager.Instance;
            if (gs == null || gs.IsGameOver) return;

            if (suspect.isSpy)
            {
                gs.DeclareWin($"정확히 지목했습니다 — {suspect.DisplayName} 가 스파이였습니다.");
            }
            else
            {
                string actualSpy = NPCRoster.Instance != null && NPCRoster.Instance.Spy != null
                    ? NPCRoster.Instance.Spy.DisplayName : "?";
                gs.DeclareLose(GameResult.LoseWrongAccuse,
                    $"오인 — {suspect.DisplayName}은(는) 무고했다. 실제 스파이는 {actualSpy}.");
            }
        }
    }
}
