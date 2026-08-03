// ═══════════════════════════════════════════════════════
// AccessoryData.cs — ScriptableObject for accessories (rings, charms)
// ═══════════════════════════════════════════════════════

using UnityEngine;

namespace InfinityRPG
{
    public enum AccessoryEffect
    {
        None,
        HPRecovery,     // % HP healed after each battle
        EXPBoost,        // % extra EXP
        BPBoost,         // flat BP bonus
        GoldBoost,       // % extra gold
        HPBoost,         // flat max HP bonus
    }

    [CreateAssetMenu(fileName = "Accessory_", menuName = "InfinityRPG/Accessory", order = 3)]
    public class AccessoryData : ScriptableObject
    {
        [Header("Identity")]
        public string itemId;
        public string displayName;

        [Header("Effect")]
        public AccessoryEffect effect;
        public float effectValue;    // 2.0 = 2%, 20.0 = +20% or +20

        [Header("Economy")]
        public int cost;

        [Header("Visual (optional)")]
        public Sprite icon;

        public string EffectDescription
        {
            get
            {
                return effect switch
                {
                    AccessoryEffect.HPRecovery => $"Rec{effectValue}%",
                    AccessoryEffect.EXPBoost  => $"EXP+{effectValue}%",
                    AccessoryEffect.BPBoost   => $"BP+{effectValue}",
                    AccessoryEffect.GoldBoost => $"Gold+{effectValue}%",
                    AccessoryEffect.HPBoost   => $"HP+{effectValue}",
                    _ => ""
                };
            }
        }

        public override string ToString() => $"{displayName} ({EffectDescription}, {cost}g)";
    }
}
