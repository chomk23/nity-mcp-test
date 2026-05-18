using UnityEngine;

namespace ForTheCompany.Grid
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("Grid Size")]
        public int width = 10;
        public int height = 10;

        [Header("Tile")]
        public GameObject tilePrefab;
        public float tileSize = 1f;

        private GameTile[,] tiles;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            GenerateGrid();
        }

        public void GenerateGrid()
        {
            tiles = new GameTile[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    Vector3 position = new Vector3(x * tileSize, 0f, z * tileSize);
                    GameObject tileGO = Instantiate(tilePrefab, position, Quaternion.identity, transform);

                    GameTile tile = tileGO.GetComponent<GameTile>();
                    if (tile == null)
                    {
                        tile = tileGO.AddComponent<GameTile>();
                    }
                    tile.Init(x, z);
                    tiles[x, z] = tile;
                }
            }

            Debug.Log($"[GridManager] {width} x {height} grid generated ({width * height} tiles).");
        }

        public GameTile GetTile(int x, int z)
        {
            if (x < 0 || x >= width || z < 0 || z >= height) return null;
            return tiles[x, z];
        }

        public bool IsInBounds(int x, int z)
        {
            return x >= 0 && x < width && z >= 0 && z < height;
        }
    }
}
