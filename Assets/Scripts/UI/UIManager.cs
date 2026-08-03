// ═══════════════════════════════════════════════════════
// UIManager.cs — Top-level UI coordinator
// ═══════════════════════════════════════════════════════

using UnityEngine;

namespace InfinityRPG
{
    /// <summary>
    /// Coordinates all UI panels. Subscribes to GameManager events
    /// and routes state changes to the appropriate panel controllers.
    ///
    /// Attach to the Canvas GameObject.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        [Header("Panels")]
        [SerializeField] private HUDController hud;
        [SerializeField] private ShopPanel shopPanel;
        [SerializeField] private EquipPanel equipPanel;
        [SerializeField] private LevelUpPanel levelUpPanel;
        [SerializeField] private GameObject hubButtons;
        [SerializeField] private GameObject runResultPanel;
        [SerializeField] private TMPro.TextMeshProUGUI battleLogText;
        [SerializeField] private GameObject toastObject;
        [SerializeField] private TMPro.TextMeshProUGUI toastText;

        private void Start()
        {
            if (gameManager == null)
                gameManager = GameManager.Instance;

            // Subscribe to events
            gameManager.OnStateChanged += OnStateChanged;
            gameManager.OnGameStateChanged += OnGameStateChanged;
            gameManager.OnBattleLog += OnBattleLog;
            gameManager.OnToast += OnToast;

            // Initial state
            RefreshAll();
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnStateChanged -= OnStateChanged;
                gameManager.OnGameStateChanged -= OnGameStateChanged;
                gameManager.OnBattleLog -= OnBattleLog;
                gameManager.OnToast -= OnToast;
            }
        }

        private void OnStateChanged(PlayerState state)
        {
            hud?.Refresh(state, gameManager);
            shopPanel?.Refresh(gameManager);
            equipPanel?.Refresh(gameManager);
        }

        private void OnGameStateChanged(GameState newState)
        {
            // Hub buttons visible only in Hub state
            hubButtons?.SetActive(newState == GameState.Hub || newState == GameState.GameOver);

            // Level-up modal
            levelUpPanel?.gameObject.SetActive(newState == GameState.LevelUp);
            if (newState == GameState.LevelUp)
                levelUpPanel?.Show(gameManager);

            // Run result
            if (newState == GameState.GameOver)
                ShowRunResult();
            else
                runResultPanel?.SetActive(false);
        }

        private void OnBattleLog(string message)
        {
            if (battleLogText != null)
                battleLogText.text = message;
        }

        private void OnToast(string message)
        {
            StopAllCoroutines();
            StartCoroutine(ShowToastRoutine(message));
        }

        private System.Collections.IEnumerator ShowToastRoutine(string message)
        {
            if (toastText != null) toastText.text = message;
            toastObject?.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            toastObject?.SetActive(false);
        }

        private void ShowRunResult()
        {
            runResultPanel?.SetActive(true);
            // Detailed text set by GameManager's EndRun log
        }

        private void RefreshAll()
        {
            OnStateChanged(gameManager.State);
            OnGameStateChanged(gameManager.CurrentGameState);
        }

        // ═══════════════════════════════════════════════
        //  BUTTON HANDLERS (wired via Inspector)
        // ═══════════════════════════════════════════════

        public void OnStartRunClicked()
        {
            gameManager.StartRun();
        }

        public void OnShopToggleClicked()
        {
            shopPanel?.gameObject.SetActive(!shopPanel.gameObject.activeSelf);
            equipPanel?.gameObject.SetActive(false);
        }

        public void OnEquipToggleClicked()
        {
            equipPanel?.gameObject.SetActive(!equipPanel.gameObject.activeSelf);
            shopPanel?.gameObject.SetActive(false);
        }

        public void OnHardResetClicked()
        {
            // Confirm dialog is handled by the button's onClick wiring
            gameManager.HardReset();
        }
    }
}
