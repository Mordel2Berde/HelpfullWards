using System.Collections.Generic;
using System.IO;
using Jotunn.Entities;
using Jotunn.Managers;
using Newtonsoft.Json;

namespace HelpfullWards
{
	/// <summary>
	/// Loads translation files from BepInEx/plugins/.../Translations/*.json.
	/// Each file is named after the language (e.g. English.json, French.json).
	/// Default files are created automatically if missing.
	/// </summary>
	public static class TranslationLoader
	{
		private static readonly string TranslationsDir = Path.Combine(
			Path.GetDirectoryName(typeof(Plugin).Assembly.Location)!,
			"Translations");

		public static void Load()
		{
			if (!Directory.Exists(TranslationsDir))
			{
				Directory.CreateDirectory(TranslationsDir);
				WriteDefaults();
			}

			foreach (var file in Directory.GetFiles(TranslationsDir, "*.json"))
			{
				string language = Path.GetFileNameWithoutExtension(file);
				try
				{
					string json = File.ReadAllText(file, System.Text.Encoding.UTF8);
					var entries = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
					if (entries == null || entries.Count == 0) continue;

					var loc = LocalizationManager.Instance.GetLocalization();
					foreach (var kv in entries)
						loc.AddTranslation(language, kv.Key, kv.Value);

					Plugin.Logger.LogInfo(
						$"[HelpfullWards] Translations loaded: {language} ({entries.Count} entries)");
				}
				catch (System.Exception e)
				{
					Plugin.Logger.LogError($"[HelpfullWards] Error reading {file}: {e.Message}");
				}
			}
		}

		// ─── Default translation files ────────────────────────────────────────

		private static void WriteDefaults()
		{
			Write("English", new Dictionary<string, string>
			{
				{ "hw_ward_fire",           "Fire Ward"             },
				{ "hw_ward_fire_desc",      "Deals fire damage periodically to enemies within range."      },
				{ "hw_ward_frost",          "Frost Ward"            },
				{ "hw_ward_frost_desc",     "Deals frost damage periodically to enemies within range."     },
				{ "hw_ward_poison",         "Poison Ward"           },
				{ "hw_ward_poison_desc",    "Deals poison damage periodically to enemies within range."    },
				{ "hw_ward_lightning",      "Lightning Ward"        },
				{ "hw_ward_lightning_desc", "Deals lightning damage periodically to enemies within range." },
				{ "hw_ward_spirit",         "Spirit Ward"           },
				{ "hw_ward_spirit_desc",    "Deals spirit damage periodically to enemies within range."    },
				{ "hw_ward_repair",         "Repair Ward"           },
				{ "hw_ward_repair_desc",    "Automatically repairs damaged constructions within range."    },
				{ "hw_ward_healing",        "Healing Ward"          },
				{ "hw_ward_healing_desc",   "Periodically heals players within range."                    },
				});

			Write("French", new Dictionary<string, string>
			{
				{ "hw_ward_fire",           "Balise de feu"              },
				{ "hw_ward_fire_desc",      "Inflige des dégâts de feu périodiques aux ennemis dans son rayon."     },
				{ "hw_ward_frost",          "Balise de givre"            },
				{ "hw_ward_frost_desc",     "Inflige des dégâts de givre périodiques aux ennemis dans son rayon."   },
				{ "hw_ward_poison",         "Balise de poison"           },
				{ "hw_ward_poison_desc",    "Inflige des dégâts de poison périodiques aux ennemis dans son rayon."  },
				{ "hw_ward_lightning",      "Balise de foudre"           },
				{ "hw_ward_lightning_desc", "Inflige des dégâts de foudre périodiques aux ennemis dans son rayon."  },
				{ "hw_ward_spirit",         "Balise spirituelle"            },
				{ "hw_ward_spirit_desc",    "Inflige des dégâts d'esprit périodiques aux ennemis dans son rayon."   },
				{ "hw_ward_repair",         "Balise de Réparation"       },
				{ "hw_ward_repair_desc",    "Répare automatiquement les constructions endommagées dans son rayon."  },
				{ "hw_ward_healing",        "Balise de Soin"             },
				{ "hw_ward_healing_desc",   "Soigne périodiquement les joueurs dans son rayon."                     },
				});
		}

		private static void Write(string language, Dictionary<string, string> entries)
		{
			string path = Path.Combine(TranslationsDir, $"{language}.json");
			if (File.Exists(path)) return;

			File.WriteAllText(path,
				JsonConvert.SerializeObject(entries, Formatting.Indented),
				System.Text.Encoding.UTF8);

			Plugin.Logger.LogInfo($"[HelpfullWards] Translation file created: {path}");
		}
	}
}
