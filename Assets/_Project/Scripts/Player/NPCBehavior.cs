using System.Collections;
using UnityEngine;
using ForTheCompany.Grid;

namespace ForTheCompany.Player
{
    [RequireComponent(typeof(NPCActor))]
    public class NPCBehavior : MonoBehaviour
    {
        public float moveDuration = 0.2f;

        private NPCActor actor;
        private bool isMoving;

        private void Awake()
        {
            actor = GetComponent<NPCActor>();
        }

        public IEnumerator TakeTurn()
        {
            if (isMoving) yield break;

            var grid = GridManager.Instance;
            if (grid == null) yield break;

            int[] dx = { 1, -1, 0, 0 };
            int[] dz = { 0, 0, 1, -1 };

            for (int attempt = 0; attempt < 4; attempt++)
            {
                int d = Random.Range(0, 4);
                int nx = actor.gridX + dx[d];
                int nz = actor.gridZ + dz[d];
                var tile = grid.GetTile(nx, nz);
                if (tile == null || !tile.IsWalkable) continue;

                yield return StartCoroutine(MoveTo(nx, nz));
                yield break;
            }
        }

        private IEnumerator MoveTo(int nx, int nz)
        {
            isMoving = true;
            actor.gridX = nx;
            actor.gridZ = nz;

            float size = GridManager.Instance.tileSize;
            Vector3 start = transform.position;
            Vector3 target = new Vector3(nx * size, actor.yOffset, nz * size);

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
