using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ForTheCompany.Systems
{
    /// <summary>
    /// 화면 하단 RPG 스타일 대화 시스템.
    /// StartDialogue(speaker, lines)로 시작 — 라인이 typewriter 효과로 한 글자씩 표시.
    /// Space/Enter/E로 다음 라인. 타이핑 중이면 즉시 완성, 완성 상태면 다음 라인.
    /// 마지막 라인 후 OnEnded 콜백 호출.
    /// </summary>
    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance { get; private set; }

        [Header("Typewriter")]
        public float charsPerSecond = 45f;

        // 현재 대화 상태
        public bool IsActive { get; private set; }
        public string CurrentSpeaker { get; private set; }
        public string CurrentFullLine { get; private set; }
        public string CurrentVisibleLine { get; private set; }
        public bool LineComplete { get; private set; }
        public bool IsLastLine => lines != null && lineIndex >= lines.Count - 1;

        public float OpenTime { get; private set; } // 슬라이드 인 애니메이션용
        public Transform CurrentNPCTransform { get; private set; } // 카메라 줌인 대상

        private List<string> lines;
        private int lineIndex;
        private float charProgress;
        private Action onEnded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            EnsureSpawned();
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureSpawned();
        }

        private static void EnsureSpawned()
        {
            if (SceneManager.GetActiveScene().name != "FacilityScene") return;
            if (FindFirstObjectByType<DialogueSystem>() != null) return;
            var go = new GameObject("DialogueSystem");
            go.AddComponent<DialogueSystem>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        /// <summary>여러 라인 대화 시작. 종료 시 onEnded 콜백 호출.</summary>
        public void StartDialogue(string speaker, IEnumerable<string> textLines,
            Transform npcTransform = null, Action onEnded = null)
        {
            lines = new List<string>();
            foreach (var t in textLines)
            {
                if (string.IsNullOrEmpty(t)) continue;
                lines.Add(t);
            }
            if (lines.Count == 0) return;

            CurrentSpeaker = speaker ?? "";
            CurrentNPCTransform = npcTransform;
            lineIndex = 0;
            this.onEnded = onEnded;
            IsActive = true;
            OpenTime = Time.time;
            StartLine();
        }

        /// <summary>한 줄짜리 간편 호출 — \n\n으로 분할</summary>
        public void StartDialogue(string speaker, string text,
            Transform npcTransform = null, Action onEnded = null)
        {
            if (string.IsNullOrEmpty(text)) return;
            var split = text.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            StartDialogue(speaker, split, npcTransform, onEnded);
        }

        private void StartLine()
        {
            CurrentFullLine = lines[lineIndex];
            CurrentVisibleLine = "";
            charProgress = 0f;
            LineComplete = false;
        }

        private void Update()
        {
            if (!IsActive) return;

            // 타이프라이터 진행
            if (!LineComplete)
            {
                charProgress += Time.deltaTime * charsPerSecond;
                int visible = Mathf.Min(CurrentFullLine.Length, Mathf.FloorToInt(charProgress));
                CurrentVisibleLine = CurrentFullLine.Substring(0, visible);
                if (visible >= CurrentFullLine.Length)
                {
                    LineComplete = true;
                }
            }

            // 시작 직후 0.2초는 입력 무시 — E키로 대화를 막 시작한 같은 프레임에
            // wasPressedThisFrame이 또 감지되어 즉시 완성/스킵되는 것 방지
            if (Time.time - OpenTime < 0.2f) return;

            // 진행 입력 (Space / Enter / E)
            var kb = Keyboard.current;
            if (kb == null) return;
            bool advance = kb.spaceKey.wasPressedThisFrame
                || kb.enterKey.wasPressedThisFrame
                || kb.eKey.wasPressedThisFrame;
            if (!advance) return;

            if (!LineComplete)
            {
                // 타이핑 중 → 즉시 완성
                CurrentVisibleLine = CurrentFullLine;
                charProgress = CurrentFullLine.Length;
                LineComplete = true;
            }
            else
            {
                // 다음 라인 또는 종료
                lineIndex++;
                if (lineIndex >= lines.Count)
                {
                    End();
                }
                else
                {
                    StartLine();
                }
            }
        }

        private void End()
        {
            IsActive = false;
            lines = null;
            CurrentSpeaker = "";
            CurrentFullLine = "";
            CurrentVisibleLine = "";
            CurrentNPCTransform = null;
            var cb = onEnded;
            onEnded = null;
            cb?.Invoke();
        }

        /// <summary>외부에서 강제 종료 (콜백 호출 안 함)</summary>
        public void ForceClose()
        {
            IsActive = false;
            lines = null;
            onEnded = null;
            CurrentSpeaker = "";
            CurrentFullLine = "";
            CurrentVisibleLine = "";
            CurrentNPCTransform = null;
        }
    }
}
