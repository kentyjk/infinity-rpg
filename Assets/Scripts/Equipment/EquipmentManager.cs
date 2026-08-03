// ═══════════════════════════════════════════════════════
// EquipmentManager.cs — Equipment ownership and loadout management
// ═══════════════════════════════════════════════════════

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityRPG
{
    /// <summary>
    /// Manages equipment ownership, equipping, and stat computation.
    /// Mostly delegates to GameManager for state mutations.
    ///
    /// This class provides the query API for UI panels.
    /// </summary>
    public class EquipmentManager : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        // ═══════════════════════════════════════════════
        //  OWNERSHIP QUERIES
        // ═══════════════════════════════════════════════

        public bool OwnsWeapon(string id) => gameManager.State.ownedWeapons.Contains(id);
        public bool OwnsArmor(string id) => gameManager.State.ownedArmors.Contains(id);
        public bool OwnsAccessory(string id) => gameManager.State.ownedAccessories.Contains(id);

        public bool IsWeaponEquipped(string id) => gameManager.State.equippedWeaponId == id;
        public bool IsArmorEquipped(string id) => gameManager.State.equippedArmorId == id;
        public bool IsAccessoryEquipped(string id) => gameManager.State.equippedAccessoryId == id;

        // ═══════════════════════════════════════════════
        //  SHOP ITEM LISTS (for UI)
        // ═══════════════════════════════════════════════

        /// <summary>
        /// All shop items merged into one list for display.
        /// Each entry includes ownership and equipped status.
        /// </summary>
        public List<ShopItemEntry> GetAllShopItems()
        {
            var items = new List<ShopItemEntry>();

            foreach (var w in gameManager.Config.allWeapons)
            {
                items.Add(new ShopItemEntry
                {
                    icon = "⚔️",
                    displayName = w.displayName,
                    statDescription = $"ATK+{w.attackBonus}",
                    cost = w.cost,
                    itemId = w.itemId,
                    slot = EquipmentSlot.Weapon,
                    owned = OwnsWeapon(w.itemId),
                    equipped = IsWeaponEquipped(w.itemId)
                });
            }

            foreach (var a in gameManager.Config.allArmors)
            {
                items.Add(new ShopItemEntry
                {
                    icon = "🛡️",
                    displayName = a.displayName,
                    statDescription = $"DEF+{a.defenseBonus}",
                    cost = a.cost,
                    itemId = a.itemId,
                    slot = EquipmentSlot.Armor,
                    owned = OwnsArmor(a.itemId),
                    equipped = IsArmorEquipped(a.itemId)
                });
            }

            foreach (var x in gameManager.Config.allAccessories)
            {
                items.Add(new ShopItemEntry
                {
                    icon = "💍",
                    displayName = x.displayName,
                    statDescription = x.EffectDescription,
                    cost = x.cost,
                    itemId = x.itemId,
                    slot = EquipmentSlot.Accessory,
                    owned = OwnsAccessory(x.itemId),
                    equipped = IsAccessoryEquipped(x.itemId)
                });
            }

            return items;
        }

        /// <summary>
        /// Get owned items for equip dropdowns, keyed by slot.
        /// </summary>
        public (List<ShopItemEntry> weapons, List<ShopItemEntry> armors, List<ShopItemEntry> accessories) GetEquippableItems()
        {
            var weapons = gameManager.State.ownedWeapons
                .Select(id => new ShopItemEntry
                {
                    displayName = gameManager.Config.GetWeapon(id)?.displayName ?? "???",
                    itemId = id,
                    slot = EquipmentSlot.Weapon,
                    equipped = IsWeaponEquipped(id)
                }).ToList();

            var armors = gameManager.State.ownedArmors
                .Select(id => new ShopItemEntry
                {
                    displayName = gameManager.Config.GetArmor(id)?.displayName ?? "???",
                    itemId = id,
                    slot = EquipmentSlot.Armor,
                    equipped = IsArmorEquipped(id)
                }).ToList();

            var accessories = gameManager.State.ownedAccessories
                .Select(id => new ShopItemEntry
                {
                    displayName = gameManager.Config.GetAccessory(id)?.displayName ?? "???",
                    itemId = id,
                    slot = EquipmentSlot.Accessory,
                    equipped = IsAccessoryEquipped(id)
                }).ToList();

            return (weapons, armors, accessories);
        }
    }

    /// <summary>
    /// Lightweight DTO for shop/equip UI items.
    /// </summary>
    [System.Serializable]
    public class ShopItemEntry
    {
        public string icon;
        public string displayName;
        public string statDescription;
        public int cost;
        public string itemId;
        public EquipmentSlot slot;
        public bool owned;
        public bool equipped;
    }
}
