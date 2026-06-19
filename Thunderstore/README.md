# HelpfullWards

HelpfullWards adds a family of craftable, fully configurable wards to Valheim.
Each ward is built like a vanilla ward and acts on everything within its radius.

## Wards

### Elemental wards (damage enemies)
Every few seconds each elemental ward deals damage to nearby enemies. Players
and Dvergr are excluded by default (configurable).

| Ward | Damage type |
|------|-------------|
| Fire Ward | Fire |
| Frost Ward | Frost |
| Poison Ward | Poison |
| Lightning Ward | Lightning |
| Spirit Ward | Spirit |

### Utility wards
- **Repair Ward** — automatically repairs damaged constructions within range.
- **Healing Ward** — periodically heals et remove effects on players within range.

## Configuration

Everything is editable through the BepInEx config file
(`BepInEx/config/Mordel2Berde.HelpfullWards.cfg`) or an in-game config manager:

- Damage / heal amount per tick
- Radius of each ward
- Tick / repair / heal interval
- Crafting recipe of each ward (`ItemName:Amount`, comma-separated)
- Factions excluded from elemental damage

## Translations

English and French are bundled. Additional languages can be added by dropping a
`<Language>.json` file into the `Translations` folder next to the plugin DLL.

## Installation

Use a mod manager (Thunderstore Mod Manager / r2modman) — recommended.

Manual install: extract the archive and place `HelpfullWards.dll` and the
`Translations` folder into `BepInEx/plugins/`. Requires
[Jotunn](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/).

## Dependencies

- BepInExPack for Valheim
- Jotunn (the Valheim Library)
- JsonDotNET
