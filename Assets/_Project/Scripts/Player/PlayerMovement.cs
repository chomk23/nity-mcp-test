using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using ForTheCompany.Grid;

namespace ForTheCompany.Player
{
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Grid Position")]
        public int gridX;
        public int gridZ;

        [Header("Movement")]
        public float moveDuration = 0.2f;
        public float yOffset = 0.55f;
        public int apCostPerMove = 1;

        private PlayerStats stats;
        private bool isMoving;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
        }

        private void Start()
        {
            SnapToGrid();
        }

        private void Update()
        {
            if (isMoving) return;
            if (ForTheCompany.Events.EventManager.Instance != null &&
                ForTheCompany.Events.EventManager.Instance.HasActive) return;
            if (ForTheCompany.Managers.TurnManager.Instance != null &&
                ForTheCompany.Managers.TurnManager.Instance.IsBusy) return;
            if (ForTheCompany.Systems.AccusationSystem.Instance != null &&
                ForTheCompany.Systems.AccusationSystem.Instance.IsMenuOpen) return;
            if (ForTheCompany.Managers.GameStateManager.Instance != null &&
                ForTheCompany.Managers.GameStateManager.Instance.IsGameOver) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
                TryMove(0, 1);
            else if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame)
                TryMove(0, -1);
            else if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)
                TryMove(-1, 0);
            else if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame)
                TryMove(1, 0);
        }

        private void TryMove(int dx, int dz)
        {
            int targetX = gridX + dx;
            int targetZ = gridZ + dz;

            var grid = GridManager.Instance;
            if (grid == null) return;

            GameTile targetTile = grid.GetTile(targetX, targetZ);
            if (targetTile == null || !targetTile.IsWalkable)
            {
                Debug.Log($"[Player] blocked at ({targetX},{targetZ}).");
                return;
            }

            if (!stats.SpendAP(apCostPerMove))
            {
                Debug.Log("[Player] not enough AP.");
                return;
            }

            gridX = targetX;
            gridZ = targetZ;
            StartCoroutine(MoveCoroutine(GridToWorld(gridX, gridZ)));
        }

        private void SnapToGrid()
        {
            transform.position = GridToWorld(gridX, gridZ);
        }

        private Vector3 GridToWorld(int x, int z)
        {
            float size = GridManager.Instance != null ? GridManager.Instance.tileSize : 1f;
            return new Vector3(x * size, yOffset, z * size);
        }

        private IEnumerator MoveCoroutine(Vector3 target)
        {
            isMoving = true;
            Vector3 start = transform.position;
            float t = 0f;
            while (t < moveDuration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(start, target, t / moveDuration);
                yield return null;
            }
            transform.position = target;
            isMoving = false;
        }
    }
}
