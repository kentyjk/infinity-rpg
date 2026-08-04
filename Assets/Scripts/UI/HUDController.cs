// ═══════════════════════════════════════════════════════
// HUDController.cs — Top stats bar update
// ═══════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;

namespace InfinityRPG
{
    /// <summary>
    /// Updates the top stats HUD bar. Subscribes to GameManager.OnStateChanged.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Stat Text Fields")]
        [SerializeField] private Text levelText;
        [SerializeField] private Text hpText;
        [SerializeField] private Text maxHpText;
        [SerializeField] private Text bpText;
        [SerializeField] private Text atkText;
        [SerializeField] private Text defText;
        [SerializeField] private Text agiText;
        [SerializeField] private Text runGoldText;
        [SerializeField] private Text bankGoldText;
        [SerializeField] private Text expText;

        /// <summary>
        /// Refresh all HUD fields from current state.
        /// </summary>
        public void Refresh(PlayerState state, GameManager gm)
        {
            if (levelText != null)
                levelText.text = state.level.ToString();

            if (hpText != null)
                hpText.text = Mathf.Max(0, state.currentHP).ToString();

            if (maxHpText != null)
                maxHpText.text = gm.EffectiveMaxHP.ToString();

            if (bpText != null)
                bpText.text = gm.EffectiveBP.ToString("N0");

            if (atkText != null)
                atkText.text = gm.EffectiveATK.ToString();

            if (defText != null)
                defText.text = gm.EffectiveDEF.ToString();

            if (agiText != null)
                agiText.text = gm.EffectiveAGI.ToString();

            if (runGoldText != null)
                runGoldText.text = state.runGold.ToString("N0");

            if (bankGoldText != null)
                bankGoldText.text = state.bankGold.ToString("N0");

            if (expText != null && state.runActive)
                expText.text = $"{state.exp}/{state.expToNext}";
            else if (expText != null)
                expText.text = "-";
        }
    }
}
