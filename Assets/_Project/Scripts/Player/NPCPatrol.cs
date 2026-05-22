using System.Collections.Generic;
using UnityEngine;
using ForTheCompany.Systems;

namespace ForTheCompany.Player
{
    /// <summary>
    /// NPC가 시작 위치 주변 3점을 천천히 왔다 갔다.
    /// 대화 중(이 NPC가 대화 대상)이면 멈춤.
    /// 시설이 살아있는 느낌 + "왜 저기 갔지?" 같은 의심 유발용.
    /// </summary>
    public class NPCPatrol : MonoBehaviour
    {
        [Header("Patrol Path")]
        [Tooltip("시작 위치 주변 patrol 반경 (방 크기 안에 머무르도록 작게)")]
        public float patrolRadius = 2.2f;
        [Tooltip("waypoint 개수")]
        public int waypointCount = 3;

        [Header("Movement")]
        [Tooltip("이동 속도 (플레이어 5보다 느리게)")]
        public float moveSpeed = 1.4f;
        [Tooltip("회전 부드러움")]
        public float turnSpeed = 5f;
        [Tooltip("waypoint 도착 후 멈춰있을 시간")]
        public float idleDuration = 2.2f;
        [Tooltip("waypoint 도착 판정 거리")]
        public float arriveDistance = 0.25f;

        private Vector3 startPos;
        private List<Vector3> waypoints;
        private int currentIdx;
        private float idleTimer;
        private bool isIdling;

        private void Start()
        {
            startPos = transform.position;
            GenerateWaypoints();
            currentIdx = 0;
            isIdling = true;
            idleTimer = Random.Range(0f, idleDuration); // NPC들이 동시 출발 안 하도록 stagger
        }

        private void GenerateWaypoints()
        {
            waypoints = new List<Vector3>();
            for (int i = 0; i < waypointCount; i++)
            {
                // 시작점 기준 원형으로 균등 분포
                float angle = (i / (float)waypointCount) * Mathf.PI * 2f
                            + Random.Range(-0.4f, 0.4f); // 약간의 변주
                float r = Random.Range(patrolRadius * 0.5f, patrolRadius);
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);
                waypoints.Add(startPos + offset);
            }
        }

        private void Update()
        {
            // 대화 중이면(이 NPC와 대화) 정지
            if (IsTalkingWithMe()) return;
            // 게임 결말 났으면 멈춤
            var s = ForTheCompany.Core.GameSession.Instance;
            if (s != null && s.Outcome != ForTheCompany.Core.RunOutcome.Ongoing) return;

            if (isIdling)
            {
                idleTimer -= Time.deltaTime;
                if (idleTimer <= 0f)
                {
                    isIdling = false;
                    currentIdx = (currentIdx + 1) % waypoints.Count;
                }
                return;
            }

            Vector3 target = waypoints[currentIdx];
            Vector3 toTarget = target - transform.position;
            toTarget.y = 0f;
            float dist = toTarget.magnitude;

            if (dist < arriveDistance)
            {
                isIdling = true;
                idleTimer = idleDuration;
                return;
            }

            Vector3 dir = toTarget.normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;

            // 진행 방향으로 부드럽게 회전
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        private bool IsTalkingWithMe()
        {
            var ds = DialogueSystem.Instance;
            if (ds == null || !ds.IsActive) return false;
            return ds.CurrentNPCTransform == transform;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.85f, 0.3f, 0.35f);
            if (waypoints != null)
            {
                foreach (var wp in waypoints)
                    Gizmos.DrawWireSphere(wp, 0.3f);
            }
            Gizmos.color = new Color(1f, 0.85f, 0.3f, 0.15f);
            Vector3 center = Application.isPlaying ? startPos : transform.position;
            Gizmos.DrawWireSphere(center, patrolRadius);
        }
    }
}
