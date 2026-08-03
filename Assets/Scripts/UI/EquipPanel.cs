// ═══════════════════════════════════════════════════════
// EquipPanel.cs — Equipment loadout selection UI
// ═══════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;

namespace InfinityRPG
{
    /// <summary>
    /// Dropdown-based equipment loadout manager.
    /// Shows dropdown for each slot (weapon/armor/accessory)
    /// populated with owned items.
    /// </summary>
    public class EquipPanel : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private EquipmentManager equipmentManager;

        [Header("Dropdowns")]
        [SerializeField] private Dropdown weaponDropdown;
        [SerializeField] private Dropdown armorDropdown;
        [SerializeField] private Dropdown accessoryDropdown;

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

            var (weapons, armors, accessories) = equipmentManager.GetEquippableItems();

            PopulateDropdown(weaponDropdown, weapons, EquipmentSlot.Weapon);
            PopulateDropdown(armorDropdown, armors, EquipmentSlot.Armor);
            PopulateDropdown(accessoryDropdown, accessories, EquipmentSlot.Accessory);
        }

        private void PopulateDropdown(Dropdown dropdown, System.Collections.Generic.List<ShopItemEntry> items, EquipmentSlot slot)
        {
            if (dropdown == null) return;

            dropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string> { "— None —" };
            int selectedIndex = 0;

            for (int i = 0; i < items.Count; i++)
            {
                options.Add(items[i].displayName);
                if (items[i].equipped) selectedIndex = i + 1;
            }

            dropdown.AddOptions(options);
            dropdown.value = selectedIndex;

            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener((index) =>
            {
                string itemId = index == 0 ? null : items[index - 1].itemId;
                switch (slot)
                {
                    case EquipmentSlot.Weapon:    gameManager.EquipWeapon(itemId); break;
                    case EquipmentSlot.Armor:     gameManager.EquipArmor(itemId); break;
                    case EquipmentSlot.Accessory: gameManager.EquipAccessory(itemId); break;
                }
            });
        }
    }
}
