// ═══════════════════════════════════════════════════════
// ArmorData.cs — ScriptableObject for armor definitions
// ═══════════════════════════════════════════════════════

using UnityEngine;

namespace InfinityRPG
{
    [CreateAssetMenu(fileName = "Armor_", menuName = "InfinityRPG/Armor", order = 2)]
    public class ArmorData : ScriptableObject
    {
        [Header("Identity")]
        public string itemId;
        public string displayName;
        public int tier;

        [Header("Stats")]
        public int defenseBonus;

        [Header("Economy")]
        public int cost;

        [Header("Visual (optional)")]
        public Sprite icon;

        public override string ToString() => $"{displayName} (DEF+{defenseBonus}, {cost}g)";
    }
}
