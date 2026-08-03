// ═══════════════════════════════════════════════════════
// PlayerController.cs — Handles tap-to-move input on the map
// ═══════════════════════════════════════════════════════

using UnityEngine;

namespace InfinityRPG
{
    /// <summary>
    /// Listens for tap/click input on the map and moves the player
    /// to adjacent tiles. Triggers battle when moving onto enemy tiles.
    ///
    /// Attach to the map GameObject or a child with a Collider2D.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private MapManager mapManager;
        [SerializeField] private Camera mainCamera;

        private void Awake()
        {
            if (mainCamera == null) mainCamera = Camera.main;
        }

        private void Update()
        {
            // Only handle input during exploration
            if (gameManager.CurrentGameState != GameState.Exploring) return;

            // Detect tap/click
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                Vector3 inputPos = Input.touchCount > 0
                    ? (Vector3)Input.GetTouch(0).position
                    : Input.mousePosition;

                HandleTap(inputPos);
            }
        }

        private void HandleTap(Vector3 screenPosition)
        {
            // Raycast from screen to world
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPosition);
            Vector2 world2D = new Vector2(worldPos.x, worldPos.y);

            RaycastHit2D hit = Physics2D.Raycast(world2D, Vector2.zero);
            if (hit.collider == null) return;

            // Try to get tile coordinates from the hit object
            var tile = hit.collider.GetComponent<MapTile>();
            if (tile == null) return;

            int targetX = tile.GridX;
            int targetY = tile.GridY;

            // Only adjacent tiles
            if (!mapManager.IsAdjacent(gameManager.State.playerX, gameManager.State.playerY, targetX, targetY))
                return;

            // Move and potentially battle
            gameManager.EngageEnemy(targetX, targetY);
        }
    }

    /// <summary>
    /// Component attached to each map tile GameObject.
    /// Stores grid coordinates for hit detection.
    /// </summary>
    public class MapTile : MonoBehaviour
    {
        public int GridX;
        public int GridY;

        public void Initialize(int x, int y, MapManager map)
        {
            GridX = x;
            GridY = y;
            name = $"Tile_{x}_{y}";
            transform.position = new Vector3(x * map.GetComponent<GameManager>().Config.tileSize,
                                             -y * map.GetComponent<GameManager>().Config.tileSize, 0);

            // Add collider for tap detection
            var col = GetComponent<BoxCollider2D>();
            if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
            float size = map.GetComponent<GameManager>().Config.tileSize;
            col.size = new Vector2(size, size);
            col.isTrigger = true;
        }
    }
}
