using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using ForTheCompany.Core;
using ForTheCompany.Player;
using ForTheCompany.Managers;

namespace ForTheCompany.Systems
{
    public class FacilityHUD : MonoBehaviour
    {
        private GUIStyle bigStyle;
        private GUIStyle midStyle;
        private GUIStyle smallStyle;
        private GUIStyle toastStyle;
        private GUIStyle promptStyle;
        private GUIStyle titleStyle;
        private GUIStyle endStyle;
        private GUIStyle btnStyle;
        private GUIStyle bodyStyle;

        private void Init()
        {
            if (bigStyle != null) return;
            bigStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26, fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            midStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                normal = { textColor = new Color(0.85f, 0.92f, 1f) }
            };
            smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = new Color(0.7f, 0.75f, 0.85f) }
            };
            toastStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.7f, 1f, 0.7f) }
            };
            promptStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.85f, 0.4f) }
            };
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 30, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.4f, 0.4f) }
            };
            endStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 46, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 20, padding = new RectOffset(12, 12, 12, 12)
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18, wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
        }

        private AccusationConsole FindConsole()
        {
            var all = FindObjectsByType<AccusationConsole>(FindObjectsSortMode.None);
            return all.Length > 0 ? all[0] : null;
        }

        private void OnGUI()
        {
            Init();
            DrawStatusBar();
            DrawInteractionPrompt();
            DrawToast();
            DrawAccusationModal();
            DrawEndScreen();
            DrawHint();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            var console = FindConsole();
            if (console != null && console.IsMenuOpen && kb.escapeKey.wasPressedThisFrame)
                console.Close();

            var s = GameSession.Instance;
            if (s != null && s.Outcome != RunOutcome.Ongoing && kb.rKey.wasPressedThisFrame)
                Restart();
        }

        private void Restart()
        {
            var s = GameSession.Instance;
            if (s != null) s.StartNewRun();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void DrawStatusBar()
        {
            var s = GameSession.Instance;
            int clues = s != null ? s.totalClues : 0;

            GUI.Box(new Rect(10, 10, 280, 80), GUIContent.none);
            GUI.Label(new Rect(24, 18, 260, 30), "산업스파이 조사", bigStyle);
            GUI.Label(new Rect(24, 52, 260, 24), $"단서   {clues}", midStyle);
        }

        private void DrawInteractionPrompt()
        {
            var p = PlayerInteractor.Instance;
            if (p == null || p.Nearest == null) return;

            // Hide prompt while accusation modal open
            var console = FindConsole();
            if (console != null && console.IsMenuOpen) return;

            string prompt = p.Nearest.PromptText;

            float w = 420;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height - 130;
            GUI.Box(new Rect(x, y, w, 50), GUIContent.none);
            GUI.Label(new Rect(x, y + 8, w, 36), prompt, promptStyle);
        }

        private void DrawToast()
        {
            var p = PlayerInteractor.Instance;
            if (p == null || string.IsNullOrEmpty(p.LastInteractionResult)) return;
            if (Time.time - p.LastInteractionTime > 4f) return;

            float w = Mathf.Min(640, Screen.width - 80);
            float h = 90;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.25f;
            GUI.Box(new Rect(x, y, w, h), GUIContent.none);
            GUI.Label(new Rect(x + 16, y + 12, w - 32, h - 24), p.LastInteractionResult, toastStyle);
        }

        private void DrawAccusationModal()
        {
            var console = FindConsole();
            if (console == null || !console.IsMenuOpen) return;

            float w = Mathf.Min(640, Screen.width - 80);
            float h = 400;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            GUI.Box(new Rect(x, y, w, h), GUIContent.none);
            GUI.Label(new Rect(x, y + 18, w, 40), "산업스파이 지목", titleStyle);
            GUI.Label(new Rect(x + 24, y + 70, w - 48, 60),
                "수집한 단서를 바탕으로 산업스파이를 지목하라. 정답이면 승리, 오답이면 패배.",
                bodyStyle);

            var roster = NPCRoster.Instance;
            int n = roster != null ? roster.npcs.Count : 0;
            float btnH = 60;
            float gap = 10;
            float startY = y + h - (btnH + gap) * Mathf.Max(1, n) - 16;
            for (int i = 0; i < n; i++)
            {
                var r = new Rect(x + 24, startY + (btnH + gap) * i, w - 48, btnH);
                string label = roster.npcs[i] != null ? roster.npcs[i].DisplayName : "?";
                if (GUI.Button(r, label, btnStyle))
                    console.Accuse(i);
            }
        }

        private void DrawEndScreen()
        {
            var s = GameSession.Instance;
            if (s == null || s.Outcome == RunOutcome.Ongoing) return;

            float w = Mathf.Min(720, Screen.width - 80);
            float h = 280;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;
            GUI.Box(new Rect(x, y, w, h), GUIContent.none);

            bool win = s.Outcome == RunOutcome.Win;
            var st = new GUIStyle(endStyle)
            { normal = { textColor = win ? new Color(0.6f, 1f, 0.7f) : new Color(1f, 0.5f, 0.5f) } };
            GUI.Label(new Rect(x, y + 30, w, 60), win ? "승리" : "패배", st);
            GUI.Label(new Rect(x + 24, y + 110, w - 48, 80), s.OutcomeMessage, bodyStyle);

            if (GUI.Button(new Rect(x + (w - 240) * 0.5f, y + h - 76, 240, 56), "다시 시작 (R)", btnStyle))
                Restart();
        }

        private void DrawHint()
        {
            GUI.Label(new Rect(10, Screen.height - 26, 1100, 22),
                "WASD: 이동   |   Shift: 달리기   |   마우스 휠: 줌   |   E: 상호작용 (대화/지목)   |   R: 게임 끝나면 재시작",
                smallStyle);
        }
    }
}
