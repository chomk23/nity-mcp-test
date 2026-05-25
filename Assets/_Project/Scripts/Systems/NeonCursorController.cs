using UnityEngine;
using UnityEngine.SceneManagement;

namespace ForTheCompany.Systems
{
    /// <summary>
    /// 시스템 마우스 커서를 숨기고 SecureSense 네온 십자 커서를 OnGUI로 그림.
    /// DontDestroyOnLoad로 모든 씬에서 작동.
    /// 가장 마지막에 그려져 어떤 UI 위에도 표시되도록 GUI.depth 조정.
    /// </summary>
    public class NeonCursorController : MonoBehaviour
    {
        public static NeonCursorController Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            // 모든 씬에서 자동 실행
            if (Instance != null) return;
            var go = new GameObject("NeonCursorController");
            DontDestroyOnLoad(go);
            go.AddComponent<NeonCursorController>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;
        }

        private void Update()
        {
            // 매 프레임 시스템 커서 숨김 강제 (다른 코드가 켜도 즉시 꺼지도록)
            if (Cursor.visible) Cursor.visible = false;
        }

        private void OnGUI()
        {
            // GUI.depth를 음수로 — 다른 OnGUI 위에 그려지도록
            GUI.depth = -1000;
            var mp = UITheme.GetMousePos();
            // 화면 밖이면 그리지 않음
            if (mp.x < 0 || mp.y < 0 || mp.x > Screen.width || mp.y > Screen.height) return;
            UITheme.DrawNeonCursor(mp);
        }
    }
}
