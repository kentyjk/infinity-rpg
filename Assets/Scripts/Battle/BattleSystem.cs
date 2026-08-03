// ═══════════════════════════════════════════════════════
// BattleSystem.cs — Auto-battle resolution
// ═══════════════════════════════════════════════════════

using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InfinityRPG
{
    /// <summary>
    /// Result of a resolved battle.
    /// </summary>
    public struct BattleResult
    {
        public bool victory;
        public string enemyName;
        public int expGained;
        public int goldGained;
        public int playerHPAfter;
        public int hpHealed;
    }

    /// <summary>
    /// Handles auto-battle resolution. Deterministic turn-based combat
    /// with AGI-based initiative and damage variance.
    ///
    /// Attach to the GameManager GameObject or a child.
    /// </summary>
    public class BattleSystem : MonoBehaviour
    {
        /// <summary>
        /// Resolve an auto-battle between the player and an enemy.
        /// Returns result via callback (supports async animation delays).
        /// </summary>
        public void ResolveBattle(GameManager gm, EnemyRuntimeData enemy, Action<BattleResult> onComplete)
        {
            var config = gm.Config;
            var state = gm.State;

            // BP soft-check: if player is WAY too weak, instant death
            int playerBP = gm.EffectiveBP;
            if (playerBP < enemy.BPReq * config.bpMinThreshold && enemy.TileType != TileType.Bonus)
            {
                onComplete?.Invoke(new BattleResult
                {
                    victory = false,
                    enemyName = enemy.DisplayName
                });
                return;
            }

            // --- Simulate combat ---
            int playerHP = state.currentHP;
            int enemyHP = enemy.currentHP;
            int playerATK = gm.EffectiveATK;
            int playerDEF = gm.EffectiveDEF;
            int playerAGI = gm.EffectiveAGI;
            bool playerFirst = playerAGI >= enemy.Agility;
            int turns = 0;

            while (playerHP > 0 && enemyHP > 0 && turns < config.maxBattleTurns)
            {
                turns++;

                if (playerFirst)
                {
                    // Player attacks
                    int dmg = Mathf.Max(1, playerATK - enemy.Defense +
                        Mathf.RoundToInt(playerATK * Random.Range(0f, config.damageVariance)));
                    enemyHP -= dmg;

                    if (enemyHP <= 0) break;

                    // Enemy attacks
                    int eDmg = Mathf.Max(1, enemy.Attack - playerDEF +
                        Mathf.RoundToInt(enemy.Attack * Random.Range(0f, config.damageVariance)));
                    playerHP -= eDmg;
                }
                else
                {
                    // Enemy attacks first
                    int eDmg = Mathf.Max(1, enemy.Attack - playerDEF +
                        Mathf.RoundToInt(enemy.Attack * Random.Range(0f, config.damageVariance)));
                    playerHP -= eDmg;

                    if (playerHP <= 0) break;

                    // Player attacks
                    int dmg = Mathf.Max(1, playerATK - enemy.Defense +
                        Mathf.RoundToInt(playerATK * Random.Range(0f, config.damageVariance)));
                    enemyHP -= dmg;
                }
            }

            if (playerHP <= 0)
            {
                // Defeat
                onComplete?.Invoke(new BattleResult
                {
                    victory = false,
                    enemyName = enemy.DisplayName
                });
                return;
            }

            // --- Victory ---
            float recoveryPct = gm.HPRecoveryPercent;
            int maxHP = gm.EffectiveMaxHP;
            int hpHealed = Mathf.RoundToInt(maxHP * recoveryPct / 100f);
            int finalHP = Mathf.Min(maxHP, playerHP + hpHealed);

            float expMult = 1f + gm.EXPBoostPercent / 100f;
            int expGain = Mathf.RoundToInt(enemy.ExpReward * expMult);

            float goldMult = 1f + gm.GoldBoostPercent / 100f;
            int goldGain = Mathf.RoundToInt(enemy.GoldReward * goldMult);

            onComplete?.Invoke(new BattleResult
            {
                victory = true,
                enemyName = enemy.DisplayName,
                expGained = expGain,
                goldGained = goldGain,
                playerHPAfter = finalHP,
                hpHealed = hpHealed
            });
        }
    }
}
