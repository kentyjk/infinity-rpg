// ═══════════════════════════════════════════════════════
// WeaponData.cs — ScriptableObject for weapon definitions
// ═══════════════════════════════════════════════════════

using UnityEngine;

namespace InfinityRPG
{
    [CreateAssetMenu(fileName = "Weapon_", menuName = "InfinityRPG/Weapon", order = 1)]
    public class WeaponData : ScriptableObject
    {
        [Header("Identity")]
        public string itemId;           // e.g. "w1"
        public string displayName;      // e.g. "Iron Blade"
        public int tier;

        [Header("Stats")]
        public int attackBonus;

        [Header("Economy")]
        public int cost;

        [Header("Visual (optional)")]
        public Sprite icon;
        public Color tintColor = Color.white;

        public override string ToString() => $"{displayName} (ATK+{attackBonus}, {cost}g)";
    }
}
