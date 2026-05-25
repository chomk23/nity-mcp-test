using UnityEngine;
using UnityEngine.InputSystem;

namespace ForTheCompany.Systems
{
    /// <summary>
    /// SecureSense 디자인 시스템 — 다크 + 네온 터미널 미학.
    /// Anthropic Claude Design (security-education-ui-remix) 기반.
    /// 색상 hex를 Unity Color로 변환 + 공용 GUIStyle 캐시.
    /// </summary>
    public static class UITheme
    {
        // ───── 배경 (다크 그라데이션) ─────
        public static readonly Color Bg0 = Hex("#050608"); // 가장 어두움
        public static readonly Color Bg1 = Hex("#0a0d12");
        public static readonly Color Bg2 = Hex("#0f141b"); // 카드 배경
        public static readonly Color Bg3 = Hex("#161c26");
        public static readonly Color Bg4 = Hex("#1f2733");

        // ───── 보더 ─────
        public static readonly Color Line = new Color(1f, 1f, 1f, 0.07f);
        public static readonly Color LineStrong = new Color(1f, 1f, 1f, 0.14f);

        // ───── 텍스트 ─────
        public static readonly Color Ink = Hex("#e6edf3");
        public static readonly Color InkDim = Hex("#8b949e");
        public static readonly Color InkFaint = Hex("#555c66");

        // ───── 네온 액센트 ─────
        public static readonly Color NeonGreen = Hex("#00ff9c"); // 주 액센트 (성공·승인)
        public static readonly Color NeonCyan = Hex("#5eead4");
        public static readonly Color NeonBlue = Hex("#00b3ff");
        public static readonly Color NeonMagenta = Hex("#ff3d8b"); // 강조·경고
        public static readonly Color NeonYellow = Hex("#ffe066");
        public static readonly Color NeonViolet = Hex("#a78bfa");

        // ───── 상태 ─────
        public static readonly Color Danger = Hex("#ff5470");
        public static readonly Color Warning = Hex("#ffaa00");
        public static readonly Color Success = Hex("#00ff9c");

        // ───── 1×1 텍스처 캐시 (DrawSolidRect용) ─────
        private static Texture2D _whitePixel;
        public static Texture2D WhitePixel
        {
            get
            {
                if (_whitePixel == null)
                {
                    _whitePixel = new Texture2D(1, 1);
                    _whitePixel.SetPixel(0, 0, Color.white);
                    _whitePixel.Apply();
                }
                return _whitePixel;
            }
        }

        /// <summary>색 사각형 그리기 (UI 어디서든 사용)</summary>
        public static void DrawRect(Rect r, Color c)
        {
            var prev = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, WhitePixel);
            GUI.color = prev;
        }

        /// <summary>1px 보더 — 카드/패널 외곽</summary>
        public static void DrawBorder(Rect r, Color c, float thickness = 1f)
        {
            DrawRect(new Rect(r.x, r.y, r.width, thickness), c); // top
            DrawRect(new Rect(r.x, r.yMax - thickness, r.width, thickness), c); // bottom
            DrawRect(new Rect(r.x, r.y, thickness, r.height), c); // left
            DrawRect(new Rect(r.xMax - thickness, r.y, thickness, r.height), c); // right
        }

        /// <summary>카드 스타일 — 어두운 배경 + 보더</summary>
        public static void DrawCard(Rect r, Color? accentColor = null)
        {
            DrawRect(r, Bg2);
            DrawBorder(r, accentColor ?? Line, 1f);
        }

        /// <summary>네온 강조 카드 — 네온 색 보더 + 글로우 시뮬레이션</summary>
        public static void DrawCardBright(Rect r, Color accent)
        {
            DrawRect(r, Bg2);
            // 외곽 글로우 (반투명 사각형들)
            var glowColor = new Color(accent.r, accent.g, accent.b, 0.15f);
            DrawRect(new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 4), glowColor);
            DrawRect(r, Bg2);
            DrawBorder(r, accent, 1f);
        }

        /// <summary>펄스 dot — 살아있는 신호 (좌표는 점의 중심)</summary>
        public static void DrawPulseDot(Vector2 center, Color color, float radius = 4f)
        {
            float pulse = 0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * 4f);
            var c = new Color(color.r, color.g, color.b, pulse);
            DrawRect(new Rect(center.x - radius, center.y - radius, radius * 2, radius * 2), c);
        }

        // ───── 공용 GUIStyle (lazy init) ─────
        private static GUIStyle _styleTitle, _styleMono, _styleMonoSmall, _styleInkDim, _styleNeon, _styleButton, _styleTag;

        public static GUIStyle Title
        {
            get
            {
                if (_styleTitle == null)
                {
                    _styleTitle = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 40,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = Ink }
                    };
                }
                return _styleTitle;
            }
        }

        public static GUIStyle Mono
        {
            get
            {
                if (_styleMono == null)
                {
                    _styleMono = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 14,
                        wordWrap = true,
                        normal = { textColor = Ink }
                    };
                }
                return _styleMono;
            }
        }

        public static GUIStyle MonoSmall
        {
            get
            {
                if (_styleMonoSmall == null)
                {
                    _styleMonoSmall = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 11,
                        normal = { textColor = InkDim }
                    };
                }
                return _styleMonoSmall;
            }
        }

        public static GUIStyle InkDimLabel
        {
            get
            {
                if (_styleInkDim == null)
                {
                    _styleInkDim = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 12,
                        normal = { textColor = InkDim }
                    };
                }
                return _styleInkDim;
            }
        }

        public static GUIStyle NeonLabel
        {
            get
            {
                if (_styleNeon == null)
                {
                    _styleNeon = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 14,
                        fontStyle = FontStyle.Bold,
                        normal = { textColor = NeonGreen }
                    };
                }
                return _styleNeon;
            }
        }

        public static GUIStyle Button
        {
            get
            {
                if (_styleButton == null)
                {
                    _styleButton = new GUIStyle(GUI.skin.button)
                    {
                        fontSize = 13,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter,
                        normal = {
                            textColor = Ink,
                            background = WhitePixel // 비어있게 표시되도록
                        }
                    };
                }
                return _styleButton;
            }
        }

        /// <summary>
        /// Editor와 빌드본 모두에서 안정적으로 작동하는 OnGUI 좌표계 마우스 위치.
        /// 빌드본의 New Input System 환경에서 Event.current.mousePosition이 갱신 안 되는 버그 대응.
        /// Mouse.current.position을 OnGUI 좌표계(좌상단 원점, Y 반전)로 변환해 fallback.
        /// </summary>
        public static Vector2 GetMousePos()
        {
            // 1순위: Mouse.current (New Input System) — 빌드본·Editor 모두 안정적
            var mouse = Mouse.current;
            if (mouse != null)
            {
                var p = mouse.position.ReadValue();
                return new Vector2(p.x, Screen.height - p.y);
            }
            // 2순위: Event.current (IMGUI 기본)
            var ev = Event.current;
            if (ev != null) return ev.mousePosition;
            return Vector2.zero;
        }

        /// <summary>네온 버튼 — 직접 그리기 (border + hover 효과)</summary>
        public static bool NeonButton(Rect rect, string text, Color accent)
        {
            bool hover = rect.Contains(GetMousePos());
            DrawRect(rect, hover ? new Color(accent.r, accent.g, accent.b, 0.12f) : Bg2);
            DrawBorder(rect, accent, hover ? 2f : 1f);

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = accent }
            };
            GUI.Label(rect, text.ToUpper(), style);

            return GUI.Button(rect, GUIContent.none, GUIStyle.none);
        }

        /// <summary>일반 버튼 — 어두운 배경 + 흰 텍스트</summary>
        public static bool GhostButton(Rect rect, string text)
        {
            bool hover = rect.Contains(GetMousePos());
            DrawRect(rect, hover ? Bg3 : Bg2);
            DrawBorder(rect, hover ? LineStrong : Line, 1f);

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = hover ? Ink : InkDim }
            };
            GUI.Label(rect, text.ToUpper(), style);

            return GUI.Button(rect, GUIContent.none, GUIStyle.none);
        }

        /// <summary>태그 chip — 작은 라벨 (TAG, BADGE)</summary>
        public static void DrawTag(Rect rect, string text, Color accent)
        {
            DrawRect(rect, new Color(accent.r, accent.g, accent.b, 0.08f));
            DrawBorder(rect, new Color(accent.r, accent.g, accent.b, 0.4f), 1f);

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = accent }
            };
            GUI.Label(rect, text.ToUpper(), style);
        }

        /// <summary>프로그레스 바 — 네온 글로우</summary>
        public static void DrawProgressBar(Rect rect, float pct01, Color accent)
        {
            DrawRect(rect, Bg3);
            float w = rect.width * Mathf.Clamp01(pct01);
            DrawRect(new Rect(rect.x, rect.y, w, rect.height), accent);
        }

        /// <summary>윈도우 헤더 (가짜 OS 창) — Mission Dossier 분위기</summary>
        public static void DrawWinBar(Rect rect, string title)
        {
            DrawRect(rect, Bg2);
            DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), Line);

            // 좌상단 dot 3개 (mac 스타일)
            float dotY = rect.y + rect.height * 0.5f - 5.5f;
            DrawRect(new Rect(rect.x + 14, dotY, 11, 11), new Color(1f, 0.37f, 0.34f));
            DrawRect(new Rect(rect.x + 30, dotY, 11, 11), new Color(1f, 0.74f, 0.18f));
            DrawRect(new Rect(rect.x + 46, dotY, 11, 11), new Color(0.16f, 0.78f, 0.25f));

            // 중앙 타이틀
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = InkDim }
            };
            GUI.Label(rect, title, style);
        }

        /// <summary>스캔라인 효과 — 화면 전체 또는 영역</summary>
        public static void DrawScanlines(Rect area, float opacity = 0.06f)
        {
            var scanColor = new Color(0f, 0f, 0f, opacity);
            for (float y = area.y; y < area.yMax; y += 3f)
                DrawRect(new Rect(area.x, y, area.width, 1f), scanColor);
        }

        /// <summary>그리드 배경 — 매트릭스/터미널 느낌</summary>
        public static void DrawGridBg(Rect area, float gridSize = 32f, float opacity = 0.04f)
        {
            DrawRect(area, Bg0);
            var lineColor = new Color(NeonGreen.r, NeonGreen.g, NeonGreen.b, opacity);
            for (float x = area.x; x < area.xMax; x += gridSize)
                DrawRect(new Rect(x, area.y, 1, area.height), lineColor);
            for (float y = area.y; y < area.yMax; y += gridSize)
                DrawRect(new Rect(area.x, y, area.width, 1), lineColor);
        }

        // ───── Hex 파서 ─────
        private static Color Hex(string hex)
        {
            hex = hex.TrimStart('#');
            byte r = System.Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = System.Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = System.Convert.ToByte(hex.Substring(4, 2), 16);
            return new Color(r / 255f, g / 255f, b / 255f);
        }
    }
}
