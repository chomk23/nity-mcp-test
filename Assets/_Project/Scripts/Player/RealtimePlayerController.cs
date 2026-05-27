using UnityEngine;
using UnityEngine.InputSystem;
using ForTheCompany.Systems;

namespace ForTheCompany.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class RealtimePlayerController : MonoBehaviour
    {
        public float walkSpeed = 5f;
        public float runMultiplier = 1.8f;
        public float gravity = -20f;

        private CharacterController controller;
        private float verticalVelocity;

        public bool IsRunning { get; private set; }
        public Vector3 LastInputDirection { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private static bool IsInputBlocked()
        {
            // WebView 임베드 레이싱이 화면에 떠 있으면 캐릭터 조작 차단
            var bridge = RacingWebViewBridge.Instance;
            if (bridge != null && bridge.IsShowing) return true;
            var rmc = RacingMissionController.Instance;
            if (rmc != null && rmc.IsOpen) return true;
            // 보안교육 미션 중 이동 차단
            var sqc = SecurityQuizController.Instance;
            if (sqc != null && sqc.IsOpen) return true;
            // 대화 진행 중에도 이동 차단
            var ds = DialogueSystem.Instance;
            if (ds != null && ds.IsActive) return true;
            // 인벤토리 열림 중에도 이동 차단
            if (FacilityHUD.IsInventoryOpen) return true;
            // 일시정지 메뉴
            var pm = PauseMenu.Instance;
            if (pm != null && pm.IsOpen) return true;
            // 오프닝 컷씬 중
            if (IntroMonologue.IsCutsceneActive) return true;
            // 지목 모달
            var partner = AccusationPartner.Instance;
            if (partner != null && partner.IsMenuOpen) return true;
            return false;
        }

        private void Update()
        {
            var kb = Keyboard.current;
            Vector2 input = Vector2.zero;
            if (kb != null && !IsInputBlocked())
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y += 1;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y -= 1;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x -= 1;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1;
            }

            if (input.sqrMagnitude > 1f) input.Normalize();

            IsRunning = kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed) && input.sqrMagnitude > 0.01f;
            float speed = walkSpeed * (IsRunning ? runMultiplier : 1f);

            Vector3 horizontal = new Vector3(input.x, 0f, input.y) * speed;
            LastInputDirection = horizontal.sqrMagnitude > 0.01f ? horizontal.normalized : LastInputDirection;

            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -1f;
            verticalVelocity += gravity * Time.deltaTime;

            Vector3 motion = horizontal + Vector3.up * verticalVelocity;
            controller.Move(motion * Time.deltaTime);

            if (horizontal.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(horizontal);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 12f * Time.deltaTime);
            }
        }
    }
}
