// ═══════════════════════════════════════════════════════
// GameConfig.cs — Master config ScriptableObject holding all game data
// ═══════════════════════════════════════════════════════

using UnityEngine;

namespace InfinityRPG
{
    /// <summary>
    /// Central configuration asset. Create ONE instance in Resources/ folder.
    /// All systems reference this for item/enemy/zone data.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "InfinityRPG/Game Config", order = 0)]
    public class GameConfig : ScriptableObject
    {
        [Header("Map")]
        public int mapWidth = 10;
        public int mapHeight = 12;
        public float tileSize = 1f;

        [Header("Zones (ordered top-to-bottom, highest difficulty first)")]
        public ZoneData[] zones;

        [Header("Equipment Database")]
        public WeaponData[] allWeapons;
        public ArmorData[] allArmors;
        public AccessoryData[] allAccessories;

        [Header("Progression")]
        public int statPointsPerLevel = 4;
        public float expCurveMultiplier = 1.35f;
        [Tooltip("Base EXP needed for level 2")]
        public int baseExpToNext = 80;

        [Header("Battle")]
        [Tooltip("If player BP < enemy BP * softCheck, instant death")]
        [Range(0f, 1f)]
        public float bpMinThreshold = 0.3f;
        [Tooltip("Max damage variance as fraction of ATK")]
        [Range(0f, 1f)]
        public float damageVariance = 0.3f;
        public int maxBattleTurns = 100;

        [Header("Starting Gear")]
        public string[] startingWeaponIds = { "w0" };
        public string[] startingArmorIds = { "a0" };

        // ---- Lookup Helpers ----
        public WeaponData GetWeapon(string id)
        {
            foreach (var w in allWeapons)
                if (w.itemId == id) return w;
            return null;
        }

        public ArmorData GetArmor(string id)
        {
            foreach (var a in allArmors)
                if (a.itemId == id) return a;
            return null;
        }

        public AccessoryData GetAccessory(string id)
        {
            foreach (var x in allAccessories)
                if (x.itemId == id) return x;
            return null;
        }

        /// <summary>Get the zone for a given map row.</summary>
        public ZoneData GetZoneForRow(int row)
        {
            if (zones == null || zones.Length == 0) return null;
            // Map zones bottom-to-top
            int zoneIndex = row switch
            {
                < 2  => 0, // Dragon Lair (top)
                < 4  => 1, // Volcanic Depths
                < 6  => 2, // Dark Caverns
                < 8  => 3, // Goblin Forest
                < 10 => 4, // Slime Plains
                _    => 5, // Starting Town
            };
            return zoneIndex < zones.Length ? zones[zoneIndex] : null;
        }

        /// <summary>Get the town zone (bottom rows).</summary>
        public ZoneData GetTownZone()
        {
            return zones.Length >= 6 ? zones[5] : null;
        }
    }
}
