using UnityEngine;
using ForTheCompany.Data;
using ForTheCompany.Grid;

namespace ForTheCompany.Player
{
    public class NPCActor : MonoBehaviour
    {
        public PlayerData data;

        [Header("Grid Position")]
        public int gridX;
        public int gridZ;

        [Header("Insider")]
        public bool isSpy;
        public int suspicion;

        [Header("Movement")]
        public float yOffset = 0.55f;

        public string DisplayName => data != null ? data.playerName : gameObject.name;

        public void AddSuspicion(int delta)
        {
            suspicion = Mathf.Max(0, suspicion + delta);
        }

        public void TeleportTo(int x, int z)
        {
            gridX = x;
            gridZ = z;
            SnapToGrid();
        }

        public void SnapToGrid()
        {
            float size = GridManager.Instance != null ? GridManager.Instance.tileSize : 1f;
            transform.position = new Vector3(gridX * size, yOffset, gridZ * size);
        }
    }
}
