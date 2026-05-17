using System.Collections.Generic;
using BepInEx.Configuration;
using Jotunn.Configs;

namespace HelpfullWards
{
	public static class WardConfig
	{
		// ── Elemental Wards ─────────────────────────────────────────────────
		public static ConfigEntry<float>  ElementalTickInterval     = null!;
		public static ConfigEntry<string> ElementalExcludedFactions = null!;

		public static ConfigEntry<float>  FireDamage         = null!;
		public static ConfigEntry<float>  FireRadius         = null!;
		public static ConfigEntry<string> FireIngredients    = null!;

		public static ConfigEntry<float>  FrostDamage        = null!;
		public static ConfigEntry<float>  FrostRadius        = null!;
		public static ConfigEntry<string> FrostIngredients   = null!;

		public static ConfigEntry<float>  PoisonDamage       = null!;
		public static ConfigEntry<float>  PoisonRadius       = null!;
		public static ConfigEntry<string> PoisonIngredients  = null!;

		public static ConfigEntry<float>  LightningDamage       = null!;
		public static ConfigEntry<float>  LightningRadius        = null!;
		public static ConfigEntry<string> LightningIngredients   = null!;

		public static ConfigEntry<float>  SpiritDamage       = null!;
		public static ConfigEntry<float>  SpiritRadius       = null!;
		public static ConfigEntry<string> SpiritIngredients  = null!;

		// ── Repair Ward ─────────────────────────────────────────────────────
		public static ConfigEntry<float>  RepairInterval     = null!;
		public static ConfigEntry<float>  RepairRadius       = null!;
		public static ConfigEntry<string> RepairIngredients  = null!;

		// ── Healing Ward ────────────────────────────────────────────────────
		public static ConfigEntry<float>  HealInterval       = null!;
		public static ConfigEntry<float>  HealAmount         = null!;
		public static ConfigEntry<float>  HealRadius         = null!;
		public static ConfigEntry<string> HealIngredients    = null!;

		private const string IngredientsDesc =
			"Crafting ingredients (comma-separated, format: ItemName:Amount).";

		public static void Init(ConfigFile cfg)
		{
			ElementalTickInterval = cfg.Bind("Elemental", "TickInterval", 3f,
				"Seconds between each elemental damage tick.");

			ElementalExcludedFactions = cfg.Bind("Elemental", "ExcludedFactions",
				"Players,Dverger",
				"Factions excluded from elemental damage (comma-separated).\n" +
				"Possible values: Players, AnimaI, ForestMonsters, Undead, Demon, " +
				"MountainMonsters, SeaMonsters, PlainsMonsters, Boss");

			FireDamage         = cfg.Bind("Ward_Fire",      "Damage",       10f,  "Fire damage per tick.");
			FireRadius         = cfg.Bind("Ward_Fire",      "Radius",       32f,  "Fire ward radius (meters).");
			FireIngredients    = cfg.Bind("Ward_Fire",      "Ingredients",  "FineWood:5,TrophySurtling:11,Eitr:1", IngredientsDesc);

			FrostDamage        = cfg.Bind("Ward_Frost",     "Damage",       10f,  "Frost damage per tick.");
			FrostRadius        = cfg.Bind("Ward_Frost",     "Radius",       32f,  "Frost ward radius (meters).");
			FrostIngredients   = cfg.Bind("Ward_Frost",     "Ingredients",  "FineWood:5,TrophyHatchling:11,Eitr:1", IngredientsDesc);

			PoisonDamage       = cfg.Bind("Ward_Poison",    "Damage",       10f,  "Poison damage per tick.");
			PoisonRadius       = cfg.Bind("Ward_Poison",    "Radius",       32f,  "Poison ward radius (meters).");
			PoisonIngredients  = cfg.Bind("Ward_Poison",    "Ingredients",  "FineWood:5,TrophyBlob:11,Eitr:1", IngredientsDesc);

			LightningDamage      = cfg.Bind("Ward_Lightning", "Damage",       10f,  "Lightning damage per tick.");
			LightningRadius      = cfg.Bind("Ward_Lightning", "Radius",       32f,  "Lightning ward radius (meters).");
			LightningIngredients = cfg.Bind("Ward_Lightning", "Ingredients",  "FineWood:5,Crystal:11,Eitr:1", IngredientsDesc);

			SpiritDamage       = cfg.Bind("Ward_Spirit",    "Damage",       10f,  "Spirit damage per tick.");
			SpiritRadius       = cfg.Bind("Ward_Spirit",    "Radius",       32f,  "Spirit ward radius (meters).");
			SpiritIngredients  = cfg.Bind("Ward_Spirit",    "Ingredients",  "FineWood:5,TrophyGhost:11,Eitr:1", IngredientsDesc);

			RepairInterval     = cfg.Bind("Ward_Repair",   "Interval",     10f,  "Seconds between each automatic repair.");
			RepairRadius       = cfg.Bind("Ward_Repair",   "Radius",       32f,  "Repair ward radius (meters).");
			RepairIngredients  = cfg.Bind("Ward_Repair",   "Ingredients",  "FineWood:5,YggdrasilWood:11,Eitr:1", IngredientsDesc);

			HealInterval       = cfg.Bind("Ward_Healing",  "Interval",     10f,  "Seconds between each automatic heal.");
			HealAmount         = cfg.Bind("Ward_Healing",  "HealAmount",   10f,  "Hit points restored per tick.");
			HealRadius         = cfg.Bind("Ward_Healing",  "Radius",       32f,  "Healing ward radius (meters).");
			HealIngredients    = cfg.Bind("Ward_Healing",  "Ingredients",  "FineWood:5,TrophyGreydwarfShaman:11,Eitr:1", IngredientsDesc);
		}

		public static HashSet<Character.Faction> GetExcludedFactions()
		{
			var result = new HashSet<Character.Faction>();
			foreach (var s in ElementalExcludedFactions.Value.Split(','))
				if (System.Enum.TryParse(s.Trim(), out Character.Faction f))
					result.Add(f);
			return result;
		}

		public static RequirementConfig[] ParseIngredients(string value)
		{
			var result = new List<RequirementConfig>();
			foreach (var entry in value.Split(','))
			{
				var parts = entry.Trim().Split(':');
				if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int amount))
					result.Add(new RequirementConfig { Item = parts[0].Trim(), Amount = amount, Recover = true });
			}
			return result.ToArray();
		}
	}
}
