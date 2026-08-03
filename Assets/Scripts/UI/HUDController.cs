// ═══════════════════════════════════════════════════════
// HUDController.cs — Top stats bar update
// ═══════════════════════════════════════════════════════

using TMPro;
using UnityEngine;

namespace InfinityRPG
{
    /// <summary>
    /// Updates the top stats HUD bar. Subscribes to GameManager.OnStateChanged.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Stat Text Fields")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI maxHpText;
        [SerializeField] private TextMeshProUGUI bpText;
        [SerializeField] private TextMeshProUGUI atkText;
        [SerializeField] private TextMeshProUGUI defText;
        [SerializeField] private TextMeshProUGUI agiText;
        [SerializeField] private TextMeshProUGUI runGoldText;
        [SerializeField] private TextMeshProUGUI bankGoldText;
        [SerializeField] private TextMeshProUGUI expText;

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
