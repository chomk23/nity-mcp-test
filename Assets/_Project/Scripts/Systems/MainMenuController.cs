using UnityEngine;
using UnityEngine.SceneManagement;
using ForTheCompany.Core;

namespace ForTheCompany.Systems
{
    /// <summary>
    /// MainMenuScene 컨트롤러 — SecureSense 디자인 (Mission Dossier 톤).
    /// 다크 + 네온 그린, 모노스페이스 터미널 미학.
    /// 흐름: Menu → 인트로 3장 → FacilityScene
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        public string facilitySceneName = "FacilityScene";

        private enum Phase { Menu, Intro }
        private Phase phase = Phase.Menu;
        private int introIndex;

        private static readonly (string title, string body)[] introSlides =
        {
            (
                "CLASSIFIED // BRIEFING 01",
                "대기업 연구시설 'For The Company' — 차세대 보안 칩 설계도가 외부로 유출되었습니다.\n\n" +
                "내부 감사 결과: 시설 내부 인원 중 한 명이 정보를 빼돌리고 있습니다."
            ),
            (
                "SUSPECT PROFILE // 03 TARGETS",
                "용의자 명단:\n\n" +
                "  ▸ 연구원 — 칩 설계 핵심 멤버\n" +
                "  ▸ 네트워크관리자 — 외부 통신 권한 보유\n" +
                "  ▸ 시설관리자 — 출입 통제 및 카드키 관리\n\n" +
                "셋 중 한 명이 진짜 산업스파이."
            ),
            (
                "OBJECTIVE // PRIMARY",
                "시설을 자유롭게 돌아다니며 단서 수집.\n" +
                "보안교육 미니게임으로 추가 정보 확보.\n\n" +
                "보안통제실 콘솔에서 단 한 번 — 진짜 스파이를 지목하라.\n" +
                "오답 시 정보는 영원히 외부로 유출."
            )
        };

        private void Awake()
        {
            if (GameSession.Instance == null)
            {
                var go = new GameObject("GameSession");
                go.AddComponent<GameSession>();
            }
        }

        private void OnGUI()
        {
            // 풀스크린 다크 배경 + 그리드 패턴
            UITheme.DrawGridBg(new Rect(0, 0, Screen.width, Screen.height));
            UITheme.DrawScanlines(new Rect(0, 0, Screen.width, Screen.height), 0.05f);

            if (phase == Phase.Menu) DrawMenu();
            else DrawIntro();

            HandleKeys();
        }

        // ═══════════════════ MAIN MENU ═══════════════════

        private void DrawMenu()
        {
            float w = Mathf.Min(880, Screen.width - 80);
            float h = 620;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            // 메인 패널 — 가짜 OS 윈도우
            UITheme.DrawWinBar(new Rect(x, y, w, 36), "secure-sense.exe");
            UITheme.DrawRect(new Rect(x, y + 36, w, h - 36), UITheme.Bg1);
            UITheme.DrawBorder(new Rect(x, y, w, h), UITheme.LineStrong);

            // 상단 태그 + dot
            float topY = y + 60;
            UITheme.DrawTag(new Rect(x + 32, topY, 110, 20), "// SYSTEM ONLINE", UITheme.NeonGreen);
            UITheme.DrawPulseDot(new Vector2(x + 156, topY + 10), UITheme.NeonGreen, 4f);

            // ASCII / 게임 코드명
            var codeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = UITheme.InkFaint }
            };
            GUI.Label(new Rect(x + 32, topY + 28, w - 64, 16),
                "> initializing secure-sense v0.1...", codeStyle);
            GUI.Label(new Rect(x + 32, topY + 44, w - 64, 16),
                "> classified briefing loaded. clearance level: omega", codeStyle);

            // 타이틀 (글리치 느낌으로 두 번 — 색 살짝 다르게)
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 56,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = UITheme.Ink }
            };
            var titleRect = new Rect(x, y + 130, w, 70);
            // 글리치 효과 — 시안/마젠타 살짝 오프셋
            var titleGlitchC = new GUIStyle(titleStyle) { normal = { textColor = new Color(UITheme.NeonCyan.r, UITheme.NeonCyan.g, UITheme.NeonCyan.b, 0.5f) } };
            var titleGlitchM = new GUIStyle(titleStyle) { normal = { textColor = new Color(UITheme.NeonMagenta.r, UITheme.NeonMagenta.g, UITheme.NeonMagenta.b, 0.5f) } };
            GUI.Label(new Rect(titleRect.x - 2, titleRect.y, titleRect.width, titleRect.height), "FOR THE COMPANY", titleGlitchC);
            GUI.Label(new Rect(titleRect.x + 2, titleRect.y, titleRect.width, titleRect.height), "FOR THE COMPANY", titleGlitchM);
            GUI.Label(titleRect, "FOR THE COMPANY", titleStyle);

            // 서브타이틀
            var subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = UITheme.NeonGreen }
            };
            GUI.Label(new Rect(x, y + 205, w, 24), "// SECURITY INVESTIGATION // CLASSIFIED", subStyle);

            // 인트로 박스 (Dossier 카드)
            float cardY = y + 250;
            float cardH = 150;
            UITheme.DrawCard(new Rect(x + 60, cardY, w - 120, cardH), UITheme.NeonGreen * new Color(1, 1, 1, 0.3f));

            // 카드 헤더
            UITheme.DrawRect(new Rect(x + 60, cardY, w - 120, 28), UITheme.Bg3);
            var cardHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(16, 0, 0, 0),
                normal = { textColor = UITheme.NeonGreen }
            };
            GUI.Label(new Rect(x + 60, cardY, w - 120, 28),
                "▸ MISSION BRIEF // INDUSTRIAL ESPIONAGE", cardHeaderStyle);

            // 카드 본문
            var cardBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(20, 20, 14, 14),
                normal = { textColor = UITheme.Ink }
            };
            GUI.Label(new Rect(x + 60, cardY + 28, w - 120, cardH - 28),
                "대기업 연구시설에 산업스파이가 잠입했다.\n" +
                "당신은 보안조사관 — 단서를 모아 진짜 스파이를 정확히 지목하라.",
                cardBodyStyle);

            // 버튼 영역
            float btnY = y + 430;
            float btnW = 340, btnH = 52;
            float btnX = x + (w - btnW) * 0.5f;

            if (UITheme.NeonButton(new Rect(btnX, btnY, btnW, btnH),
                "▶ 수사 시작  [엔터]", UITheme.NeonGreen))
                BeginIntro();

            if (UITheme.GhostButton(new Rect(btnX, btnY + 68, btnW, btnH - 8),
                "▣ 나가기  [ESC]"))
                QuitGame();

            // 하단 푸터 — 시스템 정보
            float footerY = y + h - 36;
            UITheme.DrawRect(new Rect(x, footerY, w, 1), UITheme.Line);
            var footerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(20, 0, 0, 0),
                normal = { textColor = UITheme.InkFaint }
            };
            GUI.Label(new Rect(x, footerY, w / 2, 36),
                "// v0.1 MVP · for the company · build " + Application.version, footerStyle);
            var footerRStyle = new GUIStyle(footerStyle)
            {
                alignment = TextAnchor.MiddleRight,
                padding = new RectOffset(0, 20, 0, 0)
            };
            GUI.Label(new Rect(x + w / 2, footerY, w / 2, 36),
                "STATUS: STANDBY ●", footerRStyle);
        }

        // ═══════════════════ INTRO SLIDES ═══════════════════

        private void DrawIntro()
        {
            if (introIndex < 0 || introIndex >= introSlides.Length) return;
            var slide = introSlides[introIndex];

            float w = Mathf.Min(920, Screen.width - 80);
            float h = 600;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            // 메인 패널
            UITheme.DrawWinBar(new Rect(x, y, w, 36),
                $"briefing-{introIndex + 1:D2}.dossier");
            UITheme.DrawRect(new Rect(x, y + 36, w, h - 36), UITheme.Bg1);
            UITheme.DrawBorder(new Rect(x, y, w, h), UITheme.LineStrong);

            // 상단 챕터 표시 + dot
            float topY = y + 60;
            UITheme.DrawTag(new Rect(x + 32, topY, 90, 20),
                $"{introIndex + 1:D2} / {introSlides.Length:D2}", UITheme.NeonCyan);
            UITheme.DrawPulseDot(new Vector2(x + 136, topY + 10), UITheme.NeonCyan, 4f);

            // 헤더 — 큰 타이틀 (네온 마젠타)
            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(32, 32, 0, 0),
                normal = { textColor = UITheme.NeonMagenta }
            };
            GUI.Label(new Rect(x, y + 100, w, 50), slide.title, headerStyle);

            // 헤더 아래 구분선
            UITheme.DrawRect(new Rect(x + 32, y + 158, w - 64, 1), UITheme.Line);

            // 본문 카드
            float bodyY = y + 180;
            float bodyH = h - 320;
            UITheme.DrawCard(new Rect(x + 32, bodyY, w - 64, bodyH));

            var bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(28, 28, 24, 24),
                normal = { textColor = UITheme.Ink }
            };
            GUI.Label(new Rect(x + 32, bodyY, w - 64, bodyH), slide.body, bodyStyle);

            // 진행 표시 (작은 dot들 — 챕터 인디케이터)
            float dotsY = y + h - 100;
            float dotSize = 8f, dotGap = 14f;
            float dotsW = introSlides.Length * dotSize + (introSlides.Length - 1) * (dotGap - dotSize);
            float dotsX = x + (w - dotsW) * 0.5f;
            for (int i = 0; i < introSlides.Length; i++)
            {
                Color c = i <= introIndex ? UITheme.NeonGreen : UITheme.Bg4;
                UITheme.DrawRect(new Rect(dotsX + i * dotGap, dotsY, dotSize, dotSize), c);
            }

            // 버튼들
            float btnY = y + h - 68;
            float btnW = 220, btnH = 44, gap = 14;
            float twoW = btnW * 2 + gap;
            float btnX = x + (w - twoW) * 0.5f;

            bool isLast = introIndex == introSlides.Length - 1;
            if (UITheme.NeonButton(new Rect(btnX, btnY, btnW, btnH),
                isLast ? "▶ DEPLOY  [ENTER]" : "▸ NEXT  [ENTER]", UITheme.NeonGreen))
                NextIntro();
            if (UITheme.GhostButton(new Rect(btnX + btnW + gap, btnY, btnW, btnH),
                "▣ SKIP  [S]"))
                LoadGameScene();

            // 하단 힌트
            var hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = UITheme.InkFaint }
            };
            GUI.Label(new Rect(0, Screen.height - 22, Screen.width, 16),
                "// ESC: ABORT MISSION  ·  S: SKIP TO DEPLOYMENT", hintStyle);
        }

        // ═══════════════════ INPUT ═══════════════════

        private void HandleKeys()
        {
            var e = Event.current;
            if (e.type != EventType.KeyDown) return;

            if (phase == Phase.Menu)
            {
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter ||
                    e.keyCode == KeyCode.Space)
                    BeginIntro();
                else if (e.keyCode == KeyCode.Escape)
                    QuitGame();
            }
            else
            {
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter ||
                    e.keyCode == KeyCode.Space)
                    NextIntro();
                else if (e.keyCode == KeyCode.S)
                    LoadGameScene();
                else if (e.keyCode == KeyCode.Escape)
                {
                    phase = Phase.Menu;
                    introIndex = 0;
                }
            }
        }

        public void BeginIntro() { phase = Phase.Intro; introIndex = 0; }
        public void NextIntro()
        {
            if (introIndex >= introSlides.Length - 1) LoadGameScene();
            else introIndex++;
        }
        public void LoadGameScene()
        {
            var s = GameSession.Instance;
            if (s != null) s.StartNewRun();
            SceneManager.LoadScene(facilitySceneName);
        }
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
