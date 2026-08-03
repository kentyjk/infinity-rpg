// ═══════════════════════════════════════════════════════
// PlayerState.cs — Serializable player state for save/load
// ═══════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;

namespace InfinityRPG
{
    /// <summary>
    /// Serializable container for ALL persistent player data.
    /// Serialized to JSON for save/load via PlayerPrefs.
    /// </summary>
    [System.Serializable]
    public class PlayerState
    {
        // ---- Economy ----
        public int bankGold;

        // ---- Equipment Ownership ----
        public List<string> ownedWeapons = new();
        public List<string> ownedArmors = new();
        public List<string> ownedAccessories = new();

        // ---- Equipped Loadout ----
        public string equippedWeaponId;     // null = nothing equipped
        public string equippedArmorId;
        public string equippedAccessoryId;

        // ---- Meta ----
        public int totalRuns;
        public int highestZoneReached;

        // ---- Run State (only valid during active run) ----
        public bool runActive;
        public int level = 1;
        public int exp;
        public int expToNext = 80;
        public int statPoints;
        public int currentHP;
        public int maxHP = 100;

        // Base stats (without equipment)
        public int atkBase = 10;
        public int defBase = 5;
        public int agiBase = 10;

        // Gold earned this run
        public int runGold;

        // Player position on map
        public int playerX = 4;
        public int playerY = 11;

        // ---- Helper Methods ----

        /// <summary>
        /// Returns the default (new game) state.
        /// </summary>
        public static PlayerState Default => new()
        {
            bankGold = 0,
            ownedWeapons = new List<string> { "w0" },   // Rusty Sword
            ownedArmors = new List<string> { "a0" },     // Leather Vest
            ownedAccessories = new List<string>(),
            totalRuns = 0,
            highestZoneReached = 0
        };

        /// <summary>
        /// Reset the run-specific state (called when starting a new run).
        /// </summary>
        public void ResetRunState()
        {
            runActive = true;
            level = 1;
            exp = 0;
            expToNext = 80;
            statPoints = 0;
            atkBase = 10;
            defBase = 5;
            agiBase = 10;
            maxHP = 100;
            currentHP = maxHP;
            runGold = 0;
            playerX = 4;
            playerY = 11;
        }

        /// <summary>
        /// Compute the player's effective Battle Power (BP).
        /// BP = ATK*2 + DEF*1.5 + AGI*1.5 + equipment bonuses
        /// </summary>
        public int ComputeBP(AccessoryData accessory)
        {
            float bp = atkBase * 2f + defBase * 1.5f + agiBase * 1.5f;
            if (accessory != null && accessory.effect == AccessoryEffect.BPBoost)
                bp += accessory.effectValue * 10;
            return Mathf.RoundToInt(bp);
        }
    }
}
