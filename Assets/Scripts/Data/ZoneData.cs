// ═══════════════════════════════════════════════════════
// ZoneData.cs — ScriptableObject for map zone definitions
// ═══════════════════════════════════════════════════════

using UnityEngine;

namespace InfinityRPG
{
    [CreateAssetMenu(fileName = "Zone_", menuName = "InfinityRPG/Zone", order = 5)]
    public class ZoneData : ScriptableObject
    {
        [Header("Identity")]
        public ZoneType zoneType;
        public string displayName;

        [Header("Visual")]
        public Color backgroundColor = Color.gray;
        public Color enemyColor = Color.white;

        [Header("Difficulty Range")]
        public int bpMin;
        public int bpMax;

        [Header("Enemy Pools")]
        public EnemyData[] commonEnemies;   // Pool for normal tiles
        public EnemyData bossEnemy;         // Boss at zone edge
        public EnemyData bonusEnemy;        // Rare bonus tile (optional, can be null)

        [Header("Generation")]
        [Range(0f, 0.2f)]
        public float bonusSpawnChance = 0.08f;

        /// <summary>
        /// Generate a random enemy from this zone's pool, scaled to BP range.
        /// </summary>
        public EnemyRuntimeData GenerateRandomEnemy(System.Random rng = null)
        {
            if (rng == null) rng = new System.Random();

            if (commonEnemies == null || commonEnemies.Length == 0)
                return null;

            // Pick a template from the pool
            var template = commonEnemies[rng.Next(commonEnemies.Length)];

            // Scale to zone's BP range
            float t = (float)rng.NextDouble();
            int targetBP = Mathf.RoundToInt(Mathf.Lerp(bpMin, bpMax, t));

            // Scale stats proportional to BP relative to template
            float scaleFactor = (float)targetBP / Mathf.Max(1, template.bpRequirement);

            return new EnemyRuntimeData
            {
                data = template,
                currentHP = Mathf.RoundToInt(template.hp * scaleFactor)
            };
        }
    }
}
