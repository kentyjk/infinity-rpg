// ═══════════════════════════════════════════════════════
// MapManager.cs — Grid map generation, tile data, and player interaction
// ═══════════════════════════════════════════════════════

using UnityEngine;

namespace InfinityRPG
{
    /// <summary>
    /// Manages the 2D grid map: generates enemies on tiles, tracks cleared tiles,
    /// and handles player movement validation.
    ///
    /// Attach to GameManager GameObject or a child.
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private MapRenderer mapRenderer; // Optional: visual renderer

        /// <summary>
        /// 2D array of enemy data. null = empty/cleared tile.
        /// </summary>
        private EnemyRuntimeData[,] tileData;
        private bool[,] tileCleared;

        public int MapWidth => gameManager.Config.mapWidth;
        public int MapHeight => gameManager.Config.mapHeight;

        private System.Random rng;

        // ═══════════════════════════════════════════════
        //  MAP GENERATION
        // ═══════════════════════════════════════════════

        public void GenerateMap(int seed = 0)
        {
            var config = gameManager.Config;
            if (seed == 0) seed = System.Environment.TickCount;
            rng = new System.Random(seed);

            tileData = new EnemyRuntimeData[MapHeight, MapWidth];
            tileCleared = new bool[MapHeight, MapWidth];

            for (int y = 0; y < MapHeight; y++)
            {
                var zone = config.GetZoneForRow(y);

                // Starting town rows — no enemies
                if (zone == null || zone.zoneType == ZoneType.StartingTown)
                    continue;

                for (int x = 0; x < MapWidth; x++)
                {
                    // Boss at right edge on even rows
                    bool isBoss = (x == MapWidth - 1 && y % 2 == 0);
                    // Bonus tile — rare random spawn
                    bool isBonus = (!isBoss && rng.NextDouble() < zone.bonusSpawnChance);

                    if (isBoss && zone.bossEnemy != null)
                    {
                        // Scale boss stats to 2x zone max
                        float scale = (float)(zone.bpMax * 2) / Mathf.Max(1, zone.bossEnemy.bpRequirement);
                        tileData[y, x] = new EnemyRuntimeData
                        {
                            data = zone.bossEnemy,
                            currentHP = Mathf.RoundToInt(zone.bossEnemy.hp * scale)
                        };
                    }
                    else if (isBonus && zone.bonusEnemy != null)
                    {
                        tileData[y, x] = zone.bonusEnemy.CreateRuntime();
                    }
                    else
                    {
                        // Random enemy from zone pool
                        var enemy = zone.GenerateRandomEnemy(rng);
                        tileData[y, x] = enemy;
                    }
                }
            }

            // Clear starting position
            tileCleared[4, 11] = true;

            mapRenderer?.RenderMap(this);

            Debug.Log($"[MapManager] Map generated ({MapWidth}x{MapHeight}), seed={seed}");
        }

        // ═══════════════════════════════════════════════
        //  TILE QUERIES
        // ═══════════════════════════════════════════════

        /// <summary>Get the enemy at a tile position, or null if empty/cleared.</summary>
        public EnemyRuntimeData GetEnemyAt(int x, int y)
        {
            if (x < 0 || x >= MapWidth || y < 0 || y >= MapHeight) return null;
            if (tileCleared[y, x]) return null;
            return tileData[y, x];
        }

        /// <summary>Check if a tile has been cleared (empty or enemy defeated).</summary>
        public bool IsTileCleared(int x, int y)
        {
            if (x < 0 || x >= MapWidth || y < 0 || y >= MapHeight) return false;
            return tileCleared[y, x];
        }

        /// <summary>Check if a tile is within map bounds.</summary>
        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < MapWidth && y >= 0 && y < MapHeight;
        }

        /// <summary>Mark a tile as cleared (enemy defeated).</summary>
        public void ClearTile(int x, int y)
        {
            if (x < 0 || x >= MapWidth || y < 0 || y >= MapHeight) return;
            tileCleared[y, x] = true;
            tileData[y, x] = null;
            mapRenderer?.ClearTileVisual(x, y);
        }

        /// <summary>
        /// Check if a tile is adjacent to the given position (4-directional).
        /// </summary>
        public bool IsAdjacent(int fromX, int fromY, int toX, int toY)
        {
            int dx = Mathf.Abs(toX - fromX);
            int dy = Mathf.Abs(toY - fromY);
            return dx + dy == 1;
        }

        // ═══════════════════════════════════════════════
        //  Zone info
        // ═══════════════════════════════════════════════

        public ZoneData GetZoneAt(int x, int y)
        {
            return gameManager.Config.GetZoneForRow(y);
        }

        public Color GetTileColor(int x, int y)
        {
            var zone = GetZoneAt(x, y);
            if (zone == null) return Color.gray;
            return tileCleared[y, x] ? zone.backgroundColor * 0.5f : zone.backgroundColor;
        }
    }

    /// <summary>
    /// Optional component that handles visual rendering of the map grid.
    /// Attach to a GameObject with a Grid or manual tile system.
    /// </summary>
    public abstract class MapRenderer : MonoBehaviour
    {
        public abstract void RenderMap(MapManager map);
        public abstract void ClearTileVisual(int x, int y);
        public abstract void MovePlayerVisual(int fromX, int fromY, int toX, int toY);
    }
}
