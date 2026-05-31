using UnityEngine;
using UnityEngine.SceneManagement;
using ForTheCompany.Core;
using ForTheCompany.Data;
using ForTheCompany.Managers;

namespace ForTheCompany.Player
{
    /// <summary>
    /// 카드키로 잠긴 문. GameSession.hasFacilityCardkey가 true가 되면 자동 해제.
    /// 빨간 큐브 + Collider로 길 막음. 잠금 해제 시 collider 비활성 + 색 녹색 + 작게 축소.
    /// </summary>
    public class LockedDoor : MonoBehaviour
    {
        [Tooltip("이 문이 잠금일 때 표시되는 색")]
        public Color lockedColor = new Color(0.9f, 0.2f, 0.25f);
        [Tooltip("잠금 해제 시 표시되는 색")]
        public Color unlockedColor = new Color(0.35f, 0.85f, 0.45f);

        private BoxCollider boxCol;
        private MeshRenderer mr;
        private Vector3 originalScale;
        private bool wasLocked = true;

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
            // 전력실 잠금문 제거 — 더 이상 자동 생성하지 않는다.
            // (카드키 게이트는 서사/단서로만 유지되고, 물리 차단막은 없앰)
            // 씬에 수동 배치된 잔존 인스턴스가 있으면 Awake에서 스스로 제거한다.
            if (SceneManager.GetActiveScene().name != "FacilityScene") return;
            var existing = FindFirstObjectByType<LockedDoor>();
            if (existing != null) Destroy(existing.gameObject);
        }

        private void Start()
        {
            // 시설관리자 위치를 기준으로 잠금문 자동 배치 (벽 겹침 방지)
            var manager = FindFacilityManager();
            if (manager != null)
            {
                // 시설관리자에서 중앙복도 방향 — 가장 큰 axis(동서 또는 남북)만 사용해서 axis-aligned 통로 막기
                Vector3 toCenter = -manager.transform.position;
                toCenter.y = 0f;
                if (toCenter.sqrMagnitude < 0.01f) toCenter = Vector3.left;

                Vector3 axisDir;
                if (Mathf.Abs(toCenter.x) > Mathf.Abs(toCenter.z))
                    axisDir = new Vector3(Mathf.Sign(toCenter.x), 0f, 0f); // 동서 통로
                else
                    axisDir = new Vector3(0f, 0f, Mathf.Sign(toCenter.z)); // 남북 통로

                Vector3 doorPos = manager.transform.position + axisDir * 4f;
                doorPos.y = 1.2f;
                transform.position = doorPos;
                transform.rotation = Quaternion.LookRotation(axisDir); // 통로에 수직으로 막기

                Debug.Log($"[LockedDoor] 시설관리자 기준 axis-aligned 배치: {doorPos:F1} (axis {axisDir})");
            }
            else
            {
                Debug.LogWarning("[LockedDoor] 시설관리자를 찾을 수 없어 fallback 좌표 유지");
            }
            originalScale = transform.localScale;
        }

        private static NPCActor FindFacilityManager()
        {
            var roster = NPCRoster.Instance;
            if (roster == null) return null;
            foreach (var n in roster.npcs)
            {
                if (n == null || n.data == null) continue;
                if (n.data.role == RoleType.FacilityManager) return n;
            }
            return null;
        }

        private void Awake()
        {
            // 전력실 문 제거 정책 — 어떤 경로로든 생성된 LockedDoor는 즉시 파괴.
            Destroy(gameObject);
        }

        private void Update()
        {
            bool locked = IsLocked();
            if (locked != wasLocked)
            {
                wasLocked = locked;
                ApplyVisual(locked);
            }
        }

        private bool IsLocked()
        {
            var s = GameSession.Instance;
            if (s == null) return true;
            return !s.hasFacilityCardkey;
        }

        private void ApplyVisual(bool locked)
        {
            if (boxCol != null) boxCol.enabled = locked;

            if (mr != null)
            {
                var mpb = new MaterialPropertyBlock();
                mpb.SetColor("_BaseColor", locked ? lockedColor : unlockedColor);
                mr.SetPropertyBlock(mpb);
            }

            // 잠금 해제 시 살짝 축소 (문 열린 느낌)
            transform.localScale = locked
                ? originalScale
                : new Vector3(originalScale.x * 0.35f, originalScale.y, originalScale.z);
        }
    }
}
