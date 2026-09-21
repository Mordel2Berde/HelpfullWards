using System.Collections.Generic;
using BepInEx.Configuration;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace HelpfullWards
{
	public static class WardFactory
	{
		private static readonly Dictionary<string, Dictionary<string, string>> _color_properties = new Dictionary<string, Dictionary<string, string>>
		{
			{
				"default", new Dictionary<string, string>
				{
					{"Guardstone_OdenGlow_mat", "_EmissionColor"}
				}
	
			},
			{
				"sparcs", new Dictionary<string, string>
				{
					{"gnista", "_Color"}
				}
			}
		};

		public static void RegisterWards()
		{
			RegisterElemental(
				"piece_ward_fire",
				"$hw_ward_fire",
				"$hw_ward_fire_desc",
				new Color(1f, 0f, 0f),
				ElementalWardBehavior.Element.Fire,
				WardConfig.FireIngredients);

			RegisterElemental(
				"piece_ward_frost",
				"$hw_ward_frost",
				"$hw_ward_frost_desc",
				new Color(0.8f, 0.8f, 0.8f),
				ElementalWardBehavior.Element.Frost,
				WardConfig.FrostIngredients);

			RegisterElemental(
				"piece_ward_poison",
				"$hw_ward_poison",
				"$hw_ward_poison_desc",
				new Color(0f, 1f, 0f),
				ElementalWardBehavior.Element.Poison,
				WardConfig.PoisonIngredients);

			RegisterElemental(
				"piece_ward_lightning",
				"$hw_ward_lightning",
				"$hw_ward_lightning_desc",
				new Color(0.5f, 0.5f, 1f),
				ElementalWardBehavior.Element.Lightning,
				WardConfig.LightningIngredients);

			RegisterElemental(
				"piece_ward_spirit",
				"$hw_ward_spirit",
				"$hw_ward_spirit_desc",
				new Color(0.5f, 0f, 1f),
				ElementalWardBehavior.Element.Spirit,
				WardConfig.SpiritIngredients);

			RegisterSpecial<RepairWardBehavior>(
				"piece_ward_repair",
				"$hw_ward_repair",
				"$hw_ward_repair_desc",
				new Color(0.8f, 0.5f, 0f),
				WardConfig.RepairIngredients);

			RegisterSpecial<HealingWardBehavior>(
				"piece_ward_healing",
				"$hw_ward_healing",
				"$hw_ward_healing_desc",
				new Color(1f, 0f, 1f),
				WardConfig.HealIngredients);

			PrefabManager.OnVanillaPrefabsAvailable -= RegisterWards;
		}

		private static void RegisterElemental(
			string prefabName, string displayName, string desc,
			Color lightColor, ElementalWardBehavior.Element element,
			ConfigEntry<string> ingredients)
		{
			var go = CloneWard(prefabName, lightColor, displayName);
			if (go == null) return;

			go.AddComponent<ElementalWardBehavior>().DamageElement = element;
			RegisterPiece(go, displayName, desc, ingredients);
		}

		private static void RegisterSpecial<T>(
			string prefabName, string displayName, string desc,
			Color lightColor,
			ConfigEntry<string> ingredients) where T : WardBehavior
		{
			var go = CloneWard(prefabName, lightColor, displayName);
			if (go == null) return;

			go.AddComponent<T>();
			RegisterPiece(go, displayName, desc, ingredients);
		}

		private static GameObject? CloneWard(string name, Color lightColor, string displayName)
		{
			var go = PrefabManager.Instance.CreateClonedPrefab(name, "guard_stone");
			if (go == null)
			{
				Plugin.Logger.LogError($"[HelpfullWards] Failed to clone guard_stone for: {name}");
				return null;
			}

			// Replace PrivateArea with our own component (no access control, no door blocking)
			var pa  = go.GetComponent<PrivateArea>();
			var hwa = go.AddComponent<HelpfulWardArea>();
			hwa.m_name             = displayName;
			hwa.m_enabledByDefault = true;
			hwa.m_enabledEffect    = pa.m_enabledEffect;
			hwa.m_model            = pa.m_model;
			hwa.m_areaMarker       = pa.m_areaMarker;
			hwa.m_hoverOffset      = pa.m_hoverOffset;
			hwa.m_flashEffect      = pa.m_flashEffect;
			hwa.m_activateEffect   = pa.m_activateEffect;
			hwa.m_deactivateEffect = pa.m_deactivateEffect;
			UnityEngine.Object.DestroyImmediate(pa);

			foreach (var light in go.GetComponentsInChildren<Light>(includeInactive: true))
				light.color = lightColor;

			foreach (var rend in go.GetComponentsInChildren<Renderer>(includeInactive: true))
			{
				if (! _color_properties.ContainsKey(rend.name))
					continue;
				var mats = rend.sharedMaterials;
				bool changed = false;
				for (int i = 0; i < mats.Length; i++)
				{
					if (mats[i] == null)
						continue;

					if (! _color_properties[rend.name].ContainsKey(mats[i].name))
						continue;

					string prop = _color_properties[rend.name][mats[i].name];

					var mat = new Material(mats[i]);
					mat.SetColor(prop, lightColor);
					mats[i] = mat;
					changed = true;
				}
				if (changed) rend.sharedMaterials = mats;
			}


			return go;
		}

		private static void RegisterPiece(
			GameObject go, string displayName, string desc,
			ConfigEntry<string> ingredients)
		{
			var piece = new CustomPiece(go, fixReference: true, new PieceConfig
			{
				Name            = displayName,
				Description     = desc,
				PieceTable      = "_HammerPieceTable",
				Category        = "Misc",
				CraftingStation = "piece_workbench",
				Requirements    = WardConfig.ParseIngredients(ingredients.Value),
			});
			PieceManager.Instance.AddPiece(piece);

			// Fired when the server pushes its config, when local values are restored
			// on disconnect, and on in-game edits: update the recipe on the prefab.
			ingredients.SettingChanged += (_, _) =>
				piece.Piece.m_resources = WardConfig.ToRequirements(ingredients.Value);
		}

	}
}
