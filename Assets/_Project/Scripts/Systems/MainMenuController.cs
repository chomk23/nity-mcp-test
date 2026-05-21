using UnityEngine;
using UnityEngine.SceneManagement;
using ForTheCompany.Core;

namespace ForTheCompany.Systems
{
    /// <summary>
    /// MainMenuScene 컨트롤러 — OnGUI 메뉴 + 인트로 컷씬.
    /// 흐름: Menu → (시작) → Intro 1 → Intro 2 → Intro 3 → FacilityScene 로드
    /// 스파이 정체는 절대 노출하지 않음 (혐의자만 익명으로 안내).
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        public string facilitySceneName = "FacilityScene";

        private enum Phase { Menu, Intro }
        private Phase phase = Phase.Menu;
        private int introIndex; // 0..slides.Length-1

        private static readonly (string title, string body)[] introSlides =
        {
            (
                "기밀 보고서",
                "— 대기업 연구시설 'For The Company' —\n\n" +
                "지난 주, 차세대 보안 칩 설계도가 외부로 유출되었다.\n" +
                "내부 감사 결과 — 시설 안의 누군가가 정보를 빼돌리고 있다."
            ),
            (
                "용의자 3명",
                "당신이 조사할 인물:\n\n" +
                "  · 연구원 — 칩 설계의 핵심 멤버\n" +
                "  · 네트워크관리자 — 외부 통신 권한 보유\n" +
                "  · 시설관리자 — 출입 통제 및 카드키 관리\n\n" +
                "셋 중 한 명이 진짜 산업스파이다."
            ),
            (
                "당신의 임무",
                "시설을 자유롭게 돌아다니며 단서를 모으고,\n" +
                "보안교육 미니게임을 통해 추가 정보를 확보하라.\n\n" +
                "보안통제실의 빨간 콘솔에서 단 한 번 — 진짜 스파이를 지목하라.\n" +
                "오답이면 정보는 영원히 외부로 흘러간다."
            )
        };

        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle btnStyle;
        private GUIStyle hintStyle;
        private GUIStyle introTitleStyle;
        private GUIStyle introBodyStyle;
        private Texture2D bgTex;
        private Texture2D panelTex;
        private bool stylesReady;

        private void Awake()
        {
            if (GameSession.Instance == null)
            {
                var go = new GameObject("GameSession");
                go.AddComponent<GameSession>();
            }
        }

        private void InitStyles()
        {
            if (stylesReady) return;

            bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, new Color(0.04f, 0.02f, 0.08f, 1f));
            bgTex.Apply();

            panelTex = new Texture2D(1, 1);
            panelTex.SetPixel(0, 0, new Color(0.08f, 0.05f, 0.18f, 0.95f));
            panelTex.Apply();

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 64, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.95f, 0.85f) }
            };
            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.6f, 0.9f, 1f) }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18, wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.85f, 0.85f, 0.9f) }
            };
            btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 24, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.55f, 0.55f, 0.65f) }
            };
            introTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 36, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.85f, 0.55f) }
            };
            introBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20, wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = new Color(0.9f, 0.9f, 0.95f) }
            };

            stylesReady = true;
        }

        private void OnGUI()
        {
            InitStyles();

            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), bgTex);

            if (phase == Phase.Menu) DrawMenu();
            else DrawIntro();

            HandleKeys();
        }

        private void DrawMenu()
        {
            float w = Mathf.Min(720, Screen.width - 80);
            float h = 560;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;
            GUI.DrawTexture(new Rect(x, y, w, h), panelTex);

            GUI.Label(new Rect(x, y + 30, w, 80), "FOR THE COMPANY", titleStyle);
            GUI.Label(new Rect(x, y + 110, w, 30), "산업보안 1인 탐정", subtitleStyle);

            string intro =
                "대기업 연구시설에 산업스파이가 잠입했다.\n" +
                "당신은 보안조사관 — 단서를 모아 진짜 스파이를 정확히 지목하라.\n\n" +
                "WASD 이동 · Shift 달리기 · E 상호작용 · 휠 줌";
            GUI.Label(new Rect(x + 40, y + 160, w - 80, 160), intro, bodyStyle);

            float btnW = 320, btnH = 64, btnX = x + (w - btnW) * 0.5f;
            if (GUI.Button(new Rect(btnX, y + h - 200, btnW, btnH), "게임 시작 (Enter)", btnStyle))
                BeginIntro();
            if (GUI.Button(new Rect(btnX, y + h - 124, btnW, btnH), "종료 (ESC)", btnStyle))
                QuitGame();

            GUI.Label(new Rect(0, Screen.height - 28, Screen.width, 22),
                "v0.1 MVP · For The Company", hintStyle);
        }

        private void DrawIntro()
        {
            if (introIndex < 0 || introIndex >= introSlides.Length) return;
            var slide = introSlides[introIndex];

            float w = Mathf.Min(820, Screen.width - 80);
            float h = 520;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;
            GUI.DrawTexture(new Rect(x, y, w, h), panelTex);

            // 챕터 인디케이터
            string indicator = $"  {introIndex + 1} / {introSlides.Length}";
            GUI.Label(new Rect(x + 20, y + 16, 120, 24), indicator, hintStyle);

            // 타이틀
            GUI.Label(new Rect(x, y + 50, w, 60), slide.title, introTitleStyle);

            // 본문
            GUI.Label(new Rect(x + 60, y + 140, w - 120, h - 260), slide.body, introBodyStyle);

            // 버튼 (다음 / 스킵)
            float btnW = 200, btnH = 56, gap = 24;
            float twoW = btnW * 2 + gap;
            float bx = x + (w - twoW) * 0.5f;
            float by = y + h - 84;
            bool isLast = introIndex == introSlides.Length - 1;
            string nextLabel = isLast ? "시작 (Enter)" : "다음 (Enter)";
            if (GUI.Button(new Rect(bx, by, btnW, btnH), nextLabel, btnStyle))
                NextIntro();
            if (GUI.Button(new Rect(bx + btnW + gap, by, btnW, btnH), "스킵 (S)", btnStyle))
                LoadGameScene();

            GUI.Label(new Rect(0, Screen.height - 28, Screen.width, 22),
                "ESC: 메뉴로 돌아가기", hintStyle);
        }

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
            else // Intro
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

        public void BeginIntro()
        {
            phase = Phase.Intro;
            introIndex = 0;
        }

        public void NextIntro()
        {
            if (introIndex >= introSlides.Length - 1)
                LoadGameScene();
            else
                introIndex++;
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
