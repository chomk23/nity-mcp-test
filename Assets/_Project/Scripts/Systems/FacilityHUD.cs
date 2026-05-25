using System.Collections.Generic;
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
        private GUIStyle namePlateStyle;
        private GUIStyle quizQuestionStyle;
        private GUIStyle quizResultStyle;
        private GUIStyle quizButtonStyle;

        public static bool IsInventoryOpen { get; private set; }
        private Vector2 inventoryScroll;

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
            namePlateStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            quizQuestionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20, wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = Color.white }
            };
            quizResultStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17, wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.7f, 1f, 0.7f) }
            };
            quizButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16, wordWrap = true,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(14, 12, 8, 8)
            };
        }

        private AccusationConsole FindConsole()
        {
            var all = FindObjectsByType<AccusationConsole>(FindObjectsSortMode.None);
            return all.Length > 0 ? all[0] : null;
        }

        private void HandleInventoryToggle(Keyboard kb)
        {
            // 인벤토리가 이미 열렸으면 ESC/I로 닫기
            if (IsInventoryOpen)
            {
                if (kb.escapeKey.wasPressedThisFrame || kb.iKey.wasPressedThisFrame)
                    IsInventoryOpen = false;
                return;
            }

            // 다른 모달 떠 있으면 토글 무시
            var console = FindConsole();
            if (console != null && console.IsMenuOpen) return;
            var rmc = RacingMissionController.Instance;
            if (rmc != null && rmc.IsOpen) return;
            var ds = DialogueSystem.Instance;
            if (ds != null && ds.IsActive) return;
            var sqc = SecurityQuizController.Instance;
            if (sqc != null && sqc.IsOpen) return;
            var bridge = RacingWebViewBridge.Instance;
            if (bridge != null && bridge.IsShowing) return;

            // 결말 후엔 인벤토리 안 열림 (이미 결말 화면)
            var s = GameSession.Instance;
            if (s != null && s.Outcome != RunOutcome.Ongoing) return;

            if (kb.iKey.wasPressedThisFrame)
                IsInventoryOpen = true;
        }

        private void OnGUI()
        {
            Init();
            DrawNPCNameplates();
            DrawMiniMap();        // 좌상단 — 미니맵
            DrawInfoPanel();      // 우상단 — 통합 UI (단서/시간/목표)
            DrawQuestAdvanceToast();
            DrawInteractionPrompt();
            DrawToast();
            DrawQuizModal();
            DrawRacingModal();
            DrawAccusationModal();
            DrawDialogueBox();
            DrawInventoryPanel();
            DrawEndScreen();
            DrawHint();
        }

        /// <summary>SecureSense 터미널 톤 — 하단 대화창</summary>
        private void DrawDialogueBox()
        {
            var ds = DialogueSystem.Instance;
            if (ds == null || !ds.IsActive) return;

            // 슬라이드 인 애니메이션
            float t = Mathf.Clamp01((Time.time - ds.OpenTime) / 0.25f);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            float boxW = Mathf.Min(Screen.width - 80, 1120);
            float boxH = 200;
            float boxX = (Screen.width - boxW) * 0.5f;
            float targetY = Screen.height - boxH - 40;
            float startY = Screen.height + 20;
            float boxY = Mathf.Lerp(startY, targetY, eased);

            // 메인 박스 — Bg1 + 네온 그린 강조선
            UITheme.DrawRect(new Rect(boxX, boxY, boxW, boxH), UITheme.Bg1);
            UITheme.DrawBorder(new Rect(boxX, boxY, boxW, boxH), UITheme.Line);
            UITheme.DrawRect(new Rect(boxX, boxY, boxW, 2f), UITheme.NeonGreen);

            // 좌측 NPC 이름 태그 (위로 살짝 튀어나옴)
            if (!string.IsNullOrEmpty(ds.CurrentSpeaker))
            {
                float nameW = Mathf.Min(280f, ds.CurrentSpeaker.Length * 16f + 60f);
                float nameH = 32f;
                float nameX = boxX + 24f;
                float nameY = boxY - nameH * 0.5f;

                UITheme.DrawRect(new Rect(nameX, nameY, nameW, nameH), UITheme.Bg3);
                UITheme.DrawBorder(new Rect(nameX, nameY, nameW, nameH), UITheme.NeonGreen);

                var nameStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13, fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = UITheme.NeonGreen }
                };
                GUI.Label(new Rect(nameX, nameY, nameW, nameH),
                    "▸ " + ds.CurrentSpeaker.ToUpper(), nameStyle);

                // 우측 윈도우 chrome — "transmission.log"
                var winLabel = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 10,
                    alignment = TextAnchor.MiddleRight,
                    padding = new RectOffset(0, 24, 0, 0),
                    normal = { textColor = UITheme.InkFaint }
                };
                GUI.Label(new Rect(boxX, boxY + 8, boxW, 16),
                    $"// {ds.CurrentSpeaker.ToLower()}-transmission.log", winLabel);
            }

            // 본문 텍스트 — 타이프라이팅
            var bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18, wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = UITheme.Ink }
            };
            float padX = 36, padTop = 40, padBottom = 40;
            GUI.Label(new Rect(boxX + padX, boxY + padTop, boxW - padX * 2, boxH - padTop - padBottom),
                ds.CurrentVisibleLine, bodyStyle);

            // 진행/선택지
            if (ds.AwaitingChoice)
            {
                DrawDialogueChoices(ds);
            }
            else if (ds.LineComplete)
            {
                // 우하단 진행 안내 (블링킹)
                float blink = Mathf.Abs(Mathf.Sin(Time.time * 3f));
                var prev = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.6f + 0.4f * blink);
                var promptSt = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11, fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleRight,
                    padding = new RectOffset(0, 28, 0, 0),
                    normal = { textColor = UITheme.NeonGreen }
                };
                string hint = ds.IsLastLine ? "▸ [SPACE] END TRANSMISSION" : "▸ [SPACE] CONTINUE";
                GUI.Label(new Rect(boxX, boxY + boxH - 26, boxW, 18), hint, promptSt);
                GUI.color = prev;
            }
        }

        /// <summary>SecureSense 톤 — 우측 세로 선택지 박스 (마우스/숫자키)</summary>
        private void DrawDialogueChoices(DialogueSystem ds)
        {
            var choices = ds.CurrentChoices;
            if (choices == null || choices.Count == 0) return;

            float boxW = 420f;
            float boxX = Screen.width - boxW - 30f;
            float startY = Screen.height * 0.28f;
            float gap = 10f;

            // 헤더
            var headerSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(8, 0, 0, 0),
                normal = { textColor = UITheme.NeonCyan }
            };
            GUI.Label(new Rect(boxX, startY - 24, boxW, 20),
                "▸ RESPONSE OPTIONS // SELECT ONE", headerSt);

            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15, wordWrap = true,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(48, 16, 10, 10)
            };
            var numStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(20, 0, 0, 0),
                normal = { textColor = UITheme.NeonGreen }
            };

            Vector2 mp = UITheme.GetMousePos();
            float currentY = startY;

            for (int i = 0; i < choices.Count; i++)
            {
                var c = choices[i];
                string display = c.Label;
                var content = new GUIContent(display);
                float labelH = labelStyle.CalcHeight(content, boxW - 80);
                float boxH = Mathf.Max(54f, labelH + 10f);
                var rect = new Rect(boxX, currentY, boxW, boxH);

                bool hover = rect.Contains(mp);

                // 배경 + 보더
                UITheme.DrawRect(rect, hover ? UITheme.Bg3 : UITheme.Bg2);
                UITheme.DrawBorder(rect, hover ? UITheme.NeonGreen : UITheme.Line);

                // 좌측 강조 띠
                UITheme.DrawRect(new Rect(rect.x, rect.y, 3f, rect.height),
                    hover ? UITheme.NeonGreen : UITheme.NeonCyan);

                // 번호
                GUI.Label(new Rect(rect.x, rect.y, 40, rect.height),
                    $"0{i + 1}", numStyle);

                // 텍스트
                labelStyle.normal.textColor = hover ? UITheme.Ink : UITheme.InkDim;
                GUI.Label(rect, content, labelStyle);

                // 투명 버튼 — 클릭 감지
                if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                {
                    ds.SelectChoice(i);
                }

                currentY += boxH + gap;
            }

            // 하단 힌트
            var hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                padding = new RectOffset(0, 8, 0, 0),
                normal = { textColor = UITheme.InkFaint }
            };
            GUI.Label(new Rect(boxX, currentY + 6, boxW, 18),
                "// [CLICK] OR [1·2·3] TO SELECT", hintStyle);
        }

        // ═══════════════════ 미니맵 (좌상단) ═══════════════════

        // 시설 좌표 범위 (SciFiFloorsSetup 방 좌표 기반)
        private const float FacMinX = -28f, FacMaxX = 28f;
        private const float FacMinZ = -22f, FacMaxZ = 18f;

        // 방 정보: (이름, 중심 X, 중심 Z, 폭, 깊이, 색)
        private static readonly (string label, float cx, float cz, float w, float d, System.Func<Color> col)[] Rooms =
        {
            ("연구실",     -17f, 11f, 10f, 8f, () => UITheme.NeonCyan),
            ("서버실",      0f,  11f, 10f, 8f, () => UITheme.NeonMagenta),
            ("보안통제실", 13f,  11f, 10f, 8f, () => UITheme.NeonViolet),
            ("휴게실",    -13f,  0f,  10f, 8f, () => UITheme.NeonGreen),
            ("카드키",     18f,  2f,  8f,  8f, () => UITheme.Danger),
            ("창고",      -20f, -14f, 10f, 8f, () => UITheme.NeonYellow),
            ("데이터센터", 3f,  -14f, 16f, 8f, () => UITheme.NeonCyan),
            ("전력실",    18f,  -11f, 8f,  8f, () => UITheme.NeonYellow),
        };

        /// <summary>레이싱 WebView / 풀스크린 모달이 떠 있으면 HUD 패널 숨김</summary>
        private bool ShouldHideTopHud()
        {
            // 보안 레이싱 WebView (가장 큰 이유 — Unity OnGUI 위 또는 아래에 별도 렌더링)
            var bridge = RacingWebViewBridge.Instance;
            if (bridge != null && bridge.IsShowing) return true;

            // 풀스크린 모달들
            var sqc = SecurityQuizController.Instance;
            if (sqc != null && sqc.IsOpen) return true;

            var rmc = RacingMissionController.Instance;
            if (rmc != null && rmc.IsOpen) return true;

            var console = FindConsole();
            if (console != null && console.IsMenuOpen) return true;

            if (IsInventoryOpen) return true;

            var s = GameSession.Instance;
            if (s != null && s.Outcome != RunOutcome.Ongoing) return true;

            return false;
        }

        /// <summary>좌상단 미니맵 — 시설 평면도 + 실시간 플레이어 위치</summary>
        private void DrawMiniMap()
        {
            if (ShouldHideTopHud()) return;

            float panelW = 300f, panelH = 240f;
            float panelX = 16f, panelY = 16f;
            var panel = new Rect(panelX, panelY, panelW, panelH);

            // 패널 베이스
            UITheme.DrawRect(panel, UITheme.Bg1);
            UITheme.DrawBorder(panel, UITheme.LineStrong, 1f);

            // 윈도우 헤더
            var headerRect = new Rect(panelX, panelY, panelW, 28);
            UITheme.DrawRect(headerRect, UITheme.Bg3);
            UITheme.DrawRect(new Rect(panelX, panelY + 28, panelW, 1), UITheme.Line);

            var headerSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(14, 0, 0, 0),
                normal = { textColor = UITheme.NeonGreen }
            };
            GUI.Label(headerRect, "▸ 시설 평면도 // 실시간", headerSt);
            UITheme.DrawPulseDot(new Vector2(panelX + panelW - 18, panelY + 14), UITheme.NeonGreen, 3f);

            // 맵 영역 (헤더 제외)
            float mapMargin = 12f;
            float mapX = panelX + mapMargin;
            float mapY = panelY + 28 + mapMargin;
            float mapW = panelW - mapMargin * 2;
            float mapH = panelH - 28 - mapMargin * 2;
            var mapRect = new Rect(mapX, mapY, mapW, mapH);

            // 맵 배경 (그리드)
            UITheme.DrawRect(mapRect, UITheme.Bg0);

            // 그리드 라인 (4×4 분할 — 흐릿하게)
            var gridCol = new Color(UITheme.NeonGreen.r, UITheme.NeonGreen.g, UITheme.NeonGreen.b, 0.06f);
            for (int i = 1; i < 4; i++)
            {
                float gx = mapX + (mapW / 4f) * i;
                float gy = mapY + (mapH / 4f) * i;
                UITheme.DrawRect(new Rect(gx, mapY, 1, mapH), gridCol);
                UITheme.DrawRect(new Rect(mapX, gy, mapW, 1), gridCol);
            }

            // 방 사각형 + 라벨
            var roomLabelSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 9, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = UITheme.Ink }
            };
            foreach (var r in Rooms)
            {
                Vector2 minWorld = new Vector2(r.cx - r.w * 0.5f, r.cz - r.d * 0.5f);
                Vector2 maxWorld = new Vector2(r.cx + r.w * 0.5f, r.cz + r.d * 0.5f);
                Vector2 minPx = WorldToMap(minWorld, mapRect);
                Vector2 maxPx = WorldToMap(maxWorld, mapRect);
                // Z가 클수록 화면 위 → minPx.y가 maxPx보다 크므로 정렬
                float rx = Mathf.Min(minPx.x, maxPx.x);
                float ry = Mathf.Min(minPx.y, maxPx.y);
                float rw = Mathf.Abs(maxPx.x - minPx.x);
                float rh = Mathf.Abs(maxPx.y - minPx.y);
                var roomRect = new Rect(rx, ry, rw, rh);

                Color rc = r.col();
                UITheme.DrawRect(roomRect, new Color(rc.r, rc.g, rc.b, 0.18f));
                UITheme.DrawBorder(roomRect, new Color(rc.r, rc.g, rc.b, 0.7f), 1f);

                // 방 이름 (방이 충분히 크면)
                if (rw > 32 && rh > 16)
                    GUI.Label(roomRect, r.label, roomLabelSt);
            }

            // NPC 점 표시
            var roster = NPCRoster.Instance;
            if (roster != null)
            {
                foreach (var npc in roster.npcs)
                {
                    if (npc == null) continue;
                    Vector2 p = WorldToMap(
                        new Vector2(npc.transform.position.x, npc.transform.position.z), mapRect);
                    // 의심도에 따라 색
                    int sus = npc.suspicion;
                    Color npcCol = sus >= 7 ? UITheme.Danger
                        : sus >= 4 ? UITheme.NeonYellow
                        : UITheme.InkDim;
                    UITheme.DrawRect(new Rect(p.x - 3, p.y - 3, 6, 6), npcCol);
                }
            }

            // 플레이어 점 (펄스 — 가장 큰 강조)
            var player = PlayerInteractor.Instance;
            if (player != null)
            {
                Vector2 pp = WorldToMap(
                    new Vector2(player.transform.position.x, player.transform.position.z),
                    mapRect);

                // 외곽 펄스 링 (반투명, 큼)
                float pulse = 0.4f + 0.4f * Mathf.Sin(Time.unscaledTime * 4f);
                var ringCol = new Color(UITheme.NeonGreen.r, UITheme.NeonGreen.g,
                    UITheme.NeonGreen.b, pulse * 0.5f);
                float ringR = 8f;
                UITheme.DrawRect(new Rect(pp.x - ringR, pp.y - ringR, ringR * 2, ringR * 2), ringCol);

                // 중앙 솔리드 점
                UITheme.DrawRect(new Rect(pp.x - 4, pp.y - 4, 8, 8), UITheme.NeonGreen);
                UITheme.DrawBorder(new Rect(pp.x - 5, pp.y - 5, 10, 10), UITheme.Bg0, 1f);
            }

            // 맵 영역 보더
            UITheme.DrawBorder(mapRect, UITheme.Line, 1f);
        }

        /// <summary>월드 (x, z) → 미니맵 픽셀 좌표. Z↑ = 화면↑ (Y 뒤집기)</summary>
        private static Vector2 WorldToMap(Vector2 world, Rect mapRect)
        {
            float u = Mathf.InverseLerp(FacMinX, FacMaxX, world.x);
            float v = Mathf.InverseLerp(FacMinZ, FacMaxZ, world.y);
            float px = mapRect.x + u * mapRect.width;
            // Z가 클수록 화면 위쪽이므로 v 뒤집기
            float py = mapRect.y + (1f - v) * mapRect.height;
            return new Vector2(px, py);
        }

        // ═══════════════════ 통합 정보 패널 (우상단) ═══════════════════

        /// <summary>우상단 통합 패널 — 수사 진행 + 단서·시간 + 현재 목표</summary>
        private void DrawInfoPanel()
        {
            if (ShouldHideTopHud()) return;

            var s = GameSession.Instance;
            var quest = QuestManager.Instance;

            int clues = s != null ? s.totalClues : 0;
            float timeLeft = s != null ? s.TimeRemaining : 0f;

            float w = 340f;
            float x = Screen.width - w - 16f;
            float y = 16f;

            // 헤더 + 단서/시간 섹션 높이 고정
            float headerH = 32f;
            float statsH = 70f;
            float objectiveH = 0f;
            bool hasQuest = quest != null && quest.CurrentStage != QuestManager.Stage.Done;

            // 본문 사이즈 계산용 스타일
            var objBodySt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12, wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = UITheme.Ink }
            };
            var locHintSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11, wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = UITheme.NeonCyan }
            };

            float contentW = w - 28;
            float bodyH = 0, hintH = 0;
            if (hasQuest)
            {
                bodyH = objBodySt.CalcHeight(new GUIContent(quest.CurrentObjective), contentW);
                hintH = locHintSt.CalcHeight(new GUIContent(quest.CurrentLocationHint), contentW);
                // 미니헤더(24) + 본문 + 진행dot(16) + 위치힌트 + 패딩
                objectiveH = 24 + 8 + bodyH + 10 + 16 + 6 + hintH + 16;
            }

            float h = headerH + statsH + objectiveH + 12;

            var panel = new Rect(x, y, w, h);
            UITheme.DrawRect(panel, UITheme.Bg1);
            UITheme.DrawBorder(panel, UITheme.LineStrong, 1f);

            // ── 헤더 ──
            var headerRect = new Rect(x, y, w, headerH);
            UITheme.DrawRect(headerRect, UITheme.Bg3);
            UITheme.DrawRect(new Rect(x, y + headerH, w, 1), UITheme.Line);

            var headerSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(14, 0, 0, 0),
                normal = { textColor = UITheme.NeonGreen }
            };
            GUI.Label(headerRect, "▸ 수사 진행 중", headerSt);
            UITheme.DrawPulseDot(new Vector2(x + w - 18, y + 16), UITheme.NeonGreen, 4f);

            // ── 단서 / 시간 (2열) ──
            float statsY = y + headerH + 6;
            float colW = w * 0.5f;

            var labelSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(14, 0, 0, 0),
                normal = { textColor = UITheme.InkDim }
            };

            // 좌측 — 수집한 단서
            GUI.Label(new Rect(x, statsY, colW, 16), "수집한 단서", labelSt);
            var clueValSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(14, 0, 0, 0),
                normal = { textColor = UITheme.Ink }
            };
            GUI.Label(new Rect(x, statsY + 18, colW, 36), clues.ToString("D2"), clueValSt);

            // 중앙 세로 구분선
            UITheme.DrawRect(new Rect(x + colW, statsY + 4, 1, statsH - 12), UITheme.Line);

            // 우측 — 남은 시간
            var labelRSt = new GUIStyle(labelSt) { padding = new RectOffset(14, 0, 0, 0) };
            GUI.Label(new Rect(x + colW, statsY, colW, 16), "남은 시간", labelRSt);

            int totalSec = Mathf.Max(0, Mathf.CeilToInt(timeLeft));
            int mm = totalSec / 60;
            int ss = totalSec % 60;
            Color timeCol = timeLeft <= 30f ? UITheme.Danger
                : timeLeft <= 60f ? UITheme.NeonYellow
                : UITheme.NeonCyan;
            var timeValSt = new GUIStyle(clueValSt) { normal = { textColor = timeCol } };
            GUI.Label(new Rect(x + colW, statsY + 18, colW, 36), $"{mm:D2}:{ss:D2}", timeValSt);

            // ── 현재 목표 섹션 ──
            if (!hasQuest) return;

            int total = (int)QuestManager.Stage.Done;
            int step = (int)quest.CurrentStage + 1;

            float objY = y + headerH + statsH;
            UITheme.DrawRect(new Rect(x + 14, objY, w - 28, 1), UITheme.Line);

            // 미니 헤더 (마젠타)
            var miniHeaderSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(14, 0, 0, 0),
                normal = { textColor = UITheme.NeonMagenta }
            };
            GUI.Label(new Rect(x, objY + 4, w - 60, 20),
                $"▸ 현재 목표 // 단계 {step:D2}", miniHeaderSt);

            var ratioSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                padding = new RectOffset(0, 14, 0, 0),
                normal = { textColor = UITheme.InkDim }
            };
            GUI.Label(new Rect(x, objY + 4, w, 20), $"{step:D2} / {total:D2}", ratioSt);

            // 본문
            float cy = objY + 30;
            GUI.Label(new Rect(x + 14, cy, contentW, bodyH), quest.CurrentObjective, objBodySt);
            cy += bodyH + 10;

            // 진행도 dot들
            float dotSize = 6f, dotGap = 12f;
            float dotsX = x + 14;
            for (int i = 0; i < total; i++)
            {
                Color c = i < step ? UITheme.NeonGreen : UITheme.Bg4;
                UITheme.DrawRect(new Rect(dotsX + i * dotGap, cy + 5, dotSize, dotSize), c);
            }
            cy += 16;

            // 위치 힌트
            GUI.Label(new Rect(x + 14, cy + 4, contentW, hintH),
                "▸ " + quest.CurrentLocationHint, locHintSt);
        }

        // 단계 완료 토스트가 NPC 대화 토스트 종료 후 시작되도록 추적
        private string lastShownAdvanceText;
        private float advanceToastStartTime = -1f;
        private const float AdvanceToastDuration = 4f;

        /// <summary>NPC 대화 토스트가 끝난 뒤에 단계 완료 토스트 표시 (순차 흐름)</summary>
        private void DrawQuestAdvanceToast()
        {
            var quest = QuestManager.Instance;
            if (quest == null || string.IsNullOrEmpty(quest.LastAdvanceText)) return;

            // 새로운 단계 진행이 들어왔는지 감지
            if (quest.LastAdvanceText != lastShownAdvanceText)
            {
                lastShownAdvanceText = quest.LastAdvanceText;
                advanceToastStartTime = -1f; // NPC 토스트 종료 대기
            }

            // 하단 대화창 또는 NPC 대화 토스트가 활성이면 대기
            if (advanceToastStartTime < 0f)
            {
                var ds = DialogueSystem.Instance;
                if (ds != null && ds.IsActive) return; // 대화창 종료 대기
                var p = PlayerInteractor.Instance;
                bool talkActive = p != null
                    && !string.IsNullOrEmpty(p.LastInteractionResult)
                    && (Time.time - p.LastInteractionTime) <= 4f;
                if (talkActive) return; // 대화 토스트 종료 대기
                advanceToastStartTime = Time.time;
            }

            float elapsed = Time.time - advanceToastStartTime;
            if (elapsed > AdvanceToastDuration) return;

            float w = 640;
            float contentW = w - 60;
            var bodySt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14, wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = UITheme.Ink }
            };
            float bodyH = bodySt.CalcHeight(new GUIContent(quest.LastAdvanceText), contentW);

            float headerH = 52;
            float pad = 18;
            float h = headerH + bodyH + pad * 2;

            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.24f;
            float alpha = Mathf.Clamp01(1f - (elapsed - 3f));
            var prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);

            var toastRect = new Rect(x, y, w, h);

            // ── 패널 ──
            UITheme.DrawRect(toastRect, UITheme.Bg2);
            UITheme.DrawBorder(toastRect, UITheme.NeonGreen, 1f);

            // 좌측 강조선 (네온 그린)
            UITheme.DrawRect(new Rect(x, y, 3f, h), UITheme.NeonGreen);

            // 펄스 dot + 헤더
            UITheme.DrawPulseDot(new Vector2(x + 22, y + 22), UITheme.NeonGreen, 4f);

            var tagSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10, fontStyle = FontStyle.Bold,
                normal = { textColor = UITheme.NeonGreen }
            };
            GUI.Label(new Rect(x + 36, y + 14, w - 60, 14),
                "▸ STAGE CLEARED // PROGRESS UPDATED", tagSt);

            var titleSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22, fontStyle = FontStyle.Bold,
                normal = { textColor = UITheme.Ink }
            };
            GUI.Label(new Rect(x + 36, y + 28, w - 60, 28),
                "단계 완료", titleSt);

            // 본문
            GUI.Label(new Rect(x + pad + 18, y + pad + headerH, contentW, bodyH),
                quest.LastAdvanceText, bodySt);

            GUI.color = prev;
        }

        private void DrawRacingModal()
        {
            var rmc = RacingMissionController.Instance;
            if (rmc == null || !rmc.IsOpen) return;

            // 임베드 WebView가 보이는 중엔 Unity 등수 입력 모달 숨김
            var bridge = RacingWebViewBridge.Instance;
            bool isEmbedded = bridge != null && bridge.IsShowing;
            if (rmc.CurrentPhase == RacingMissionController.Phase.AwaitingRank && isEmbedded)
            {
                DrawRacingEmbeddedClose(rmc);
                return;
            }

            // 어둠 오버레이
            UITheme.DrawRect(new Rect(0, 0, Screen.width, Screen.height),
                new Color(UITheme.Bg0.r, UITheme.Bg0.g, UITheme.Bg0.b, 0.86f));
            UITheme.DrawScanlines(new Rect(0, 0, Screen.width, Screen.height), 0.05f);

            float w = Mathf.Min(680, Screen.width - 80);
            float h = 500;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            var rect = new Rect(x, y, w, h);
            UITheme.DrawRect(rect, UITheme.Bg1);
            UITheme.DrawBorder(rect, UITheme.NeonCyan, 1f);

            // 윈도우 헤더
            UITheme.DrawWinBar(new Rect(x, y, w, 32), "security-race.module");

            // 헤더 섹션
            float headerY = y + 50;
            UITheme.DrawPulseDot(new Vector2(x + 28, headerY + 12), UITheme.NeonCyan, 4f);

            var tagSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10, fontStyle = FontStyle.Bold,
                normal = { textColor = UITheme.NeonCyan }
            };
            GUI.Label(new Rect(x + 44, headerY + 2, w - 80, 14),
                "▸ HIGH-SPEED INFILTRATION // STAGE", tagSt);

            var titleSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26, fontStyle = FontStyle.Bold,
                normal = { textColor = UITheme.Ink }
            };
            GUI.Label(new Rect(x + 44, headerY + 16, w - 80, 32),
                "SECURITY RACE", titleSt);

            // 구분선
            UITheme.DrawRect(new Rect(x + 24, headerY + 58, w - 48, 1), UITheme.Line);

            if (rmc.CurrentPhase == RacingMissionController.Phase.AwaitingRank)
                DrawRankInputModal(rmc, x, y, w, h);
            else if (rmc.CurrentPhase == RacingMissionController.Phase.Finished)
                DrawRacingFinished(rmc, x, y, w, h);
        }

        private void DrawRacingEmbeddedClose(RacingMissionController rmc)
        {
            // 중앙 상단 — 우상단 InfoPanel과 겹치지 않도록
            float w = 360, h = 38;
            float x = (Screen.width - w) * 0.5f;
            float y = 12;

            var rect = new Rect(x, y, w, h);
            UITheme.DrawRect(rect, new Color(UITheme.Bg1.r, UITheme.Bg1.g, UITheme.Bg1.b, 0.92f));
            UITheme.DrawBorder(rect, UITheme.NeonCyan, 1f);

            UITheme.DrawPulseDot(new Vector2(x + 14, y + h * 0.5f), UITheme.NeonCyan, 3f);

            var tagSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = UITheme.NeonCyan }
            };
            GUI.Label(new Rect(x + 28, y, 180, h),
                "▸ 보안 레이싱 진행 중", tagSt);

            var hintSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = UITheme.InkDim }
            };
            GUI.Label(new Rect(x + 200, y, w - 216, h),
                "[ESC] 강제 종료", hintSt);
        }

        private void DrawRankInputModal(RacingMissionController rmc, float x, float y, float w, float h)
        {
            var bodySt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13, wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = UITheme.InkDim }
            };
            GUI.Label(new Rect(x + 30, y + 120, w - 60, 80),
                "SECURITY RACE를 플레이하셨습니다.\n1등으로 결승선을 통과하면 클리어, 그 외엔 재시도하세요.",
                bodySt);

            float btnH = 60;
            float gap = 10;
            float startY = y + 220;

            DrawRankButton(new Rect(x + 30, startY, w - 60, btnH),
                "▸ 1등 했음 — 클리어 (+5 단서)",
                UITheme.NeonGreen, () => rmc.ReportRank(1));

            DrawRankButton(new Rect(x + 30, startY + (btnH + gap), w - 60, btnH),
                "▸ 1등 못 함 — 재시도 (보상 없음)",
                UITheme.InkDim, () => rmc.Cancel());
        }

        private void DrawRankButton(Rect r, string label, Color tint, System.Action onClick)
        {
            bool hover = r.Contains(UITheme.GetMousePos());
            UITheme.DrawRect(r, hover ? new Color(tint.r, tint.g, tint.b, 0.14f) : UITheme.Bg2);
            UITheme.DrawBorder(r, hover ? tint : UITheme.Line, hover ? 2f : 1f);

            var s = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = hover ? tint : UITheme.Ink },
                padding = new RectOffset(20, 0, 0, 0)
            };
            GUI.Label(r, label, s);

            if (GUI.Button(r, GUIContent.none, GUIStyle.none)) onClick?.Invoke();
        }

        private void DrawRacingFinished(RacingMissionController rmc, float x, float y, float w, float h)
        {
            string rankLabel = rmc.FinalRank == 1 ? "1ST"
                             : rmc.FinalRank == 2 ? "2ND"
                             : "3RD";
            Color rankColor = rmc.FinalRank == 1 ? UITheme.NeonYellow : UITheme.NeonCyan;

            var rankS = new GUIStyle(GUI.skin.label)
            {
                fontSize = 56, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = rankColor }
            };
            GUI.Label(new Rect(x, y + 130, w, 80), rankLabel, rankS);

            var subSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = UITheme.InkDim }
            };
            GUI.Label(new Rect(x, y + 200, w, 22),
                rmc.FinalRank == 1 ? "// SUCCESS"
                : rmc.FinalRank == 2 ? "// RUNNER-UP"
                : "// FINISHED", subSt);

            var bodySt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13, wordWrap = true,
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = UITheme.Ink }
            };
            GUI.Label(new Rect(x + 30, y + 240, w - 60, 80),
                rmc.ResultMessage, bodySt);

            // 확인 버튼
            if (UITheme.NeonButton(new Rect(x + (w - 280) * 0.5f, y + h - 70, 280, 46),
                "▸ ACKNOWLEDGE  [SPACE / ESC / ENTER]", rankColor))
                rmc.Acknowledge();
        }

        private static Texture2D _solid;
        private static void DrawSolidRect(Rect r, Color c)
        {
            if (_solid == null)
            {
                _solid = new Texture2D(1, 1);
                _solid.SetPixel(0, 0, Color.white);
                _solid.Apply();
            }
            var prev = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, _solid);
            GUI.color = prev;
        }

        private void DrawNPCNameplates()
        {
            // WebView 레이싱 화면이 떠 있으면 NPC 이름표 숨김
            var bridge = RacingWebViewBridge.Instance;
            if (bridge != null && bridge.IsShowing) return;

            var roster = NPCRoster.Instance;
            if (roster == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            var nameSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = UITheme.Ink }
            };

            foreach (var npc in roster.npcs)
            {
                if (npc == null) continue;
                Vector3 worldHead = npc.transform.position + Vector3.up * 2.4f;
                Vector3 screen = cam.WorldToScreenPoint(worldHead);
                if (screen.z < 0) continue;

                float guiY = Screen.height - screen.y;
                string name = npc.DisplayName;
                float w = 160f, h = 24f;
                var rect = new Rect(screen.x - w * 0.5f, guiY - h * 0.5f, w, h);

                // 의심도에 따른 보더 색
                int sus = npc.suspicion;
                Color borderColor = sus >= 7 ? UITheme.Danger
                    : sus >= 4 ? UITheme.NeonYellow
                    : UITheme.Line;

                UITheme.DrawRect(rect, new Color(UITheme.Bg1.r, UITheme.Bg1.g, UITheme.Bg1.b, 0.92f));
                UITheme.DrawBorder(rect, borderColor, 1f);

                GUI.Label(rect, name, nameSt);

                // 의심도 막대 (이름표 바로 아래)
                if (sus > 0)
                {
                    const float MaxSuspicion = 10f;
                    float barW = 130f, barH = 4f;
                    float barX = screen.x - barW * 0.5f;
                    float barY = guiY + h * 0.5f + 3f;

                    UITheme.DrawRect(new Rect(barX, barY, barW, barH), UITheme.Bg3);

                    float pct = Mathf.Clamp01(sus / MaxSuspicion);
                    Color susColor = sus >= 7 ? UITheme.Danger
                        : sus >= 4 ? UITheme.NeonYellow
                        : UITheme.InkDim;
                    UITheme.DrawRect(new Rect(barX, barY, barW * pct, barH), susColor);
                }
            }
        }

        private void DrawQuizModal()
        {
            var ctrl = SecurityQuizController.Instance;
            if (ctrl == null) return;
            // Show result toast (briefly)
            if (!ctrl.IsOpen && !string.IsNullOrEmpty(ctrl.LastResultText)
                && Time.time - ctrl.LastResultTime < 3.5f && ctrl.LastWasCorrect)
            {
                DrawQuizResultToast(ctrl);
                return;
            }
            if (!ctrl.IsOpen) return;

            var data = ctrl.ActiveClue.data;

            // 어둠 오버레이
            UITheme.DrawRect(new Rect(0, 0, Screen.width, Screen.height),
                new Color(UITheme.Bg0.r, UITheme.Bg0.g, UITheme.Bg0.b, 0.86f));
            UITheme.DrawScanlines(new Rect(0, 0, Screen.width, Screen.height), 0.05f);

            float w = Mathf.Min(780, Screen.width - 80);
            float h = 600;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            var rect = new Rect(x, y, w, h);

            // 패널
            UITheme.DrawRect(rect, UITheme.Bg1);
            UITheme.DrawBorder(rect, UITheme.NeonViolet, 1f);

            // 윈도우 헤더
            UITheme.DrawWinBar(new Rect(x, y, w, 32), "security-training.module");

            // 헤더
            float headerY = y + 48;
            UITheme.DrawPulseDot(new Vector2(x + 28, headerY + 12), UITheme.NeonViolet, 4f);

            var tagSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10, fontStyle = FontStyle.Bold,
                normal = { textColor = UITheme.NeonViolet }
            };
            GUI.Label(new Rect(x + 44, headerY + 2, w - 100, 14),
                "▸ SECURITY TRAINING // MODULE", tagSt);

            var titleSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22, fontStyle = FontStyle.Bold,
                normal = { textColor = UITheme.Ink }
            };
            GUI.Label(new Rect(x + 44, headerY + 16, w - 100, 30),
                "보안 교육 미션", titleSt);

            // 위치 / 객체 태그
            var locTagSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11, fontStyle = FontStyle.Bold,
                normal = { textColor = UITheme.NeonCyan }
            };
            GUI.Label(new Rect(x + 28, headerY + 54, w - 56, 18),
                $"▸ {data.roomName.ToUpper()}  //  {data.objectLabel.ToUpper()}", locTagSt);

            // 구분선
            UITheme.DrawRect(new Rect(x + 24, headerY + 78, w - 48, 1), UITheme.Line);

            // 질문
            var qSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16, fontStyle = FontStyle.Bold, wordWrap = true,
                normal = { textColor = UITheme.Ink }
            };
            GUI.Label(new Rect(x + 30, headerY + 92, w - 60, 100), data.quizQuestion, qSt);

            // 선택지 버튼들
            int n = data.quizOptions != null ? data.quizOptions.Length : 0;
            float btnH = 52;
            float gap = 8;
            float startY = y + 240;

            for (int i = 0; i < n; i++)
            {
                var r = new Rect(x + 30, startY + (btnH + gap) * i, w - 60, btnH);
                string label = $"  0{i + 1}    {data.quizOptions[i]}";

                bool hover = r.Contains(UITheme.GetMousePos());
                Color accent = UITheme.NeonViolet;

                UITheme.DrawRect(r, hover ? new Color(accent.r, accent.g, accent.b, 0.14f) : UITheme.Bg2);
                UITheme.DrawBorder(r, hover ? accent : UITheme.Line, hover ? 2f : 1f);

                var btnSt = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13, fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = hover ? accent : UITheme.Ink }
                };
                GUI.Label(r, label, btnSt);

                if (GUI.Button(r, GUIContent.none, GUIStyle.none))
                    ctrl.Answer(i);
            }

            // 오답 피드백
            if (!string.IsNullOrEmpty(ctrl.LastResultText)
                && !ctrl.LastWasCorrect
                && Time.time - ctrl.LastResultTime < 3.5f)
            {
                var feedSt = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13, wordWrap = true,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = UITheme.Danger }
                };
                GUI.Label(new Rect(x + 24, y + h - 80, w - 48, 40),
                    "▸ " + ctrl.LastResultText, feedSt);
            }

            // 닫기 버튼
            var closeRect = new Rect(x + w - 96, y + 8, 80, 24);
            if (UITheme.GhostButton(closeRect, "[ESC]"))
                ctrl.Close();
        }

        private void DrawQuizResultToast(SecurityQuizController ctrl)
        {
            float w = Mathf.Min(660, Screen.width - 80);

            var bodySt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14, wordWrap = true,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = UITheme.Ink }
            };
            float contentW = w - 80;
            float contentH = bodySt.CalcHeight(new GUIContent(ctrl.LastResultText), contentW);
            float h = Mathf.Max(96, contentH + 56);

            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.2f;

            var rect = new Rect(x, y, w, h);
            UITheme.DrawRect(rect, UITheme.Bg2);
            UITheme.DrawBorder(rect, UITheme.NeonGreen, 1f);
            UITheme.DrawRect(new Rect(x, y, 3f, h), UITheme.NeonGreen);

            UITheme.DrawPulseDot(new Vector2(x + 22, y + 22), UITheme.NeonGreen, 4f);
            var tagSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10, fontStyle = FontStyle.Bold,
                normal = { textColor = UITheme.NeonGreen }
            };
            GUI.Label(new Rect(x + 36, y + 14, w - 60, 14),
                "▸ CORRECT // SECURITY MODULE PASSED", tagSt);

            GUI.Label(new Rect(x + 36, y + 36, contentW, contentH),
                ctrl.LastResultText, bodySt);
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            var console = FindConsole();
            if (console != null && console.IsMenuOpen && kb.escapeKey.wasPressedThisFrame)
                console.Close();

            // 인벤토리 토글 (I) — 다른 모달 없을 때만
            HandleInventoryToggle(kb);

            var rmc = RacingMissionController.Instance;
            if (rmc != null)
            {
                if (rmc.CurrentPhase == RacingMissionController.Phase.AwaitingRank
                    && kb.escapeKey.wasPressedThisFrame)
                    rmc.Cancel();
                else if (rmc.CurrentPhase == RacingMissionController.Phase.Finished
                    && (kb.spaceKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame
                        || kb.enterKey.wasPressedThisFrame))
                    rmc.Acknowledge();
            }

            var s = GameSession.Instance;
            if (s != null && s.Outcome != RunOutcome.Ongoing)
            {
                if (kb.rKey.wasPressedThisFrame) Restart();
                else if (kb.mKey.wasPressedThisFrame) ReturnToMenu();
            }
        }

        private void Restart()
        {
            var s = GameSession.Instance;
            if (s != null) s.StartNewRun();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void ReturnToMenu()
        {
            // 메인 메뉴 씬이 빌드 인덱스에 없으면 그냥 재시작
            const string menuScene = "MainMenuScene";
            if (Application.CanStreamedLevelBeLoaded(menuScene))
                SceneManager.LoadScene(menuScene);
            else
                Restart();
        }

        private void DrawInteractionPrompt()
        {
            // 대화창이 활성이면 상호작용 프롬프트 숨김
            var ds = DialogueSystem.Instance;
            if (ds != null && ds.IsActive) return;

            var p = PlayerInteractor.Instance;
            if (p == null || p.Nearest == null) return;

            // Hide prompt while accusation modal open
            var console = FindConsole();
            if (console != null && console.IsMenuOpen) return;

            string prompt = p.Nearest.PromptText;

            float w = 480;
            float h = 52;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height - 150;

            var rect = new Rect(x, y, w, h);
            UITheme.DrawRect(rect, UITheme.Bg2);
            UITheme.DrawBorder(rect, UITheme.NeonCyan, 1f);

            // 좌측 펄스 dot
            UITheme.DrawPulseDot(new Vector2(x + 18, y + h * 0.5f), UITheme.NeonCyan, 3f);

            // 좌측 '▸' 화살표 + 텍스트
            var promptSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = UITheme.Ink }
            };
            GUI.Label(rect, "▸ " + prompt, promptSt);
        }

        private void DrawToast()
        {
            // 대화창이 활성이면 토스트 숨김
            var ds = DialogueSystem.Instance;
            if (ds != null && ds.IsActive) return;

            var p = PlayerInteractor.Instance;
            if (p == null || string.IsNullOrEmpty(p.LastInteractionResult)) return;
            float elapsed = Time.time - p.LastInteractionTime;
            if (elapsed > 4f) return;

            float w = Mathf.Min(700, Screen.width - 80);
            float contentW = w - 60;

            var bodySt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15, wordWrap = true,
                normal = { textColor = UITheme.Ink }
            };
            float contentH = bodySt.CalcHeight(new GUIContent(p.LastInteractionResult), contentW);

            float headerH = 28;
            float pad = 14;
            float h = headerH + contentH + pad * 2;

            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.24f;

            // 페이드아웃
            float alpha = Mathf.Clamp01(1f - (elapsed - 3f));
            var prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);

            var rect = new Rect(x, y, w, h);
            UITheme.DrawRect(rect, UITheme.Bg2);
            UITheme.DrawBorder(rect, UITheme.NeonYellow, 1f);

            // 좌측 강조선
            UITheme.DrawRect(new Rect(x, y, 3f, h), UITheme.NeonYellow);

            // 헤더 라벨
            var headerSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10, fontStyle = FontStyle.Bold,
                normal = { textColor = UITheme.NeonYellow }
            };
            GUI.Label(new Rect(x + 18, y + 10, w - 36, 16),
                "▸ INTEL // RELAY", headerSt);

            // 본문
            GUI.Label(new Rect(x + pad + 14, y + pad + headerH, contentW, contentH),
                p.LastInteractionResult, bodySt);

            GUI.color = prev;
        }

        private void DrawAccusationModal()
        {
            var console = FindConsole();
            if (console == null || !console.IsMenuOpen) return;

            // 어둠 오버레이
            UITheme.DrawRect(new Rect(0, 0, Screen.width, Screen.height),
                new Color(UITheme.Bg0.r, UITheme.Bg0.g, UITheme.Bg0.b, 0.88f));
            UITheme.DrawScanlines(new Rect(0, 0, Screen.width, Screen.height), 0.05f);

            float w = Mathf.Min(680, Screen.width - 80);
            float h = 440;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            var rect = new Rect(x, y, w, h);

            // 패널 + 매젠타 보더 (위협적인 톤)
            UITheme.DrawRect(rect, UITheme.Bg1);
            UITheme.DrawBorder(rect, UITheme.NeonMagenta, 1f);

            // 윈도우 헤더
            UITheme.DrawWinBar(new Rect(x, y, w, 32), "verdict.terminal");

            // 헤더 섹션
            float headerY = y + 50;
            UITheme.DrawPulseDot(new Vector2(x + 26, headerY + 14), UITheme.NeonMagenta, 4f);

            var tagSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10, fontStyle = FontStyle.Bold,
                normal = { textColor = UITheme.NeonMagenta }
            };
            GUI.Label(new Rect(x + 40, headerY + 4, w - 80, 14),
                "▸ FINAL ACCUSATION // IRREVERSIBLE", tagSt);

            var titleSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24, fontStyle = FontStyle.Bold,
                normal = { textColor = UITheme.Ink }
            };
            GUI.Label(new Rect(x + 40, headerY + 18, w - 80, 32),
                "산업스파이 지목", titleSt);

            // 구분선
            UITheme.DrawRect(new Rect(x + 24, headerY + 60, w - 48, 1), UITheme.Line);

            // 본문 설명
            var bodySt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13, wordWrap = true,
                normal = { textColor = UITheme.InkDim }
            };
            GUI.Label(new Rect(x + 30, headerY + 70, w - 60, 60),
                "수집한 단서를 바탕으로 산업스파이를 지목하라.\n정답이면 승리, 오답이면 패배 — 되돌릴 수 없다.",
                bodySt);

            // 용의자 버튼 리스트
            var roster = NPCRoster.Instance;
            int n = roster != null ? roster.npcs.Count : 0;
            float btnH = 50;
            float gap = 8;
            float startY = y + h - (btnH + gap) * Mathf.Max(1, n) - 24;

            for (int i = 0; i < n; i++)
            {
                var r = new Rect(x + 30, startY + (btnH + gap) * i, w - 60, btnH);
                string label = roster.npcs[i] != null ? roster.npcs[i].DisplayName : "?";
                string display = $"  0{i + 1}    {label}";

                bool hover = r.Contains(UITheme.GetMousePos());
                UITheme.DrawRect(r, hover ? new Color(UITheme.NeonMagenta.r, UITheme.NeonMagenta.g,
                    UITheme.NeonMagenta.b, 0.14f) : UITheme.Bg2);
                UITheme.DrawBorder(r, hover ? UITheme.NeonMagenta : UITheme.Line, hover ? 2f : 1f);

                var btnSt = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14, fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = hover ? UITheme.NeonMagenta : UITheme.Ink }
                };
                GUI.Label(r, display, btnSt);

                if (GUI.Button(r, GUIContent.none, GUIStyle.none))
                    console.Accuse(i);
            }
        }

        private void DrawEndScreen()
        {
            var s = GameSession.Instance;
            if (s == null || s.Outcome == RunOutcome.Ongoing) return;

            bool win = s.Outcome == RunOutcome.Win;
            Color accent = win ? UITheme.NeonGreen : UITheme.Danger;

            // 풀스크린 어둠 + 스캔라인
            UITheme.DrawRect(new Rect(0, 0, Screen.width, Screen.height),
                new Color(UITheme.Bg0.r, UITheme.Bg0.g, UITheme.Bg0.b, 0.92f));
            UITheme.DrawScanlines(new Rect(0, 0, Screen.width, Screen.height), 0.06f);

            float w = Mathf.Min(760, Screen.width - 80);
            float h = 360;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;
            var rect = new Rect(x, y, w, h);

            // 패널
            UITheme.DrawRect(rect, UITheme.Bg1);
            UITheme.DrawBorder(rect, accent, 1f);

            // 윈도우 헤더
            UITheme.DrawWinBar(new Rect(x, y, w, 32), "mission-result.dossier");

            // 상태 태그 (좌측 상단)
            float headerY = y + 50;
            UITheme.DrawPulseDot(new Vector2(x + 28, headerY + 14), accent, 5f);

            var tagSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11, fontStyle = FontStyle.Bold,
                normal = { textColor = accent }
            };
            GUI.Label(new Rect(x + 44, headerY + 4, w - 80, 16),
                win ? "▸ MISSION SUCCESS // SUSPECT IDENTIFIED"
                    : "▸ MISSION FAILED // INVESTIGATION COMPROMISED", tagSt);

            // 메인 결과 라벨
            var bigSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 56, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = accent }
            };
            GUI.Label(new Rect(x, y + 90, w, 80), win ? "VICTORY" : "DEFEAT", bigSt);

            // 부제 한국어
            var subKoSt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = UITheme.InkDim }
            };
            GUI.Label(new Rect(x, y + 156, w, 26), win ? "사건 해결" : "조사 실패", subKoSt);

            // 본문 메시지
            var bodySt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14, wordWrap = true,
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = UITheme.Ink }
            };
            GUI.Label(new Rect(x + 40, y + 192, w - 80, 60),
                s.OutcomeMessage, bodySt);

            // 버튼 영역
            float btnW = 230, btnH = 46, gap = 14;
            float twoW = btnW * 2 + gap;
            float bx = x + (w - twoW) * 0.5f;
            float by = y + h - 70;

            if (UITheme.NeonButton(new Rect(bx, by, btnW, btnH),
                "▸ RESTART  [R]", win ? UITheme.NeonGreen : UITheme.NeonCyan))
                Restart();

            if (UITheme.GhostButton(new Rect(bx + btnW + gap, by, btnW, btnH),
                "▸ MAIN MENU  [M]"))
                ReturnToMenu();
        }

        private void DrawHint()
        {
            // 레이싱 WebView 진행 중엔 하단 안내도 숨김 (WebView 위로 떠 보이지 않게)
            var bridge = RacingWebViewBridge.Instance;
            if (bridge != null && bridge.IsShowing) return;

            float h = 22;
            var bgRect = new Rect(0, Screen.height - h, Screen.width, h);
            UITheme.DrawRect(bgRect, new Color(UITheme.Bg0.r, UITheme.Bg0.g, UITheme.Bg0.b, 0.85f));
            UITheme.DrawRect(new Rect(0, Screen.height - h, Screen.width, 1), UITheme.Line);

            var st = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = UITheme.InkFaint }
            };
            GUI.Label(new Rect(14, Screen.height - h, Screen.width - 28, h),
                "// [WASD] 이동  ·  [SHIFT] 달리기  ·  [휠] 줌  ·  [SPACE] 상호작용  ·  [I] 인벤토리  ·  [ESC] 닫기  ·  [R] 재시작  ·  [M] 메인메뉴",
                st);
        }

        /// <summary>I키로 토글되는 인벤토리 — SecureSense Dossier 스타일 단서 아카이브</summary>
        private void DrawInventoryPanel()
        {
            if (!IsInventoryOpen) return;
            var s = GameSession.Instance;
            if (s == null) return;

            // ── 전체 어둠 오버레이 ──
            UITheme.DrawRect(new Rect(0, 0, Screen.width, Screen.height),
                new Color(UITheme.Bg0.r, UITheme.Bg0.g, UITheme.Bg0.b, 0.88f));
            UITheme.DrawScanlines(new Rect(0, 0, Screen.width, Screen.height), 0.04f);

            float w = Mathf.Min(900, Screen.width - 60);
            float h = Mathf.Min(700, Screen.height - 80);
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            var panelRect = new Rect(x, y, w, h);

            // ── 패널 (윈도우 chrome + 보더) ──
            UITheme.DrawRect(panelRect, UITheme.Bg1);
            UITheme.DrawBorder(panelRect, UITheme.LineStrong, 1f);

            // 윈도우 헤더 바
            var winBarRect = new Rect(x, y, w, 32);
            UITheme.DrawWinBar(winBarRect, "evidence-archive.dossier");

            // ── 헤더 섹션 ──
            float headerY = y + 44;
            // 좌측 펄스 dot
            UITheme.DrawPulseDot(new Vector2(x + 22, headerY + 14), UITheme.NeonGreen, 4f);

            var sectionTitle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = UITheme.NeonGreen }
            };
            GUI.Label(new Rect(x + 36, headerY, w - 200, 28),
                "▸ EVIDENCE COLLECTED", sectionTitle);

            // 우측 카운트 태그
            int count = s.CollectedClues.Count;
            string countLabel = $"{count:D2} / ∞ ITEMS";
            float tagW = 120;
            UITheme.DrawTag(new Rect(x + w - tagW - 24, headerY + 2, tagW, 22),
                countLabel, UITheme.NeonCyan);

            // 닫기 안내
            var hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = UITheme.InkFaint }
            };
            GUI.Label(new Rect(x + 24, headerY + 28, w - 48, 16),
                "// PRESS [I] OR [ESC] TO CLOSE", hintStyle);

            // 헤더 하단 구분선
            UITheme.DrawRect(new Rect(x + 20, headerY + 50, w - 40, 1), UITheme.Line);

            // ── 비어있을 때 ──
            if (count == 0)
            {
                var emptyTitle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16, fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = UITheme.InkDim }
                };
                GUI.Label(new Rect(x + 40, y + h * 0.42f, w - 80, 28),
                    "// NO ENTRIES IN ARCHIVE", emptyTitle);

                var emptyBody = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13, wordWrap = true,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = UITheme.InkFaint }
                };
                GUI.Label(new Rect(x + 40, y + h * 0.48f, w - 80, 60),
                    "NPC와 대화하거나 환경 단서를 조사하면\n수집한 정보가 자동으로 기록됩니다.",
                    emptyBody);
                return;
            }

            // ── 카드 리스트 (스크롤뷰) ──
            float listY = headerY + 60;
            float listH = h - (listY - y) - 20;
            var viewRect = new Rect(x + 20, listY, w - 40, listH);

            float cardW = viewRect.width - 24;
            float gap = 8;

            // 카드 본문 텍스트 스타일
            var cardBodySt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13, wordWrap = true,
                normal = { textColor = UITheme.Ink }
            };

            // 콘텐츠 높이 계산
            var heights = new List<float>(count);
            float total = 0;
            foreach (var c in s.CollectedClues)
            {
                float bodyH = cardBodySt.CalcHeight(new GUIContent(c.text), cardW - 60);
                float cardH = bodyH + 60;
                heights.Add(cardH);
                total += cardH + gap;
            }

            var contentRect = new Rect(0, 0, cardW, total);
            inventoryScroll = GUI.BeginScrollView(viewRect, inventoryScroll, contentRect);

            float cy = 0;
            for (int i = 0; i < count; i++)
            {
                var c = s.CollectedClues[i];
                float cardH = heights[i];
                var cardRect = new Rect(0, cy, cardW, cardH);

                // 출처별 색깔 (SecureSense 네온 매핑)
                Color srcColor;
                string srcLabel;
                switch (c.source)
                {
                    case ClueSource.Environment:
                        srcColor = UITheme.NeonYellow; srcLabel = "ENV·LOG"; break;
                    case ClueSource.NPC:
                        srcColor = UITheme.NeonCyan; srcLabel = "TESTIMONY"; break;
                    case ClueSource.Minigame:
                        srcColor = UITheme.NeonGreen; srcLabel = "MISSION"; break;
                    default:
                        srcColor = UITheme.NeonViolet; srcLabel = "MISC"; break;
                }

                // 카드 배경 + 보더
                UITheme.DrawRect(cardRect, UITheme.Bg2);
                UITheme.DrawBorder(cardRect, UITheme.Line, 1f);

                // 좌측 강조선 (출처 색)
                UITheme.DrawRect(new Rect(cardRect.x, cardRect.y, 3f, cardH), srcColor);

                // 카드 번호 (왼쪽 상단)
                var idxStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 10,
                    normal = { textColor = UITheme.InkFaint }
                };
                GUI.Label(new Rect(cardRect.x + 14, cardRect.y + 8, 40, 14),
                    $"#{(i + 1):D3}", idxStyle);

                // 제목
                var titleSt = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15, fontStyle = FontStyle.Bold,
                    normal = { textColor = UITheme.Ink }
                };
                GUI.Label(new Rect(cardRect.x + 50, cardRect.y + 6, cardW - 200, 22),
                    c.title, titleSt);

                // 출처 태그 (우측 상단)
                float tagWidth = 100;
                UITheme.DrawTag(new Rect(cardRect.x + cardW - tagWidth - 14, cardRect.y + 8,
                    tagWidth, 18), srcLabel, srcColor);

                // 본문
                GUI.Label(new Rect(cardRect.x + 18, cardRect.y + 34, cardW - 36, cardH - 42),
                    c.text, cardBodySt);

                cy += cardH + gap;
            }

            GUI.EndScrollView();
        }
    }
}
