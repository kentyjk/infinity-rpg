// ═══════════════════════════════════════════════════════
// GameEnums.cs — All enums used across the game
// ═══════════════════════════════════════════════════════

namespace InfinityRPG
{
    public enum ZoneType
    {
        SlimePlains,
        GoblinForest,
        DarkCaverns,
        VolcanicDepths,
        DragonLair,
        StartingTown
    }

    public enum EquipmentSlot
    {
        Weapon,
        Armor,
        Accessory
    }

    public enum TileType
    {
        Empty,
        Enemy,
        Boss,
        Bonus,
        Town
    }

    public enum StatType
    {
        HP,
        ATK,
        DEF,
        AGI
    }

    public enum GameState
    {
        Hub,        // Between runs — shop/equip
        Exploring,  // Moving on the map
        Battling,   // Battle animation playing
        LevelUp,    // Level-up modal open
        GameOver    // Run ended
    }
}
