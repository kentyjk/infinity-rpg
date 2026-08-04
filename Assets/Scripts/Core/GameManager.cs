// ═══════════════════════════════════════════════════════
// GameManager.cs — Central game orchestrator (Singleton)
// ═══════════════════════════════════════════════════════

using System;
using UnityEngine;

namespace InfinityRPG
{
    /// <summary>
    /// Central game orchestrator. Manages game state, coordinates subsystems,
    /// and provides the primary API for UI and other systems.
    ///
    /// Attach to a persistent GameObject in the first scene.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ---- Singleton ----
        public static GameManager Instance { get; private set; }

        // ---- Configuration (assign in Inspector) ----
        [SerializeField] private GameConfig config;

        // ---- Sub-system references (auto-wired via GetComponent or Inspector) ----
        [SerializeField] private BattleSystem battleSystem;
        [SerializeField] private MapManager mapManager;
        [SerializeField] private EquipmentManager equipmentManager;
        [SerializeField] private UIManager uiManager;

        // ---- State ----
        public PlayerState State { get; private set; }
        public GameState CurrentGameState { get; private set; } = GameState.Hub;

        // ---- Events (UI subscribes to these) ----
        public event Action<PlayerState> OnStateChanged;
        public event Action<GameState> OnGameStateChanged;
        public event Action<string> OnBattleLog;
        public event Action<string> OnToast;

        // ---- Computed Properties ----
        public WeaponData EquippedWeapon =>
            string.IsNullOrEmpty(State.equippedWeaponId) ? null : config.GetWeapon(State.equippedWeaponId);
        public ArmorData EquippedArmor =>
            string.IsNullOrEmpty(State.equippedArmorId) ? null : config.GetArmor(State.equippedArmorId);
        public AccessoryData EquippedAccessory =>
            string.IsNullOrEmpty(State.equippedAccessoryId) ? null : config.GetAccessory(State.equippedAccessoryId);

        public int EffectiveATK => State.atkBase + (EquippedWeapon?.attackBonus ?? 0);
        public int EffectiveDEF => State.defBase + (EquippedArmor?.defenseBonus ?? 0);
        public int EffectiveAGI => State.agiBase;
        public int EffectiveBP => State.ComputeBP(EquippedAccessory);
        public int EffectiveMaxHP => State.maxHP + (EquippedAccessory?.effect == AccessoryEffect.HPBoost
            ? Mathf.RoundToInt(EquippedAccessory.effectValue) : 0);
        public float HPRecoveryPercent =>
            EquippedAccessory?.effect == AccessoryEffect.HPRecovery ? EquippedAccessory.effectValue : 0f;
        public float EXPBoostPercent =>
            EquippedAccessory?.effect == AccessoryEffect.EXPBoost ? EquippedAccessory.effectValue : 0f;
        public float GoldBoostPercent =>
            EquippedAccessory?.effect == AccessoryEffect.GoldBoost ? EquippedAccessory.effectValue : 0f;

        // ═══════════════════════════════════════════════
        //  LIFECYCLE
        // ═══════════════════════════════════════════════

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Validate config
            if (config == null)
            {
                Debug.LogError("[GameManager] GameConfig not assigned! Create one in Resources/ and assign it.");
                return;
            }

            // Wire subsystems
            if (battleSystem == null) battleSystem = GetComponentInChildren<BattleSystem>();
            if (mapManager == null) mapManager = GetComponentInChildren<MapManager>();
            if (equipmentManager == null) equipmentManager = GetComponentInChildren<EquipmentManager>();
            if (uiManager == null) uiManager = GetComponentInChildren<UIManager>();
        }

        private void Start()
        {
            // Load saved state
            State = SaveSystem.Load();

            // Apply equipped stats to run state if mid-run
            if (State.runActive)
            {
                SetGameState(GameState.Exploring);
            }
            else
            {
                SetGameState(GameState.Hub);
            }

            NotifyStateChanged();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveSystem.Save(State);
        }

        private void OnApplicationQuit()
        {
            SaveSystem.Save(State);
        }

        // ═══════════════════════════════════════════════
        //  GAME FLOW
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Start a new run. Resets run state, generates map, places player.
        /// </summary>
        public void StartRun()
        {
            if (State.runActive)
            {
                Debug.LogWarning("[GameManager] Run already active.");
                return;
            }

            State.ResetRunState();
            State.totalRuns++;

            // Apply HP bonus from equipment
            State.maxHP = EffectiveMaxHP;
            State.currentHP = State.maxHP;

            mapManager?.GenerateMap();
            SetGameState(GameState.Exploring);
            LogBattle("⚔️ Run started! Tap an adjacent enemy to battle.");
            SaveSystem.Save(State);
            NotifyStateChanged();
        }

        /// <summary>
        /// End the current run (player died or forfeited).
        /// </summary>
        public void EndRun(bool died)
        {
            if (!State.runActive) return;

            State.bankGold += State.runGold;
            State.runGold = 0;
            State.runActive = false;

            SetGameState(GameState.GameOver);

            if (died)
                LogBattle($"💀 You fell! {State.runGold:N0}g earned. Bank: {State.bankGold:N0}g");
            else
                LogBattle($"🏁 Run ended. Bank: {State.bankGold:N0}g");

            SaveSystem.Save(State);
            NotifyStateChanged();
        }

        /// <summary>
        /// Called by PlayerController when the player moves onto an enemy tile.
        /// </summary>
        public void EngageEnemy(int mapX, int mapY)
        {
            if (!State.runActive) return;
            if (battleSystem == null) return;

            State.playerX = mapX;
            State.playerY = mapY;

            var enemy = mapManager?.GetEnemyAt(mapX, mapY);
            if (enemy == null)
            {
                // Empty tile — just move
                LogBattle($"📍 Moved to ({mapX},{mapY}).");
                NotifyStateChanged();
                return;
            }

            SetGameState(GameState.Battling);
            battleSystem.ResolveBattle(this, enemy, OnBattleResult);
        }

        /// <summary>
        /// Callback from BattleSystem when battle completes.
        /// </summary>
        private void OnBattleResult(BattleResult result)
        {
            if (!State.runActive) return;

            if (!result.victory)
            {
                State.currentHP = 0;
                EndRun(true);
                return;
            }

            // Apply rewards
            State.currentHP = result.playerHPAfter;
            State.exp += result.expGained;
            State.runGold += result.goldGained;

            // Clear defeated enemy from map
            mapManager?.ClearTile(State.playerX, State.playerY);

            // Level-up check
            while (State.exp >= State.expToNext)
            {
                State.exp -= State.expToNext;
                State.level++;
                State.expToNext = Mathf.RoundToInt(State.expToNext * config.expCurveMultiplier);
                State.statPoints += config.statPointsPerLevel;
            }

            // Log
            string log = $"⚔️ Defeated {result.enemyName}! " +
                         $"<color=#44ccff>+{result.expGained:N0} EXP</color> · " +
                         $"<color=#ffcc44>+{result.goldGained:N0}g</color>";
            if (result.hpHealed > 0)
                log += $" · <color=#44ff66>+{result.hpHealed} HP</color>";
            LogBattle(log);

            // Check for level-up modal
            if (State.statPoints > 0)
            {
                SetGameState(GameState.LevelUp);
            }
            else
            {
                SetGameState(GameState.Exploring);
            }

            SaveSystem.Save(State);
            NotifyStateChanged();
        }

        /// <summary>
        /// Apply stat allocation and resume exploration.
        /// </summary>
        public void ApplyStatAllocation(int hpPoints, int atkPoints, int defPoints, int agiPoints)
        {
            State.atkBase += atkPoints * 2;
            State.defBase += defPoints;
            State.agiBase += agiPoints;
            State.maxHP += hpPoints * 5;
            State.currentHP = Mathf.Min(EffectiveMaxHP, State.currentHP + hpPoints * 5);
            State.statPoints = 0;

            SetGameState(GameState.Exploring);
            NotifyStateChanged();
            SaveSystem.Save(State);
        }

        // ═══════════════════════════════════════════════
        //  EQUIPMENT (delegates to EquipmentManager)
        // ═══════════════════════════════════════════════

        public bool BuyWeapon(WeaponData weapon)
        {
            if (State.bankGold < weapon.cost) return false;
            State.bankGold -= weapon.cost;
            State.ownedWeapons.Add(weapon.itemId);
            SaveSystem.Save(State);
            NotifyStateChanged();
            return true;
        }

        public bool BuyArmor(ArmorData armor)
        {
            if (State.bankGold < armor.cost) return false;
            State.bankGold -= armor.cost;
            State.ownedArmors.Add(armor.itemId);
            SaveSystem.Save(State);
            NotifyStateChanged();
            return true;
        }

        public bool BuyAccessory(AccessoryData accessory)
        {
            if (State.bankGold < accessory.cost) return false;
            State.bankGold -= accessory.cost;
            State.ownedAccessories.Add(accessory.itemId);
            SaveSystem.Save(State);
            NotifyStateChanged();
            return true;
        }

        public void EquipWeapon(string weaponId)
        {
            State.equippedWeaponId = string.IsNullOrEmpty(weaponId) ? null : weaponId;
            SaveSystem.Save(State);
            NotifyStateChanged();
        }

        public void EquipArmor(string armorId)
        {
            State.equippedArmorId = string.IsNullOrEmpty(armorId) ? null : armorId;
            SaveSystem.Save(State);
            NotifyStateChanged();
        }

        public void EquipAccessory(string accessoryId)
        {
            State.equippedAccessoryId = string.IsNullOrEmpty(accessoryId) ? null : accessoryId;
            SaveSystem.Save(State);
            NotifyStateChanged();
        }

        /// <summary>
        /// Hard reset — wipe save and reload scene.
        /// </summary>
        public void HardReset()
        {
            SaveSystem.DeleteSave();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        // ═══════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════

        private void SetGameState(GameState newState)
        {
            if (CurrentGameState == newState) return;
            CurrentGameState = newState;
            OnGameStateChanged?.Invoke(newState);
        }

        private void NotifyStateChanged()
        {
            OnStateChanged?.Invoke(State);
        }

        private void LogBattle(string message)
        {
            OnBattleLog?.Invoke(message);
        }

        public void ShowToast(string message)
        {
            OnToast?.Invoke(message);
        }

        /// <summary>
        /// Public accessor for GameConfig (used by subsystems).
        /// </summary>
        public GameConfig Config => config;

        /// <summary>
        /// Set config at runtime (bootstrap).
        /// </summary>
        public void SetConfig(GameConfig c) => config = c;
    }
}
