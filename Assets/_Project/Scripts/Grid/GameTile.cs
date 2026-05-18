using UnityEngine;

namespace ForTheCompany.Grid
{
    public enum TileType
    {
        Floor,
        Wall,
        Door,
        Hazard,
        Extraction
    }

    public class GameTile : MonoBehaviour
    {
        public int x;
        public int z;
        public TileType type = TileType.Floor;

        public bool IsWalkable => type != TileType.Wall;

        public void Init(int x, int z, TileType type = TileType.Floor)
        {
            this.x = x;
            this.z = z;
            this.type = type;
            gameObject.name = $"Tile_{x}_{z}";
        }
    }
}
