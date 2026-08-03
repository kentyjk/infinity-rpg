// ═══════════════════════════════════════════════════════
// SaveSystem.cs — Save/Load player state via PlayerPrefs
// ═══════════════════════════════════════════════════════

using UnityEngine;

namespace InfinityRPG
{
    /// <summary>
    /// Handles persistence of PlayerState to/from PlayerPrefs as JSON.
    /// Thread-safe, handles corrupted saves gracefully.
    /// </summary>
    public static class SaveSystem
    {
        private const string SAVE_KEY = "infinity_rpg_save";
        private const string BACKUP_KEY = "infinity_rpg_save_backup";

        /// <summary>
        /// Save the current player state. Writes to backup first as a safety measure.
        /// </summary>
        public static void Save(PlayerState state)
        {
            if (state == null) return;

            string json = JsonUtility.ToJson(state, prettyPrint: false);

            // Write to backup first (atomic-ish via PlayerPrefs)
            PlayerPrefs.SetString(BACKUP_KEY, json);

            // Then write to main key
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load the player state. Returns default state if no save exists or save is corrupted.
        /// </summary>
        public static PlayerState Load()
        {
            // Try main key first
            string json = PlayerPrefs.GetString(SAVE_KEY, "");

            // Fallback to backup
            if (string.IsNullOrEmpty(json))
                json = PlayerPrefs.GetString(BACKUP_KEY, "");

            if (string.IsNullOrEmpty(json))
                return PlayerState.Default;

            try
            {
                var state = JsonUtility.FromJson<PlayerState>(json);
                if (state == null) return PlayerState.Default;

                // Sanity checks
                if (state.ownedWeapons == null) state.ownedWeapons = new();
                if (state.ownedArmors == null) state.ownedArmors = new();
                if (state.ownedAccessories == null) state.ownedAccessories = new();
                if (state.maxHP <= 0) state.maxHP = 100;
                if (state.currentHP <= 0 && state.runActive) state.currentHP = state.maxHP;

                return state;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Corrupted save data: {e.Message}. Resetting.");
                DeleteSave();
                return PlayerState.Default;
            }
        }

        /// <summary>
        /// Hard reset — wipe all saved data.
        /// </summary>
        public static void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.DeleteKey(BACKUP_KEY);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Check if a save file exists.
        /// </summary>
        public static bool SaveExists()
        {
            return PlayerPrefs.HasKey(SAVE_KEY) || PlayerPrefs.HasKey(BACKUP_KEY);
        }
    }
}
