using System.Collections.Generic;
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
				new Color(1f, 0.15f, 0f),
				ElementalWardBehavior.Element.Fire,
				WardConfig.FireRadius.Value,
				WardConfig.ParseIngredients(WardConfig.FireIngredients.Value));

			RegisterElemental(
				"piece_ward_frost",
				"$hw_ward_frost",
				"$hw_ward_frost_desc",
				new Color(0.85f, 0.95f, 1f),
				ElementalWardBehavior.Element.Frost,
				WardConfig.FrostRadius.Value,
				WardConfig.ParseIngredients(WardConfig.FrostIngredients.Value));

			RegisterElemental(
				"piece_ward_poison",
				"$hw_ward_poison",
				"$hw_ward_poison_desc",
				new Color(0.1f, 0.9f, 0.1f),
				ElementalWardBehavior.Element.Poison,
				WardConfig.PoisonRadius.Value,
				WardConfig.ParseIngredients(WardConfig.PoisonIngredients.Value));

			RegisterElemental(
				"piece_ward_lightning",
				"$hw_ward_lightning",
				"$hw_ward_lightning_desc",
				new Color(0.2f, 0.75f, 1f),
				ElementalWardBehavior.Element.Lightning,
				WardConfig.LightningRadius.Value,
				WardConfig.ParseIngredients(WardConfig.LightningIngredients.Value));

			RegisterElemental(
				"piece_ward_spirit",
				"$hw_ward_spirit",
				"$hw_ward_spirit_desc",
				new Color(0.65f, 0.1f, 1f),
				ElementalWardBehavior.Element.Spirit,
				WardConfig.SpiritRadius.Value,
				WardConfig.ParseIngredients(WardConfig.SpiritIngredients.Value));

			RegisterSpecial<RepairWardBehavior>(
				"piece_ward_repair",
				"$hw_ward_repair",
				"$hw_ward_repair_desc",
				new Color(1f, 0.45f, 0f),
				WardConfig.RepairRadius.Value,
				WardConfig.ParseIngredients(WardConfig.RepairIngredients.Value));

			RegisterSpecial<HealingWardBehavior>(
				"piece_ward_healing",
				"$hw_ward_healing",
				"$hw_ward_healing_desc",
				new Color(1f, 0.35f, 0.65f),
				WardConfig.HealRadius.Value,
				WardConfig.ParseIngredients(WardConfig.HealIngredients.Value));

			PrefabManager.OnVanillaPrefabsAvailable -= RegisterWards;
		}

		private static void RegisterElemental(
			string prefabName, string displayName, string desc,
			Color lightColor, ElementalWardBehavior.Element element,
			float radius,
			params RequirementConfig[] reqs)
		{
			var go = CloneWard(prefabName, lightColor, displayName, radius);
			if (go == null) return;

			go.AddComponent<ElementalWardBehavior>().DamageElement = element;
			go.GetComponent<ElementalWardBehavior>().Radius = radius;
			RegisterPiece(go, displayName, desc, reqs);
		}

		private static void RegisterSpecial<T>(
			string prefabName, string displayName, string desc,
			Color lightColor, float radius,
			params RequirementConfig[] reqs) where T : WardBehavior
		{
			var go = CloneWard(prefabName, lightColor, displayName, radius);
			if (go == null) return;

			go.AddComponent<T>();
			go.GetComponent<T>().Radius = radius;
			RegisterPiece(go, displayName, desc, reqs);
		}

		private static GameObject? CloneWard(string name, Color lightColor, string displayName, float radius)
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
			hwa.m_radius           = radius;
			hwa.m_enabledByDefault = true;
			hwa.m_enabledEffect    = pa.m_enabledEffect;
			hwa.m_model            = pa.m_model;
			hwa.m_areaMarker       = pa.m_areaMarker;
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
			RequirementConfig[] reqs)
		{
			PieceManager.Instance.AddPiece(new CustomPiece(go, fixReference: true, new PieceConfig
			{
				Name            = displayName,
				Description     = desc,
				PieceTable      = "_HammerPieceTable",
				Category        = "Misc",
				CraftingStation = "piece_workbench",
				Requirements    = reqs,
			}));
		}

	}
}
