using UnityEngine;
using ForTheCompany.Core;
using ForTheCompany.Managers;

namespace ForTheCompany.Systems
{
    public class OverworldHUD : MonoBehaviour
    {
        private GUIStyle bigStyle;
        private GUIStyle midStyle;
        private GUIStyle smallStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle resultStyle;
        private GUIStyle endStyle;
        private GUIStyle btnStyle;

        private float toastUntil;

        private void OnEnable()
        {
            if (EncounterController.Instance != null)
                EncounterController.Instance.OnEncounterClosed += OnClosed;
        }

        private void OnDisable()
        {
            if (EncounterController.Instance != null)
                EncounterController.Instance.OnEncounterClosed -= OnClosed;
        }

        private void Start()
        {
            if (EncounterController.Instance != null)
            {
                EncounterController.Instance.OnEncounterClosed -= OnClosed;
                EncounterController.Instance.OnEncounterClosed += OnClosed;
            }
        }

        private void OnClosed()
        {
            toastUntil = Time.time + 2.5f;
        }

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
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 30, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.85f, 0.4f) }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18, wordWrap = true,
                normal = { textColor = Color.white }
            };
            resultStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18, wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.7f, 1f, 0.7f) }
            };
            endStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 44, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                padding = new RectOffset(12, 12, 10, 10)
            };
        }

        private void OnGUI()
        {
            Init();
            DrawTopBar();
            DrawHoverInfo();
            DrawToast();
            DrawEncounterModal();
            DrawEndScreen();
            DrawHint();
        }

        private void DrawTopBar()
        {
            var s = GameSession.Instance;
            int clues = s != null ? s.totalClues : 0;
            int cleared = s != null ? s.clearedNodeIds.Count : 0;
            int total = OverworldManager.Instance != null ? OverworldManager.Instance.nodes.Count : 0;

            GUI.Box(new Rect(10, 10, 280, 110), GUIContent.none);
            GUI.Label(new Rect(24, 18, 260, 30), "오버월드", bigStyle);
            GUI.Label(new Rect(24, 52, 260, 24), $"단서   {clues}", midStyle);
            GUI.Label(new Rect(24, 78, 260, 24), $"클리어 노드   {cleared} / {total}", midStyle);
        }

        private void DrawHoverInfo()
        {
            var hovered = OverworldManager.Instance != null ? OverworldManager.Instance.HoveredNode : null;
            if (hovered == null) return;
            if (EncounterController.Instance != null && EncounterController.Instance.IsActive) return;

            float w = 360, h = 90;
            float x = Screen.width - w - 10;
            float y = Screen.height - h - 40;
            GUI.Box(new Rect(x, y, w, h), GUIContent.none);
            string status = hovered.IsCleared ? "클리어됨" : (hovered.IsReachable ? "이동 가능 (클릭)" : "잠김");
            GUI.Label(new Rect(x + 14, y + 8, w - 28, 26), hovered.displayName, bigStyle);
            GUI.Label(new Rect(x + 14, y + 38, w - 28, 22), $"유형 {hovered.kind}", midStyle);
            GUI.Label(new Rect(x + 14, y + 60, w - 28, 22), status, smallStyle);
        }

        private void DrawToast()
        {
            if (Time.time > toastUntil) return;
            var s = GameSession.Instance;
            if (s == null) return;

            float w = 460;
            float x = (Screen.width - w) * 0.5f;
            GUI.Box(new Rect(x, 60, w, 50), GUIContent.none);
            GUI.Label(new Rect(x + 12, 70, w - 24, 30),
                $"노드 클리어 — 단서 +{s.LastEncounterRewardClues}", midStyle);
        }

        private void DrawEncounterModal()
        {
            var ec = EncounterController.Instance;
            if (ec == null || !ec.IsActive) return;

            float w = Mathf.Min(680, Screen.width - 80);
            float h = 440;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            GUI.Box(new Rect(x, y, w, h), GUIContent.none);
            GUI.Label(new Rect(x, y + 16, w, 40), ec.Title, titleStyle);
            GUI.Label(new Rect(x + 24, y + 66, w - 48, 120), ec.Body, bodyStyle);

            switch (ec.Phase)
            {
                case EncounterPhase.SkillCheck:
                    DrawSkillCheckButtons(ec, x, y, w, h);
                    break;
                case EncounterPhase.Dialogue:
                    DrawDialogueButtons(ec, x, y, w, h);
                    break;
                case EncounterPhase.Boss:
                    DrawBossButtons(ec, x, y, w, h);
                    break;
                case EncounterPhase.Resolved:
                    DrawResultPanel(ec, x, y, w, h);
                    break;
            }
        }

        private void DrawSkillCheckButtons(EncounterController ec, float x, float y, float w, float h)
        {
            GUI.Label(new Rect(x + 24, y + 200, w - 48, 26),
                $"목표 {ec.RollTarget}   |   내 보너스 +{ec.RollBonus}", midStyle);
            if (GUI.Button(new Rect(x + 24, y + h - 90, w - 48, 60), "주사위 굴리기 (1-10)", btnStyle))
                ec.Roll();
        }

        private void DrawDialogueButtons(EncounterController ec, float x, float y, float w, float h)
        {
            if (ec.DialogueChoices == null) return;
            int n = ec.DialogueChoices.Length;
            float btnH = 54;
            float gap = 8;
            float startY = y + h - (btnH + gap) * n - 16;
            for (int i = 0; i < n; i++)
            {
                if (GUI.Button(new Rect(x + 24, startY + (btnH + gap) * i, w - 48, btnH),
                        ec.DialogueChoices[i], btnStyle))
                    ec.PickDialogueChoice(i);
            }
        }

        private void DrawBossButtons(EncounterController ec, float x, float y, float w, float h)
        {
            GUI.Label(new Rect(x + 24, y + 190, w - 48, 26),
                "산업스파이를 지목하라:", midStyle);

            float btnH = 56;
            float gap = 10;
            int n = GameSession.SpyRoleNames.Length;
            float startY = y + h - (btnH + gap) * n - 16;
            for (int i = 0; i < n; i++)
            {
                if (GUI.Button(new Rect(x + 24, startY + (btnH + gap) * i, w - 48, btnH),
                        GameSession.SpyRoleNames[i], btnStyle))
                    ResolveBoss(ec, i);
            }
        }

        private void ResolveBoss(EncounterController ec, int chosenIndex)
        {
            var s = GameSession.Instance;
            if (s == null) return;
            bool correct = chosenIndex == s.spyRoleIndex;
            if (correct)
            {
                s.DeclareWin($"정답! 산업스파이는 {GameSession.SpyRoleNames[chosenIndex]} 였다.");
            }
            else
            {
                s.DeclareLose($"오인 지목. 실제 스파이는 {GameSession.SpyRoleNames[s.spyRoleIndex]} 였다.");
            }
            if (ec.ActiveNode != null && !s.clearedNodeIds.Contains(ec.ActiveNode.nodeId))
                s.clearedNodeIds.Add(ec.ActiveNode.nodeId);
            ec.Confirm();
        }

        private void DrawResultPanel(EncounterController ec, float x, float y, float w, float h)
        {
            GUI.Label(new Rect(x + 24, y + 200, w - 48, 100), ec.Result, resultStyle);
            if (GUI.Button(new Rect(x + 24, y + h - 80, w - 48, 60), "확인", btnStyle))
                ec.Confirm();
        }

        private void DrawEndScreen()
        {
            var s = GameSession.Instance;
            if (s == null || s.Outcome == RunOutcome.Ongoing) return;

            float w = Mathf.Min(720, Screen.width - 80);
            float h = 260;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;
            GUI.Box(new Rect(x, y, w, h), GUIContent.none);

            bool win = s.Outcome == RunOutcome.Win;
            var st = new GUIStyle(endStyle)
            { normal = { textColor = win ? new Color(0.6f, 1f, 0.7f) : new Color(1f, 0.5f, 0.5f) } };
            GUI.Label(new Rect(x, y + 28, w, 60), win ? "승리" : "패배", st);
            GUI.Label(new Rect(x + 24, y + 110, w - 48, 60), s.OutcomeMessage, bodyStyle);
            if (GUI.Button(new Rect(x + (w - 220) * 0.5f, y + h - 76, 220, 56), "다시 시작 (R)", btnStyle))
                Restart();
        }

        private void Update()
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null) return;
            var s = GameSession.Instance;
            if (s != null && s.Outcome != RunOutcome.Ongoing && kb.rKey.wasPressedThisFrame)
                Restart();
        }

        private void Restart()
        {
            var s = GameSession.Instance;
            if (s != null) s.StartNewRun();
            if (OverworldManager.Instance != null) OverworldManager.Instance.RefreshAll();
        }

        private void DrawHint()
        {
            GUI.Label(new Rect(10, Screen.height - 26, 700, 22),
                "마우스로 노드 클릭   |   초록 = 클리어   |   하늘 = 이동 가능   |   회색 = 잠김",
                smallStyle);
        }
    }
}
