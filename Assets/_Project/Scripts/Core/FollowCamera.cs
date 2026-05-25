using UnityEngine;
using UnityEngine.InputSystem;
using ForTheCompany.Systems;

namespace ForTheCompany.Core
{
    public class FollowCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 22f, -14f);
        public float followSmooth = 8f;

        [Header("Zoom")]
        [Tooltip("게임 시작 시 기본 줌 (1=원래 크기, 0.7=가까이, 1.5=멀리). minZoom~maxZoom 사이.")]
        [Range(0.3f, 2f)]
        public float defaultZoom = 0.7f;
        public float zoomSpeed = 4f;
        public float minZoom = 0.5f;
        public float maxZoom = 1.8f;
        public float zoomLerp = 10f;

        [Header("Dialogue Mode")]
        [Tooltip("NPC-플레이어 측면 거리 (perpendicular) — 작을수록 줌인")]
        public float dialogueSideDistance = 3.06f;  // 직전 30% 줌인 롤백 (2.14 → 3.06)
        [Tooltip("플레이어 등 뒤로 카메라 추가 오프셋 (양수면 NPC 얼굴이 잘 보임, 음수면 NPC 등이 보임)")]
        public float dialogueForwardOffset = 1.8f;  // 1.26 → 1.8
        [Tooltip("대화 중 카메라 높이 (작을수록 캐릭터 눈높이 시점)")]
        public float dialogueHeight = 2.04f;        // 1.43 → 2.04
        [Tooltip("플레이어 → NPC 중간점에서 NPC 쪽으로 얼마나 치우칠지 (0=플레이어, 1=NPC)")]
        public float dialogueFocusLerp = 0.5f;
        [Tooltip("대화 모드 전환 부드러움")]
        public float dialogueTransitionLerp = 4f;

        private float zoomLevel = 1f;
        private float zoomTarget = 1f;
        private float dialogueBlend = 0f; // 0=일반 모드, 1=대화 모드
        private Vector3 dialogueSideSign = Vector3.right; // 측면 방향 락 (대화 도중 좌우 흔들림 방지)

        private void Awake()
        {
            // 기본 줌 적용 (Inspector의 defaultZoom 값)
            zoomLevel = defaultZoom;
            zoomTarget = defaultZoom;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            HandleZoomInput();

            // 대화 모드 블렌딩
            var ds = DialogueSystem.Instance;
            bool inDialogue = ds != null && ds.IsActive && ds.CurrentNPCTransform != null;
            dialogueBlend = Mathf.Lerp(dialogueBlend, inDialogue ? 1f : 0f,
                dialogueTransitionLerp * Time.deltaTime);

            zoomLevel = Mathf.Lerp(zoomLevel, zoomTarget, zoomLerp * Time.deltaTime);
            Vector3 normalOffset = offset * zoomLevel;

            // 포커스 타겟 + 대화 offset 계산
            // 대화 모드일 때는 캐릭터 발이 아닌 가슴 높이를 LookAt 대상으로 → 정수리 시점 방지
            const float ChestHeightOffset = 1.2f;
            Vector3 focusPos = target.position;
            Vector3 dialogueOff = normalOffset;
            if (ds != null && ds.CurrentNPCTransform != null)
            {
                Vector3 playerChest = target.position + Vector3.up * ChestHeightOffset;
                Vector3 npcChest = ds.CurrentNPCTransform.position + Vector3.up * ChestHeightOffset;
                Vector3 midPos = Vector3.Lerp(playerChest, npcChest, dialogueFocusLerp);
                focusPos = Vector3.Lerp(target.position, midPos, dialogueBlend);

                dialogueOff = ComputeDialogueOffset(target.position, ds.CurrentNPCTransform.position, inDialogue);
            }

            Vector3 blendedOffset = Vector3.Lerp(normalOffset, dialogueOff, dialogueBlend);

            Vector3 desired = focusPos + blendedOffset;
            transform.position = Vector3.Lerp(transform.position, desired,
                followSmooth * Time.deltaTime);
            transform.LookAt(focusPos);
        }

        /// <summary>
        /// 대화 모드 카메라 offset — NPC-플레이어 측면 + NPC 쪽 사선 + 약간 위에서.
        /// 영상처럼 두 캐릭터가 화면에 비스듬히 나란히 보이는 시점.
        /// 측면 방향은 대화 시작 시 한 번 고정해서 카메라가 좌우로 흔들리지 않음.
        /// </summary>
        private Vector3 ComputeDialogueOffset(Vector3 playerPos, Vector3 npcPos, bool justStarting)
        {
            Vector3 toNPC = npcPos - playerPos;
            toNPC.y = 0f;
            if (toNPC.sqrMagnitude < 0.01f)
                return new Vector3(0f, dialogueHeight, -dialogueSideDistance);

            Vector3 npcDir = toNPC.normalized;
            // NPC-Player line에 수직인 측면 방향
            Vector3 side = Vector3.Cross(Vector3.up, npcDir);

            // 측면 lock — 첫 대화 진입 시 한 번만 정함
            if (justStarting && dialogueBlend < 0.05f)
                dialogueSideSign = side;
            if (Vector3.Dot(dialogueSideSign, side) < 0f)
                dialogueSideSign = -dialogueSideSign;

            // 측면(perpendicular) + 플레이어 등 뒤(NPC 반대 방향) = 사선 시점에서 NPC 얼굴 보임
            return dialogueSideSign.normalized * dialogueSideDistance
                 - npcDir * dialogueForwardOffset
                 + Vector3.up * dialogueHeight;
        }

        private void HandleZoomInput()
        {
            // WebView 레이싱 화면 동안에는 줌 차단
            var bridge = RacingWebViewBridge.Instance;
            if (bridge != null && bridge.IsShowing) return;
            var rmc = RacingMissionController.Instance;
            if (rmc != null && rmc.IsOpen) return;
            // 대화 중 줌 차단
            var ds = DialogueSystem.Instance;
            if (ds != null && ds.IsActive) return;
            // 인벤토리 열림 중에도 줌 차단
            if (FacilityHUD.IsInventoryOpen) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) < 0.01f) return;

            // Positive scroll = zoom in (smaller multiplier)
            zoomTarget -= scroll * zoomSpeed * 0.01f;
            zoomTarget = Mathf.Clamp(zoomTarget, minZoom, maxZoom);
        }
    }
}
