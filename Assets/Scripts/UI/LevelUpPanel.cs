// ═══════════════════════════════════════════════════════
// LevelUpPanel.cs — Stat allocation modal on level-up
// ═══════════════════════════════════════════════════════

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfinityRPG
{
    /// <summary>
    /// Modal panel for allocating stat points after level-up.
    /// Shows remaining SP and +/- buttons for HP/ATK/DEF/AGI.
    /// </summary>
    public class LevelUpPanel : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI spRemainingText;
        [SerializeField] private TextMeshProUGUI hpAllocText;
        [SerializeField] private TextMeshProUGUI atkAllocText;
        [SerializeField] private TextMeshProUGUI defAllocText;
        [SerializeField] private TextMeshProUGUI agiAllocText;
        [SerializeField] private Button confirmButton;

        [Header("Allocation Counters")]
        private int allocHP;
        private int allocATK;
        private int allocDEF;
        private int allocAGI;

        private void Awake()
        {
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        public void Show(GameManager gm)
        {
            gameManager = gm;
            allocHP = 0;
            allocATK = 0;
            allocDEF = 0;
            allocAGI = 0;
            UpdateDisplay();
        }

        // ═══════════════════════════════════════════════
        //  BUTTON HANDLERS (wired via Inspector)
        // ═══════════════════════════════════════════════

        public void OnAddHP()
        {
            if (gameManager.State.statPoints <= 0) return;
            gameManager.State.statPoints--;
            allocHP++;
            UpdateDisplay();
        }

        public void OnAddATK()
        {
            if (gameManager.State.statPoints <= 0) return;
            gameManager.State.statPoints--;
            allocATK++;
            UpdateDisplay();
        }

        public void OnAddDEF()
        {
            if (gameManager.State.statPoints <= 0) return;
            gameManager.State.statPoints--;
            allocDEF++;
            UpdateDisplay();
        }

        public void OnAddAGI()
        {
            if (gameManager.State.statPoints <= 0) return;
            gameManager.State.statPoints--;
            allocAGI++;
            UpdateDisplay();
        }

        public void OnConfirmClicked()
        {
            gameManager.ApplyStatAllocation(allocHP, allocATK, allocDEF, allocAGI);
        }

        private void UpdateDisplay()
        {
            if (spRemainingText != null)
                spRemainingText.text = gameManager.State.statPoints.ToString();

            if (hpAllocText != null)
                hpAllocText.text = allocHP.ToString();

            if (atkAllocText != null)
                atkAllocText.text = allocATK.ToString();

            if (defAllocText != null)
                defAllocText.text = allocDEF.ToString();

            if (agiAllocText != null)
                agiAllocText.text = allocAGI.ToString();
        }
    }
}
