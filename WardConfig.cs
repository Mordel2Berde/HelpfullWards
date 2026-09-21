using System.Collections.Generic;
using BepInEx.Configuration;
using Jotunn.Configs;
using Jotunn.Managers;

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

		private static HashSet<Character.Faction>? _excludedFactions;

		private static ConfigEntry<T> Bind<T>(ConfigFile cfg, string section, string key, T value, string description)
		{
			return cfg.Bind(section, key, value, new ConfigDescription(description, null,
				new ConfigurationManagerAttributes { IsAdminOnly = true }));
		}

		public static void Init(ConfigFile cfg)
		{
			ElementalTickInterval = Bind(cfg, "Elemental", "TickInterval", 11f,
				"Seconds between each elemental damage tick.");

			ElementalExcludedFactions = Bind(cfg, "Elemental", "ExcludedFactions",
				"Players,Dverger",
				"Factions excluded from elemental damage (comma-separated).\n" +
				"Possible values: Players, AnimalsVeg, ForestMonsters, Undead, Demon, " +
				"MountainMonsters, SeaMonsters, PlainsMonsters, Boss, DeepNorth");

			FireDamage         = Bind(cfg, "Ward_Fire",      "Damage",       11f,  "Fire damage per tick.");
			FireRadius         = Bind(cfg, "Ward_Fire",      "Radius",       32f,  "Fire ward radius (meters).");
			FireIngredients    = Bind(cfg, "Ward_Fire",      "Ingredients",  "FineWood:11,TrophySurtling:3,SurtlingCore:1", IngredientsDesc);

			FrostDamage        = Bind(cfg, "Ward_Frost",     "Damage",       11f,  "Frost damage per tick.");
			FrostRadius        = Bind(cfg, "Ward_Frost",     "Radius",       32f,  "Frost ward radius (meters).");
			FrostIngredients   = Bind(cfg, "Ward_Frost",     "Ingredients",  "FineWood:11,TrophyHatchling:3,SurtlingCore:1", IngredientsDesc);

			PoisonDamage       = Bind(cfg, "Ward_Poison",    "Damage",       11f,  "Poison damage per tick.");
			PoisonRadius       = Bind(cfg, "Ward_Poison",    "Radius",       32f,  "Poison ward radius (meters).");
			PoisonIngredients  = Bind(cfg, "Ward_Poison",    "Ingredients",  "FineWood:11,TrophyBlob:3,SurtlingCore:1", IngredientsDesc);

			LightningDamage      = Bind(cfg, "Ward_Lightning", "Damage",       11f,  "Lightning damage per tick.");
			LightningRadius      = Bind(cfg, "Ward_Lightning", "Radius",       32f,  "Lightning ward radius (meters).");
			LightningIngredients = Bind(cfg, "Ward_Lightning", "Ingredients",  "FineWood:11,Crystal:3,SurtlingCore:1", IngredientsDesc);

			SpiritDamage       = Bind(cfg, "Ward_Spirit",    "Damage",       11f,  "Spirit damage per tick.");
			SpiritRadius       = Bind(cfg, "Ward_Spirit",    "Radius",       32f,  "Spirit ward radius (meters).");
			SpiritIngredients  = Bind(cfg, "Ward_Spirit",    "Ingredients",  "FineWood:11,TrophyGhost:3,SurtlingCore:1", IngredientsDesc);

			RepairInterval     = Bind(cfg, "Ward_Repair",   "Interval",     11f,  "Seconds between each automatic repair.");
			RepairRadius       = Bind(cfg, "Ward_Repair",   "Radius",       32f,  "Repair ward radius (meters).");
			RepairIngredients  = Bind(cfg, "Ward_Repair",   "Ingredients",  "FineWood:11,Root:3,SurtlingCore:1", IngredientsDesc);

			HealInterval       = Bind(cfg, "Ward_Healing",  "Interval",     11f,  "Seconds between each automatic heal.");
			HealAmount         = Bind(cfg, "Ward_Healing",  "HealAmount",   11f,  "Hit points restored per tick.");
			HealRadius         = Bind(cfg, "Ward_Healing",  "Radius",       32f,  "Healing ward radius (meters).");
			HealIngredients    = Bind(cfg, "Ward_Healing",  "Ingredients",  "FineWood:11,TrophyGreydwarfShaman:3,SurtlingCore:1", IngredientsDesc);

			ElementalExcludedFactions.SettingChanged += (_, _) => _excludedFactions = null;
		}

		public static HashSet<Character.Faction> ExcludedFactions
		{
			get
			{
				if (_excludedFactions != null)
					return _excludedFactions;
				_excludedFactions = new HashSet<Character.Faction>();
				foreach (var s in ElementalExcludedFactions.Value.Split(','))
					if (System.Enum.TryParse(s.Trim(), out Character.Faction f))
						_excludedFactions.Add(f);
				return _excludedFactions;
			}
		}

		public static Piece.Requirement[] ToRequirements(string value)
		{
			var result = new List<Piece.Requirement>();
			foreach (var req in ParseIngredients(value))
			{
				var item = PrefabManager.Cache.GetPrefab<ItemDrop>(req.Item);
				if (item == null)
				{
					Plugin.Logger.LogWarning($"[HelpfullWards] Unknown ingredient item: {req.Item}");
					continue;
				}
				result.Add(new Piece.Requirement { m_resItem = item, m_amount = req.Amount, m_recover = req.Recover });
			}
			return result.ToArray();
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
