using UnityEngine;
using ForTheCompany.Systems;

namespace ForTheCompany.Player
{
    /// <summary>
    /// 정적 캐릭터 모델(Kenney Blocky 등)에 procedural 걷기 애니메이션 부여.
    /// - 이동 감지 → Y bobbing + 좌우 기울임 + 팔다리 swing
    /// - 정지 → 가벼운 idle 호흡
    /// Kenney 캐릭터 자식 구조 (root → leg-left/right, torso → arm-left/right/head)에 맞춰 자동 부품 탐색.
    /// </summary>
    public class CharacterBobbing : MonoBehaviour
    {
        [Tooltip("애니메이션 적용할 자식 GameObject (캐릭터 모델). 비워두면 transform 자신")]
        public Transform model;

        [Header("Walk Bobbing")]
        public float bobSpeed = 9f;
        public float bobAmplitude = 0.08f;
        public float tiltAmplitude = 4f;

        [Header("Limb Swing (걷기 시 팔다리 흔들기)")]
        [Tooltip("팔다리 swing 각도 (도) — 30~50 권장")]
        public float limbSwingAngle = 40f;
        [Tooltip("팔 swing 강도 (다리보다 약간 작게)")]
        public float armSwingMultiplier = 0.9f;

        [Header("Talking Body Language (대화 시 제스처)")]
        [Tooltip("제스처 전환 부드러움 (높을수록 빠르게 전환)")]
        public float gestureLerpSpeed = 3.5f;
        [Tooltip("한 제스처 유지 시간 최소(초)")]
        public float gestureMinDuration = 2.5f;
        [Tooltip("한 제스처 유지 시간 최대(초)")]
        public float gestureMaxDuration = 4.5f;
        [Tooltip("제스처 위에 추가되는 미세 흔들림 폭(도)")]
        public float gestureMicroJitter = 3f;

        [Header("Idle Breathing")]
        public float idleBreathSpeed = 1.5f;
        public float idleBreathAmplitude = 0.015f;

        [Header("Movement Detection")]
        public float moveThreshold = 0.005f;

        private Vector3 modelStartLocalPos;
        private Quaternion modelStartLocalRot;
        private Vector3 lastWorldPos;
        private float bobPhase;
        private float walkBlend; // 0=idle, 1=walking

        // 자동 탐색된 팔다리
        private Transform legLeft, legRight, armLeft, armRight;
        private Quaternion legLeftRest, legRightRest, armLeftRest, armRightRest;

        // 대화 제스처 시스템
        private struct Gesture
        {
            public string name;
            public Vector3 armLeftEuler;
            public Vector3 armRightEuler;
            public Gesture(string n, Vector3 l, Vector3 r) { name = n; armLeftEuler = l; armRightEuler = r; }
        }
        private static readonly Gesture[] TalkGestures =
        {
            new Gesture("Neutral",        new Vector3(0f, 0f, 0f),       new Vector3(0f, 0f, 0f)),
            new Gesture("HandsOnHips",    new Vector3(20f, 0f, -45f),    new Vector3(20f, 0f, 45f)),
            new Gesture("RightChinTouch", new Vector3(0f, 0f, 0f),       new Vector3(-90f, 30f, -25f)),
            new Gesture("LeftHairTouch",  new Vector3(-100f, -30f, 25f), new Vector3(0f, 0f, 0f)),
            new Gesture("PointForward",   new Vector3(0f, 0f, 0f),       new Vector3(-75f, 20f, 0f)),
            new Gesture("RubEyes",        new Vector3(0f, 0f, 0f),       new Vector3(-85f, -10f, -15f)),
            new Gesture("LeftHandOut",    new Vector3(-75f, -20f, 0f),   new Vector3(0f, 0f, 0f)),
            new Gesture("ArmsCrossed",    new Vector3(-40f, 30f, -15f),  new Vector3(-40f, -30f, 15f)),
        };
        private int currentGestureIdx = -1;
        private string lastTalkLine = ""; // 라인 변경 감지
        private Vector3 currentArmLeftTarget, currentArmRightTarget;

        private void Start()
        {
            if (model == null) model = transform;
            modelStartLocalPos = model.localPosition;
            modelStartLocalRot = model.localRotation;
            lastWorldPos = transform.position;

            FindLimbs(model);
            CacheLimbRestRotations();
        }

        private void FindLimbs(Transform root)
        {
            // 재귀로 모든 자식 탐색
            foreach (Transform t in root)
            {
                string n = t.name.ToLower();
                if (legLeft == null && (n.Contains("leg-left") || n.Contains("leg_left") || n == "legl"))
                    legLeft = t;
                else if (legRight == null && (n.Contains("leg-right") || n.Contains("leg_right") || n == "legr"))
                    legRight = t;
                else if (armLeft == null && (n.Contains("arm-left") || n.Contains("arm_left") || n == "arml"))
                    armLeft = t;
                else if (armRight == null && (n.Contains("arm-right") || n.Contains("arm_right") || n == "armr"))
                    armRight = t;

                FindLimbs(t);
            }
        }

        private void CacheLimbRestRotations()
        {
            if (legLeft != null) legLeftRest = legLeft.localRotation;
            if (legRight != null) legRightRest = legRight.localRotation;
            if (armLeft != null) armLeftRest = armLeft.localRotation;
            if (armRight != null) armRightRest = armRight.localRotation;
        }

        private void LateUpdate()
        {
            // 이동 감지 (XZ 평면)
            Vector3 delta = transform.position - lastWorldPos;
            delta.y = 0f;
            lastWorldPos = transform.position;
            bool moving = delta.sqrMagnitude > moveThreshold * moveThreshold;

            // walkBlend 부드러운 전환
            walkBlend = Mathf.MoveTowards(walkBlend, moving ? 1f : 0f, Time.deltaTime * 6f);

            if (walkBlend > 0.01f)
                bobPhase += Time.deltaTime * bobSpeed;

            // ── 몸통 Y bobbing + 좌우 기울임 ──
            float walkY = Mathf.Abs(Mathf.Sin(bobPhase)) * bobAmplitude;
            float idleY = Mathf.Sin(Time.time * idleBreathSpeed) * idleBreathAmplitude;
            float yOffset = Mathf.Lerp(idleY, walkY, walkBlend);
            float tilt = Mathf.Sin(bobPhase) * tiltAmplitude * walkBlend;

            model.localPosition = modelStartLocalPos + new Vector3(0f, yOffset, 0f);
            model.localRotation = modelStartLocalRot * Quaternion.Euler(0f, 0f, tilt);

            // ── 다리 swing (걷기) ──
            float legSwing = Mathf.Sin(bobPhase) * limbSwingAngle * walkBlend;
            if (legLeft != null)
                legLeft.localRotation = legLeftRest * Quaternion.Euler(legSwing, 0f, 0f);
            if (legRight != null)
                legRight.localRotation = legRightRest * Quaternion.Euler(-legSwing, 0f, 0f);

            // ── 팔 동작 결정 ──
            bool talking = IsTalkingNow();

            if (talking)
            {
                ApplyTalkingGestures();
            }
            else
            {
                // 걷기 중이면 다리와 반대 방향 swing, 아니면 거의 정지
                float walkArmSwing = legSwing * armSwingMultiplier;
                if (armLeft != null)
                    armLeft.localRotation = armLeftRest * Quaternion.Euler(-walkArmSwing, 0f, 0f);
                if (armRight != null)
                    armRight.localRotation = armRightRest * Quaternion.Euler(walkArmSwing, 0f, 0f);
                // 다음 대화 시작 시 첫 제스처 즉시 고르도록
                lastTalkLine = "";
                currentGestureIdx = -1;
            }
        }

        /// <summary>대화 라인이 바뀔 때마다 새 제스처 + 부드러운 Slerp + 미세 흔들림</summary>
        private void ApplyTalkingGestures()
        {
            var ds = DialogueSystem.Instance;
            string currentLine = ds != null ? ds.CurrentFullLine : "";

            // 첫 진입 또는 라인이 바뀌면 새 제스처 선택
            if (currentGestureIdx < 0 || currentLine != lastTalkLine)
            {
                int prevIdx = currentGestureIdx;
                int newIdx;
                int safety = 0;
                do
                {
                    newIdx = Random.Range(0, TalkGestures.Length);
                    safety++;
                } while (newIdx == prevIdx && TalkGestures.Length > 1 && safety < 5);

                currentGestureIdx = newIdx;
                currentArmLeftTarget = TalkGestures[newIdx].armLeftEuler;
                currentArmRightTarget = TalkGestures[newIdx].armRightEuler;
                lastTalkLine = currentLine;
            }

            // 미세 흔들림 — 한 자세에서도 살짝 움직여 살아있는 느낌
            float micro = Mathf.Sin(Time.time * 2.3f) * gestureMicroJitter;
            float microR = Mathf.Sin(Time.time * 1.8f + 1.1f) * gestureMicroJitter;

            // 목표 회전 (rest + gesture + micro jitter)
            Quaternion targetLeft = armLeftRest
                * Quaternion.Euler(currentArmLeftTarget.x + micro, currentArmLeftTarget.y, currentArmLeftTarget.z);
            Quaternion targetRight = armRightRest
                * Quaternion.Euler(currentArmRightTarget.x + microR, currentArmRightTarget.y, currentArmRightTarget.z);

            // Slerp로 부드럽게 전환
            float t = Time.deltaTime * gestureLerpSpeed;
            if (armLeft != null)
                armLeft.localRotation = Quaternion.Slerp(armLeft.localRotation, targetLeft, t);
            if (armRight != null)
                armRight.localRotation = Quaternion.Slerp(armRight.localRotation, targetRight, t);
        }

        /// <summary>이 캐릭터가 현재 대화 대상 NPC인지</summary>
        private bool IsTalkingNow()
        {
            var ds = DialogueSystem.Instance;
            if (ds == null || !ds.IsActive) return false;
            return ds.CurrentNPCTransform == transform;
        }
    }
}
