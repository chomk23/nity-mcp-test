using UnityEngine;
using ForTheCompany.Managers;
using ForTheCompany.Player;
using ForTheCompany.Events;
using ForTheCompany.Data;

namespace ForTheCompany.Systems
{
    public class GameHUD : MonoBehaviour
    {
        private GUIStyle bigStyle;
        private GUIStyle midStyle;
        private GUIStyle smallStyle;
        private GUIStyle eventTitleStyle;
        private GUIStyle eventBodyStyle;
        private GUIStyle eventResultStyle;
        private GUIStyle choiceButtonStyle;

        private float resultDisplayTime;
        private string lastResultShown;

        private void InitStyles()
        {
            if (bigStyle != null) return;

            bigStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            midStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                normal = { textColor = new Color(0.85f, 0.92f, 1f) }
            };
            smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                normal = { textColor = new Color(0.7f, 0.75f, 0.85f) }
            };
            eventTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.85f, 0.4f) }
            };
            eventBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = Color.white }
            };
            eventResultStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.7f, 1f, 0.7f) }
            };
            choiceButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                padding = new RectOffset(12, 12, 10, 10)
            };
        }

        private void OnGUI()
        {
            InitStyles();
            DrawStatusPanel();
            DrawFacilityPanel();
            DrawSuspectsPanel();
            DrawEventModal();
            DrawAccusationModal();
            DrawResultToast();
            DrawEndScreen();
            DrawHint();
        }

        private void DrawAccusationModal()
        {
            var acc = AccusationSystem.Instance;
            var roster = NPCRoster.Instance;
            if (acc == null || !acc.IsMenuOpen || roster == null) return;

            float w = Mathf.Min(560, Screen.width - 80);
            float headerH = 80;
            float btnH = 56;
            float gap = 8;
            float h = headerH + (btnH + gap) * roster.npcs.Count + 24;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            GUI.Box(new Rect(x, y, w, h), GUIContent.none);
            GUI.Label(new Rect(x, y + 12, w, 36), "스파이를 지목하라", eventTitleStyle);
            GUI.Label(new Rect(x + 24, y + 50, w - 48, 24),
                "정답이면 승리, 오답이면 패배. (Esc 또는 Q 로 닫기)", smallStyle);

            for (int i = 0; i < roster.npcs.Count; i++)
            {
                var n = roster.npcs[i];
                if (n == null) continue;
                float by = y + headerH + (btnH + gap) * i;
                string label = $"{n.DisplayName}   (의심도 {n.suspicion})";
                if (GUI.Button(new Rect(x + 24, by, w - 48, btnH), label, choiceButtonStyle))
                {
                    acc.Accuse(n);
                }
            }
        }

        private void DrawEndScreen()
        {
            var gs = GameStateManager.Instance;
            if (gs == null || !gs.IsGameOver) return;

            float w = Mathf.Min(720, Screen.width - 80);
            float h = 240;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            GUI.Box(new Rect(x, y, w, h), GUIContent.none);

            bool win = gs.Result == GameResult.Win;
            GUIStyle headStyle = new GUIStyle(eventTitleStyle)
            {
                fontSize = 40,
                normal = { textColor = win ? new Color(0.6f, 1f, 0.7f) : new Color(1f, 0.5f, 0.5f) }
            };
            GUI.Label(new Rect(x, y + 24, w, 60), win ? "승리" : "패배", headStyle);
            GUI.Label(new Rect(x + 24, y + 90, w - 48, 80), gs.ResultMessage, eventBodyStyle);
            GUI.Label(new Rect(x, y + h - 50, w, 28), "R 키를 눌러 다시 시작", midStyle);
        }

        private void DrawSuspectsPanel()
        {
            var roster = NPCRoster.Instance;
            if (roster == null || roster.npcs == null || roster.npcs.Count == 0) return;

            float w = 280;
            float rowH = 28;
            float headerH = 36;
            float h = headerH + rowH * roster.npcs.Count + 12;
            float x = 10;
            float y = 150;

            GUI.Box(new Rect(x, y, w, h), GUIContent.none);
            GUI.Label(new Rect(x + 14, y + 6, w - 28, 30), "용의자 의심도", bigStyle);

            for (int i = 0; i < roster.npcs.Count; i++)
            {
                var n = roster.npcs[i];
                if (n == null) continue;

                float ry = y + headerH + rowH * i;
                Color tint = n.data != null ? n.data.displayColor : Color.white;
                GUI.color = tint;
                GUI.Label(new Rect(x + 14, ry, 130, rowH), n.DisplayName, midStyle);
                GUI.color = Color.white;
                GUI.Label(new Rect(x + w - 70, ry, 60, rowH), $"{n.suspicion}", midStyle);
            }
        }

        private void DrawStatusPanel()
        {
            var tm = TurnManager.Instance;
            if (tm == null) return;

            PlayerStats p = tm.CurrentPlayer;
            string playerName = (p != null && p.data != null) ? p.data.playerName : "-";
            int hp = p != null ? p.currentHP : 0;
            int maxHp = (p != null && p.data != null) ? p.data.maxHP : 0;
            int ap = p != null ? p.currentAP : 0;
            int maxAp = (p != null && p.data != null) ? p.data.maxAP : 0;

            GUI.Box(new Rect(10, 10, 280, 130), GUIContent.none);
            GUI.Label(new Rect(24, 18, 260, 36), $"Turn {tm.turnNumber}", bigStyle);
            GUI.Label(new Rect(24, 56, 260, 28), $"{playerName}", midStyle);
            GUI.Label(new Rect(24, 84, 260, 24), $"HP  {hp} / {maxHp}", midStyle);
            GUI.Label(new Rect(24, 108, 260, 24), $"AP  {ap} / {maxAp}", midStyle);
        }

        private void DrawFacilityPanel()
        {
            var fs = FacilityState.Instance;
            if (fs == null) return;

            float x = Screen.width - 240 - 10;
            GUI.Box(new Rect(x, 10, 240, 90), GUIContent.none);
            GUI.Label(new Rect(x + 14, 18, 220, 26), "시설 상태", bigStyle);
            GUI.Label(new Rect(x + 14, 50, 220, 22), $"의심도   {fs.suspicionLevel}", midStyle);
            GUI.Label(new Rect(x + 14, 72, 220, 22), $"데이터   {fs.dataIntegrity}%", midStyle);
        }

        private void DrawEventModal()
        {
            var em = EventManager.Instance;
            if (em == null || !em.HasActive) return;

            var card = em.ActiveCard;
            float w = Mathf.Min(640, Screen.width - 80);
            float h = 360;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            GUI.Box(new Rect(x, y, w, h), GUIContent.none);
            GUI.Label(new Rect(x, y + 20, w, 40), card.title, eventTitleStyle);
            GUI.Label(new Rect(x + 24, y + 70, w - 48, 120), card.description, eventBodyStyle);

            float btnY = y + h - 90;
            float btnH = 64;
            float gap = 10;
            int n = Mathf.Max(1, card.choices.Count);
            float btnW = (w - 48 - gap * (n - 1)) / n;

            for (int i = 0; i < card.choices.Count; i++)
            {
                var c = card.choices[i];
                var r = new Rect(x + 24 + (btnW + gap) * i, btnY, btnW, btnH);
                if (GUI.Button(r, c.label, choiceButtonStyle))
                {
                    em.ResolveChoice(i);
                    resultDisplayTime = Time.time + 3.5f;
                    lastResultShown = em.LastResult;
                }
            }
        }

        private void DrawResultToast()
        {
            if (Time.time > resultDisplayTime || string.IsNullOrEmpty(lastResultShown)) return;

            float w = Mathf.Min(560, Screen.width - 80);
            float h = 64;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.18f;
            GUI.Box(new Rect(x, y, w, h), GUIContent.none);
            GUI.Label(new Rect(x + 12, y + 6, w - 24, h - 12), lastResultShown, eventResultStyle);
        }

        private void DrawHint()
        {
            GUI.Label(new Rect(10, Screen.height - 36, 900, 24),
                "WASD: 이동   |   Space: 턴 종료   |   Q: 스파이 지목   |   R: 게임 끝나면 재시작",
                smallStyle);
        }
    }
}
