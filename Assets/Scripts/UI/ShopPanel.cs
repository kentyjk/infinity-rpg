// ═══════════════════════════════════════════════════════
// ShopPanel.cs — Equipment shop UI
// ═══════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;

namespace InfinityRPG
{
    /// <summary>
    /// Displays all shop items with Buy buttons.
    /// Refreshes from EquipmentManager data.
    /// </summary>
    public class ShopPanel : MonoBehaviour
    {
        [SerializeField] private EquipmentManager equipmentManager;
        [SerializeField] private GameManager gameManager;

        [Header("Prefabs")]
        [SerializeField] private GameObject shopItemPrefab;   // Prefab with ShopItemRow component

        [Header("Container")]
        [SerializeField] private Transform contentParent;

        private void OnEnable()
        {
            Refresh(gameManager);
        }

        public void Refresh(GameManager gm)
        {
            if (gm == null) gm = GameManager.Instance;
            if (equipmentManager == null)
                equipmentManager = GetComponentInParent<EquipmentManager>()
                    ?? FindAnyObjectByType<EquipmentManager>();

            if (contentParent == null || shopItemPrefab == null) return;

            // Clear existing
            foreach (Transform child in contentParent)
                Destroy(child.gameObject);

            var items = equipmentManager.GetAllShopItems();

            foreach (var item in items)
            {
                var go = Instantiate(shopItemPrefab, contentParent);
                var row = go.GetComponent<ShopItemRow>();
                if (row != null)
                    row.Setup(item, gm);
            }
        }
    }

    /// <summary>
    /// Component on each shop item row prefab.
    /// </summary>
    public class ShopItemRow : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text statText;
        [SerializeField] private Text costText;
        [SerializeField] private Button buyButton;
        [SerializeField] private GameObject ownedBadge;
        [SerializeField] private GameObject equippedBorder;

        private ShopItemEntry item;
        private GameManager gm;

        public void Setup(ShopItemEntry entry, GameManager gameManager)
        {
            item = entry;
            gm = gameManager;

            if (nameText != null) nameText.text = $"{entry.icon} {entry.displayName}";
            if (statText != null) statText.text = entry.statDescription;

            bool isOwned = entry.owned;
            bool isEquipped = entry.equipped;

            if (costText != null)
                costText.text = isOwned ? "OWNED" : $"{entry.cost:N0}g";

            if (ownedBadge != null) ownedBadge.SetActive(isOwned);
            if (equippedBorder != null) equippedBorder.SetActive(isEquipped);

            if (buyButton != null)
            {
                buyButton.gameObject.SetActive(!isOwned);
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(OnBuyClicked);
            }
        }

        private void OnBuyClicked()
        {
            if (item == null || gm == null) return;

            bool success = item.slot switch
            {
                EquipmentSlot.Weapon    => gm.BuyWeapon(gm.Config.GetWeapon(item.itemId)),
                EquipmentSlot.Armor     => gm.BuyArmor(gm.Config.GetArmor(item.itemId)),
                EquipmentSlot.Accessory => gm.BuyAccessory(gm.Config.GetAccessory(item.itemId)),
                _ => false
            };

            if (!success)
                gm.ShowToast("Not enough gold!");
        }
    }
}
