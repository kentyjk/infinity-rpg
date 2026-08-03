# Infinity RPG — Unity Project Setup Guide

## Overview
A complete Unity project for a mobile Roguelite Auto-Battler RPG inspired by **Inflation RPG** and **Hero's Quest**.

**Architecture**: MVC-like with ScriptableObject data, singleton GameManager, event-driven UI.

## Prerequisites
1. **Unity Hub** — download from [unity.com](https://unity.com)
2. **Unity Editor 2022.3 LTS** (or newer) — install via Unity Hub
   - Required modules: **Android Build Support** (if targeting mobile), **iOS Build Support** (optional)
3. **Unity Account** — free Personal license (revenue < $200K/year)

## Project Setup

### 1. Create Unity Project
1. Open Unity Hub → **New Project**
2. Template: **2D (Built-in Render Pipeline)**
3. Project Name: `InfinityRPG`
4. Location: wherever you keep projects

### 2. Import Scripts
Copy the entire `Assets/` folder from this directory into your Unity project's `Assets/` folder, or:

```bash
cp -r /opt/data/InfinityRPG_Unity/Assets/* /path/to/your/UnityProject/Assets/
```

### 3. Create ScriptableObjects

#### GameConfig (CRITICAL — do this first)
1. Right-click in Project window → **Create → InfinityRPG → Game Config**
2. Name it `GameConfig`
3. Move to `Assets/Resources/` (create the folder if needed)
4. Fill in the fields:

**Map Settings:**
- Map Width: `10`
- Map Height: `12`
- Tile Size: `1`

**Zones (drag & drop from step 4):**
- Create Zone assets (see below), then assign them in order (highest difficulty first)

**Equipment Database:**
- Create Weapon/Armor/Accessory assets (see below), then assign to arrays

**Progression:**
- Stat Points Per Level: `4`
- EXP Curve Multiplier: `1.35`
- Base EXP To Next: `80`

**Battle:**
- BP Min Threshold: `0.3`
- Damage Variance: `0.3`
- Max Battle Turns: `100`

#### Create Item Assets
For each item, right-click → **Create → InfinityRPG → Weapon/Armor/Accessory**:

**Weapons:**
| ID | Name | ATK | Cost | Tier |
|----|------|-----|------|------|
| w0 | Rusty Sword | 8 | 150 | 0 |
| w1 | Iron Blade | 20 | 600 | 1 |
| w2 | Steel Sword | 50 | 2500 | 2 |
| w3 | Flame Saber | 120 | 10000 | 3 |
| w4 | Dragon Slayer | 300 | 40000 | 4 |

**Armors:**
| ID | Name | DEF | Cost | Tier |
|----|------|-----|------|------|
| a0 | Leather Vest | 5 | 120 | 0 |
| a1 | Chain Mail | 15 | 500 | 1 |
| a2 | Plate Armor | 40 | 2000 | 2 |
| a3 | Mithril Coat | 100 | 8000 | 3 |
| a4 | Dragon Scale | 250 | 35000 | 4 |

**Accessories:**
| ID | Name | Effect | Value | Cost |
|----|------|--------|-------|------|
| x0 | Recovery Ring | HPRecovery | 2 | 1500 |
| x1 | EXP Charm | EXPBoost | 25 | 3000 |
| x2 | BP Ring +2 | BPBoost | 2 | 2000 |
| x3 | Gold Amulet | GoldBoost | 20 | 2500 |
| x4 | Vitality Band | HPBoost | 80 | 4000 |
| x5 | Recovery Amulet | HPRecovery | 5 | 10000 |

#### Create Zone Assets
Create 6 Zone assets:

| Zone | BP Min | BP Max | Bonus Chance |
|------|--------|--------|--------------|
| Dragon Lair | 3000 | 15000 | 0.08 |
| Volcanic Depths | 800 | 3000 | 0.08 |
| Dark Caverns | 200 | 800 | 0.08 |
| Goblin Forest | 50 | 200 | 0.08 |
| Slime Plains | 10 | 50 | 0.08 |
| Starting Town | 0 | 0 | 0 |

For each combat zone, create EnemyData assets for common enemies and bosses.

#### Create Enemy Assets
Create enemy prefabs for each zone tier. Example for Slime Plains:
- **Common**: Slime — HP:30, ATK:8, DEF:3, AGI:5, BPReq:25, EXP:100, Gold:15
- **Boss**: Slime King — HP:200, ATK:15, DEF:8, AGI:8, BPReq:80, EXP:1000, Gold:150
- **Bonus**: Treasure Spirit — HP:50, ATK:1, DEF:1, AGI:1, BPReq:1, EXP:5000, Gold:500

### 4. Scene Setup

#### Create Main Scene
1. **Canvas** (Screen Space - Overlay)
   - Add `UIManager` component
   - Child: **HUD Panel** → Add `HUDController`, wire TextMeshPro fields
   - Child: **Shop Panel** → Add `ShopPanel`, create shop item prefab with `ShopItemRow`
   - Child: **Equip Panel** → Add `EquipPanel`, create 3 Dropdowns
   - Child: **LevelUp Panel** → Add `LevelUpPanel`, create +/- buttons and Confirm button
   - Child: **Battle Log Text** (TextMeshPro)
   - Child: **Hub Buttons** (Start Run, Shop, Equip, Reset)
   - Child: **Run Result Panel**
   - Child: **Toast** (TextMeshPro)

2. **GameManager GameObject** (empty, named "GameManager")
   - Add `GameManager` component
   - Assign `GameConfig` from Resources
   - Add `BattleSystem` component as child
   - Add `MapManager` component as child
   - Add `EquipmentManager` component as child
   - Wire all [SerializeField] references in Inspector

3. **Map GameObject** (empty, child of GameManager)
   - Add a `MapRenderer` implementation (use Unity Tilemap or manual sprite instantiation)
   - Add `PlayerController` for tap-to-move
   - Map tiles should have BoxCollider2D and `MapTile` component

#### Wire References
In the Inspector for `UIManager`:
- Drag GameManager → `gameManager`
- Drag each panel → corresponding slot
- Drag TMP texts → corresponding slots

### 5. Build Settings

#### Android
1. File → Build Settings → Switch Platform to Android
2. Player Settings:
   - Resolution: 1080x1920 Portrait
   - **Auto Graphics API**: disable, set OpenGL ES 3.2
   - Package Name: `com.yourname.infinityrpg`
   - Minimum API Level: 26 (Android 8.0)
   - Target API Level: 34

#### iOS
1. File → Build Settings → Switch Platform to iOS
2. Player Settings:
   - Camera Usage Description: "Required for AR features" (if using)
   - Target minimum iOS: 14.0

### 6. Performance Targets
- **60 FPS** on mid-range devices (Snapdragon 720G / A12 Bionic)
- **Texture compression**: ASTC (Android), PVRTC (iOS)
- **Sprite atlas**: Use for all UI and map tiles
- **Canvas**: Use multiple Canvases — separate ones for HUD (static) and panels (dynamic)

## Architecture

```
GameManager (Singleton, DontDestroyOnLoad)
├── GameConfig (ScriptableObject — all item/zone/enemy data)
├── PlayerState (Serializable — save/load data)
├── SaveSystem (static — PlayerPrefs JSON)
├── BattleSystem (auto-battle resolution)
├── MapManager (grid map, tile data)
│   └── MapRenderer (abstract — implement for your visual style)
├── EquipmentManager (ownership queries, shop item lists)
├── PlayerController (tap-to-move input)
└── UIManager (event-driven, coordinates panels)
    ├── HUDController
    ├── ShopPanel → ShopItemRow
    ├── EquipPanel
    └── LevelUpPanel
```

## Key Design Decisions

1. **No MonoBehaviour singletons for subsystems** — all injected via `[SerializeField]` in GameManager. Easy to test, replace, or mock.

2. **Event-driven UI** — UIManager subscribes to `GameManager.OnStateChanged`, `OnBattleLog`, etc. UI never reaches into game state directly; it only reacts to events.

3. **ScriptableObject data** — items, enemies, zones are assets, not hardcoded. Designers can tweak without touching code. A `GameConfig` asset holds all references.

4. **Save as JSON** — PlayerState is a plain C# class serialized to JSON via `JsonUtility`. Easy to debug, extend, and migrate.

5. **Battle is pure logic** — BattleSystem has no MonoBehaviour dependencies beyond Random. Can be unit tested independently.

6. **Map is abstracted** — `MapRenderer` is an abstract class. Implement it with Unity Tilemap, sprites, or even UI images. The map logic doesn't care.

## Extending

### Add a new weapon
1. Create → InfinityRPG → Weapon asset
2. Fill in stats
3. Add to GameConfig.allWeapons array
4. Done — shop automatically picks it up

### Add a new zone
1. Create → InfinityRPG → Zone asset
2. Add enemy pool
3. Add to GameConfig.zones array (ordered highest difficulty first)
4. Update `GetZoneForRow()` in GameConfig if row mapping changes

### Add PvP / Multiplayer
1. Add a `MultiplayerManager` as a child of GameManager
2. Use Unity Netcode for GameObjects or Photon Fusion
3. BattleSystem and MapManager are already stateless (all state in PlayerState)

## Troubleshooting

**"GameConfig not assigned"**
→ Create the asset in Resources/ and drag it into GameManager's Inspector.

**Shop items don't appear**
→ Verify all item arrays in GameConfig are populated.
→ Verify EquipmentManager is assigned in GameManager.

**Tapping map tiles does nothing**
→ Ensure tiles have BoxCollider2D + MapTile component.
→ Check that PlayerController.mainCamera is assigned.

**Save not persisting**
→ PlayerPrefs requires a valid bundle identifier in Player Settings.
→ On Android, PlayerPrefs is cleared on app uninstall.
