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

        private void OnGUI()
        {
            Init();
            DrawNPCNameplates();
            DrawStatusBar();
            DrawObjectivePanel();
            DrawQuestAdvanceToast();
            DrawInteractionPrompt();
            DrawToast();
            DrawQuizModal();
            DrawRacingModal();
            DrawAccusationModal();
            DrawDialogueBox();
            DrawEndScreen();
            DrawHint();
        }

        /// <summary>RPG 스타일 하단 대화창 — 슬라이드 인 + 타이프라이터 + 진행 표시</summary>
        private void DrawDialogueBox()
        {
            var ds = DialogueSystem.Instance;
            if (ds == null || !ds.IsActive) return;

            // 슬라이드 인 애니메이션 (0.25초)
            float t = Mathf.Clamp01((Time.time - ds.OpenTime) / 0.25f);
            float eased = 1f - Mathf.Pow(1f - t, 3f); // ease-out cubic

            float boxW = Mathf.Min(Screen.width - 60, 1100);
            float boxH = 180;
            float boxX = (Screen.width - boxW) * 0.5f;
            float targetY = Screen.height - boxH - 40;
            float startY = Screen.height + 20;
            float boxY = Mathf.Lerp(startY, targetY, eased);

            // 메인 박스
            DrawSolidRect(new Rect(boxX, boxY, boxW, boxH),
                new Color(0.04f, 0.05f, 0.12f, 0.92f));
            // 박스 윗변 강조선
            DrawSolidRect(new Rect(boxX, boxY, boxW, 2f),
                new Color(0.5f, 0.85f, 1f, 0.9f));

            // 좌측 상단 NPC 이름 헤더 (박스 위로 살짝 튀어나옴)
            if (!string.IsNullOrEmpty(ds.CurrentSpeaker))
            {
                float nameW = Mathf.Min(260f, ds.CurrentSpeaker.Length * 22f + 40f);
                float nameH = 38f;
                float nameX = boxX + 24f;
                float nameY = boxY - nameH * 0.5f;
                DrawSolidRect(new Rect(nameX, nameY, nameW, nameH),
                    new Color(0.12f, 0.18f, 0.35f, 0.95f));
                DrawSolidRect(new Rect(nameX, nameY, nameW, 2f),
                    new Color(0.7f, 0.9f, 1f, 0.95f));

                var nameStyle = new GUIStyle(midStyle)
                {
                    fontSize = 20, fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.95f, 0.95f, 1f) }
                };
                GUI.Label(new Rect(nameX, nameY, nameW, nameH), ds.CurrentSpeaker, nameStyle);
            }

            // 본문 텍스트 (타이프라이터 진행 중인 부분)
            var bodyStyle = new GUIStyle(midStyle)
            {
                fontSize = 22, wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = new Color(0.95f, 0.95f, 0.98f) }
            };
            float padX = 36, padTop = 28, padBottom = 36;
            GUI.Label(new Rect(boxX + padX, boxY + padTop, boxW - padX * 2, boxH - padTop - padBottom),
                ds.CurrentVisibleLine, bodyStyle);

            // 우하단 진행 표시 (타이핑 끝났을 때 깜빡임)
            if (ds.LineComplete)
            {
                float blink = Mathf.Abs(Mathf.Sin(Time.time * 3f));
                var prev = GUI.color;
                GUI.color = new Color(1f, 0.9f, 0.5f, 0.5f + 0.5f * blink);
                var promptSt = new GUIStyle(smallStyle)
                {
                    fontSize = 16, alignment = TextAnchor.MiddleRight,
                    normal = { textColor = new Color(1f, 0.9f, 0.5f) }
                };
                string hint = ds.IsLastLine ? "▼ Space — 대화 종료" : "▼ Space — 다음";
                GUI.Label(new Rect(boxX + boxW - 280, boxY + boxH - 32, 260, 24), hint, promptSt);
                GUI.color = prev;
            }
        }

        /// <summary>우상단에 현재 목표 패널 표시 (스토리 모드 안내). 텍스트 길이에 따라 동적 높이.</summary>
        private void DrawObjectivePanel()
        {
            var quest = QuestManager.Instance;
            if (quest == null || quest.CurrentStage == QuestManager.Stage.Done) return;

            // wordWrap 보장된 스타일 (한글 길이 대응)
            int total = (int)QuestManager.Stage.Done; // 6단계
            int step = (int)quest.CurrentStage + 1;

            var headerStyle = new GUIStyle(midStyle)
            { wordWrap = true, normal = { textColor = new Color(1f, 0.85f, 0.5f) } };
            var bodyStyle = new GUIStyle(midStyle) { wordWrap = true };
            var hintStyle = new GUIStyle(smallStyle)
            { wordWrap = true, normal = { textColor = new Color(0.7f, 0.85f, 1f) } };

            float w = 340;
            float contentW = w - 28;
            string header = $"현재 목표  ({step}/{total})";

            float headerH = headerStyle.CalcHeight(new GUIContent(header), contentW);
            float bodyH = bodyStyle.CalcHeight(new GUIContent(quest.CurrentObjective), contentW);
            float hintH = hintStyle.CalcHeight(new GUIContent(quest.CurrentLocationHint), contentW);

            float pad = 10, gap = 6;
            float h = pad * 2 + headerH + gap + bodyH + gap + hintH;

            float x = Screen.width - w - 10;
            float y = 10;
            GUI.Box(new Rect(x, y, w, h), GUIContent.none);

            float cy = y + pad;
            GUI.Label(new Rect(x + 14, cy, contentW, headerH), header, headerStyle);
            cy += headerH + gap;
            GUI.Label(new Rect(x + 14, cy, contentW, bodyH), quest.CurrentObjective, bodyStyle);
            cy += bodyH + gap;
            GUI.Label(new Rect(x + 14, cy, contentW, hintH), quest.CurrentLocationHint, hintStyle);
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

            float w = 600;
            float contentW = w - 40;
            var bodySt = new GUIStyle(midStyle)
            { alignment = TextAnchor.UpperCenter, wordWrap = true };
            float bodyH = bodySt.CalcHeight(new GUIContent(quest.LastAdvanceText), contentW);

            float headerH = 36;
            float pad = 16;
            float h = headerH + bodyH + pad * 2;

            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.28f;
            float alpha = Mathf.Clamp01(1f - (elapsed - 3f));
            var prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);

            GUI.Box(new Rect(x, y, w, h), GUIContent.none);
            var titleSt = new GUIStyle(bigStyle)
            { alignment = TextAnchor.UpperCenter,
              normal = { textColor = new Color(1f, 0.9f, 0.5f) } };
            GUI.Label(new Rect(x, y + pad, w, headerH), "단계 완료!", titleSt);
            GUI.Label(new Rect(x + 20, y + pad + headerH, contentW, bodyH),
                quest.LastAdvanceText, bodySt);
            GUI.color = prev;
        }

        private void DrawRacingModal()
        {
            var rmc = RacingMissionController.Instance;
            if (rmc == null || !rmc.IsOpen) return;

            // 임베드 WebView가 보이는 중엔 Unity 등수 입력 모달 숨김
            // (HTML 끝나면 uwb.ExecuteJsMethod로 자동 보고됨)
            var bridge = RacingWebViewBridge.Instance;
            bool isEmbedded = bridge != null && bridge.IsShowing;
            if (rmc.CurrentPhase == RacingMissionController.Phase.AwaitingRank && isEmbedded)
            {
                // ESC로 강제 종료 버튼만 작은 모달로
                DrawRacingEmbeddedClose(rmc);
                return;
            }

            float w = Mathf.Min(640, Screen.width - 80);
            float h = 460;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            // Background (neon frame)
            var bgColor = new Color(0.04f, 0.01f, 0.1f, 1f);
            DrawSolidRect(new Rect(x, y, w, h), bgColor);
            DrawSolidRect(new Rect(x, y, w, 4), new Color(0f, 0.94f, 1f));
            DrawSolidRect(new Rect(x, y + h - 4, w, 4), new Color(1f, 0.18f, 0.58f));

            var titleS = new GUIStyle(titleStyle) { normal = { textColor = new Color(0f, 0.94f, 1f) } };
            GUI.Label(new Rect(x, y + 18, w, 38), "SECURITY RACE", titleS);

            if (rmc.CurrentPhase == RacingMissionController.Phase.AwaitingRank)
                DrawRankInputModal(rmc, x, y, w, h);
            else if (rmc.CurrentPhase == RacingMissionController.Phase.Finished)
                DrawRacingFinished(rmc, x, y, w, h);
        }

        private void DrawRacingEmbeddedClose(RacingMissionController rmc)
        {
            // 우상단에 작은 닫기 안내 (HTML이 자동 결과 보고하지만 ESC로 수동 종료 가능)
            float w = 320, h = 56;
            float x = Screen.width - w - 20;
            float y = 20;
            GUI.Box(new Rect(x, y, w, h), GUIContent.none);
            GUI.Label(new Rect(x + 12, y + 4, w - 24, 20),
                "SECURITY RACE 진행 중", midStyle);
            GUI.Label(new Rect(x + 12, y + 28, w - 24, 20),
                "ESC: 강제 종료 (보상 없음)", smallStyle);
        }

        private void DrawRankInputModal(RacingMissionController rmc, float x, float y, float w, float h)
        {
            GUI.Label(new Rect(x + 24, y + 70, w - 48, 90),
                "SECURITY RACE를 플레이하셨습니다.\n\n1등으로 결승선을 통과하면 클리어, 그 외엔 재시도하세요.",
                bodyStyle);

            float btnH = 70;
            float gap = 12;
            float startY = y + 200;

            DrawRankButton(new Rect(x + 24, startY, w - 48, btnH),
                "🥇  1등 했음 — 클리어 (+5 단서)",
                new Color(1f, 0.85f, 0.2f), () => rmc.ReportRank(1));

            DrawRankButton(new Rect(x + 24, startY + (btnH + gap), w - 48, btnH),
                "1등 못 함 — 재시도 (보상 없음)",
                new Color(0.55f, 0.55f, 0.55f), () => rmc.Cancel());
        }

        private void DrawRankButton(Rect r, string label, Color tint, System.Action onClick)
        {
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = tint;
            var s = new GUIStyle(btnStyle)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.05f, 0.02f, 0.1f) }
            };
            if (GUI.Button(r, label, s)) onClick?.Invoke();
            GUI.backgroundColor = prev;
        }

        private void DrawRacingFinished(RacingMissionController rmc, float x, float y, float w, float h)
        {
            string rankLabel = rmc.FinalRank == 1 ? "🥇 1등"
                             : rmc.FinalRank == 2 ? "🥈 2등"
                             : "🥉 3등";

            var rankS = new GUIStyle(endStyle)
            {
                normal = { textColor = rmc.FinalRank == 1
                    ? new Color(1f, 0.85f, 0.2f)
                    : new Color(0.85f, 0.92f, 1f) }
            };
            GUI.Label(new Rect(x, y + 90, w, 80), rankLabel, rankS);

            GUI.Label(new Rect(x + 30, y + 200, w - 60, 60),
                rmc.ResultMessage, bodyStyle);

            if (GUI.Button(new Rect(x + (w - 240) * 0.5f, y + h - 80, 240, 56),
                "확인 (E / ESC / Enter)", btnStyle))
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

            foreach (var npc in roster.npcs)
            {
                if (npc == null) continue;
                Vector3 worldHead = npc.transform.position + Vector3.up * 2.4f;
                Vector3 screen = cam.WorldToScreenPoint(worldHead);
                if (screen.z < 0) continue;

                float guiY = Screen.height - screen.y;
                string name = npc.DisplayName;
                float w = 150f, h = 26f;
                var rect = new Rect(screen.x - w * 0.5f, guiY - h * 0.5f, w, h);
                GUI.Box(rect, GUIContent.none);
                GUI.Label(rect, name, namePlateStyle);

                // 의심도 막대 (이름표 바로 아래)
                int sus = npc.suspicion;
                if (sus > 0)
                {
                    const float MaxSuspicion = 10f;
                    float barW = 120f, barH = 6f;
                    float barX = screen.x - barW * 0.5f;
                    float barY = guiY + h * 0.5f + 4f;

                    // 배경 (어둡게)
                    DrawSolidRect(new Rect(barX, barY, barW, barH),
                        new Color(0f, 0f, 0f, 0.65f));

                    float pct = Mathf.Clamp01(sus / MaxSuspicion);
                    Color susColor = sus >= 7 ? new Color(1f, 0.35f, 0.35f) // 빨강
                        : sus >= 4 ? new Color(1f, 0.85f, 0.3f) // 노랑
                        : new Color(0.7f, 0.7f, 0.75f);          // 회색
                    DrawSolidRect(new Rect(barX, barY, barW * pct, barH), susColor);
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

            float w = Mathf.Min(740, Screen.width - 80);
            float h = 560;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            GUI.Box(new Rect(x, y, w, h), GUIContent.none);
            GUI.Label(new Rect(x, y + 18, w, 40), "보안 교육 미션", titleStyle);
            GUI.Label(new Rect(x + 24, y + 64, w - 48, 28),
                $"[{data.roomName}] {data.objectLabel}", midStyle);
            GUI.Label(new Rect(x + 24, y + 100, w - 48, 100), data.quizQuestion, quizQuestionStyle);

            int n = data.quizOptions != null ? data.quizOptions.Length : 0;
            float btnH = 56;
            float gap = 8;
            float startY = y + 210;
            for (int i = 0; i < n; i++)
            {
                var r = new Rect(x + 24, startY + (btnH + gap) * i, w - 48, btnH);
                string label = $"  {(char)('A' + i)}.  {data.quizOptions[i]}";
                if (GUI.Button(r, label, quizButtonStyle))
                    ctrl.Answer(i);
            }

            // Wrong-answer feedback in modal
            if (!string.IsNullOrEmpty(ctrl.LastResultText)
                && !ctrl.LastWasCorrect
                && Time.time - ctrl.LastResultTime < 3.5f)
            {
                GUI.Label(new Rect(x + 24, y + h - 80, w - 48, 40),
                    ctrl.LastResultText,
                    new GUIStyle(bodyStyle) { normal = { textColor = new Color(1f, 0.6f, 0.6f) } });
            }

            // Close button
            if (GUI.Button(new Rect(x + w - 110, y + 12, 96, 32), "닫기 (ESC)", smallStyle))
                ctrl.Close();
        }

        private void DrawQuizResultToast(SecurityQuizController ctrl)
        {
            float w = Mathf.Min(640, Screen.width - 80);
            float h = 120;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.2f;
            GUI.Box(new Rect(x, y, w, h), GUIContent.none);
            GUI.Label(new Rect(x + 16, y + 12, w - 32, h - 24),
                ctrl.LastResultText, quizResultStyle);
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            var console = FindConsole();
            if (console != null && console.IsMenuOpen && kb.escapeKey.wasPressedThisFrame)
                console.Close();

            var rmc = RacingMissionController.Instance;
            if (rmc != null)
            {
                if (rmc.CurrentPhase == RacingMissionController.Phase.AwaitingRank
                    && kb.escapeKey.wasPressedThisFrame)
                    rmc.Cancel();
                else if (rmc.CurrentPhase == RacingMissionController.Phase.Finished
                    && (kb.eKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame
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

        private void DrawStatusBar()
        {
            var s = GameSession.Instance;
            int clues = s != null ? s.totalClues : 0;
            float timeLeft = s != null ? s.TimeRemaining : 0f;

            GUI.Box(new Rect(10, 10, 280, 116), GUIContent.none);
            GUI.Label(new Rect(24, 18, 260, 30), "산업스파이 조사", bigStyle);
            GUI.Label(new Rect(24, 52, 260, 24), $"단서   {clues}", midStyle);

            // 시간 표시 — 30초 이하 빨강, 60초 이하 노랑
            int totalSec = Mathf.Max(0, Mathf.CeilToInt(timeLeft));
            int mm = totalSec / 60;
            int ss = totalSec % 60;
            Color timeCol = timeLeft <= 30f ? new Color(1f, 0.35f, 0.35f)
                : timeLeft <= 60f ? new Color(1f, 0.85f, 0.3f)
                : Color.white;
            var timeStyle = new GUIStyle(midStyle) { normal = { textColor = timeCol } };
            GUI.Label(new Rect(24, 82, 260, 24), $"남은 시간   {mm:D2}:{ss:D2}", timeStyle);
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

            float w = 420;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height - 130;
            GUI.Box(new Rect(x, y, w, 50), GUIContent.none);
            GUI.Label(new Rect(x, y + 8, w, 36), prompt, promptStyle);
        }

        private void DrawToast()
        {
            // 대화창이 활성이면 토스트 숨김
            var ds = DialogueSystem.Instance;
            if (ds != null && ds.IsActive) return;

            var p = PlayerInteractor.Instance;
            if (p == null || string.IsNullOrEmpty(p.LastInteractionResult)) return;
            if (Time.time - p.LastInteractionTime > 4f) return;

            float w = Mathf.Min(680, Screen.width - 80);
            float contentW = w - 40;
            // 텍스트 길이에 맞춰 박스 높이 동적 계산
            float contentH = toastStyle.CalcHeight(new GUIContent(p.LastInteractionResult), contentW);
            float h = contentH + 32;

            // 두 토스트는 순차 표시하므로 NPC 대화 토스트는 항상 본래 위치
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.28f;

            GUI.Box(new Rect(x, y, w, h), GUIContent.none);
            GUI.Label(new Rect(x + 20, y + 16, contentW, contentH),
                p.LastInteractionResult, toastStyle);
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

            float btnW = 240, btnH = 56, gap = 16;
            float twoW = btnW * 2 + gap;
            float bx = x + (w - twoW) * 0.5f;
            float by = y + h - 76;
            if (GUI.Button(new Rect(bx, by, btnW, btnH), "다시 시작 (R)", btnStyle))
                Restart();
            if (GUI.Button(new Rect(bx + btnW + gap, by, btnW, btnH), "메인 메뉴 (M)", btnStyle))
                ReturnToMenu();
        }

        private void DrawHint()
        {
            GUI.Label(new Rect(10, Screen.height - 26, 1200, 22),
                "WASD: 이동   |   Shift: 달리기   |   휠: 줌   |   E: 대화/단서 조사/지목   |   ESC: 모달 닫기   |   R: 재시작   |   M: 메인 메뉴",
                smallStyle);
        }
    }
}
