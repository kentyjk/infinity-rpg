// ═══════════════════════════════════════════════════════
// EnemyData.cs — ScriptableObject for enemy definitions
// ═══════════════════════════════════════════════════════

using UnityEngine;

namespace InfinityRPG
{
    [CreateAssetMenu(fileName = "Enemy_", menuName = "InfinityRPG/Enemy", order = 4)]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string displayName;
        public TileType tileType = TileType.Enemy;  // Enemy, Boss, or Bonus
        public string icon = "🟢";                    // Emoji or sprite ref

        [Header("Combat Stats")]
        public int hp;
        public int attack;
        public int defense;
        public int agility;
        public int bpRequirement;    // Recommended BP to fight

        [Header("Rewards")]
        public int expReward;
        public int goldReward;

        // --- Runtime helpers (used when generating map instances) ---
        public EnemyRuntimeData CreateRuntime() => new EnemyRuntimeData
        {
            data = this,
            currentHP = hp
        };
    }

    /// <summary>
    /// Runtime instance of an enemy on the map — tracks current HP during a run.
    /// </summary>
    [System.Serializable]
    public class EnemyRuntimeData
    {
        public EnemyData data;
        public int currentHP;

        public string DisplayName => data.displayName;
        public TileType TileType => data.tileType;
        public string Icon => data.icon;
        public int HP => data.hp;
        public int Attack => data.attack;
        public int Defense => data.defense;
        public int Agility => data.agility;
        public int BPReq => data.bpRequirement;
        public int ExpReward => data.expReward;
        public int GoldReward => data.goldReward;
    }
}
