using System.Collections.Generic;
using UnityEngine;

namespace HelpfullWards
{
	public class ElementalWardBehavior : WardBehavior
	{
		public enum Element { Fire, Frost, Poison, Lightning, Spirit }

		public Element DamageElement;

		protected override float Interval => WardConfig.ElementalTickInterval.Value;

		private static HashSet<Character.Faction>? _excluded;
		private static HashSet<Character.Faction> Excluded
			=> _excluded ??= WardConfig.GetExcludedFactions();

		private static readonly Dictionary<Element, string> StatusEffectNames = new()
		{
			{ Element.Fire,   "Burning" },
			{ Element.Frost,  "Frost" },
			{ Element.Poison, "Poison" },
			{ Element.Spirit, "Spirit" },
		};

		private static readonly Dictionary<string, int> _statusEffectHashes = new();


		protected override bool Tick()
		{
			float amount = GetDamage();

			var all = new List<Character>();
			Character.GetCharactersInRange(transform.position, Radius, all);

			var targets = new List<Character>();
			foreach (var c in all)
			{
				if (c == null || Excluded.Contains(c.m_faction) || c.IsTamed()) continue;
				targets.Add(c);
			}

			if (targets.Count == 0) return false;

			var target = targets[Random.Range(0, targets.Count)];
			var hit = new HitData { m_attacker = ZDOID.None };
			switch (DamageElement)
			{
				case Element.Fire:      hit.m_damage.m_fire      = amount; break;
				case Element.Frost:     hit.m_damage.m_frost     = amount; break;
				case Element.Poison:    hit.m_damage.m_poison    = amount; break;
				case Element.Lightning: hit.m_damage.m_lightning = amount; break;
				case Element.Spirit:    hit.m_damage.m_spirit    = amount; break;
			}

			if (StatusEffectNames.TryGetValue(DamageElement, out var seName))
				hit.m_statusEffectHash = GetStatusEffectHash(seName);

			target.Damage(hit);
			return true;
		}

		private static int GetStatusEffectHash(string name)
		{
			if (_statusEffectHashes.TryGetValue(name, out var cached))
				return cached;

			int hash = name.GetStableHashCode();
			if (ObjectDB.instance != null && ObjectDB.instance.GetStatusEffect(hash) == null)
				Plugin.Logger.LogWarning(
					$"[ElementalWard] StatusEffect '{name}' not found in ObjectDB; " +
					"only direct damage will be applied.");

			_statusEffectHashes[name] = hash;
			return hash;
		}

		private float GetDamage() => DamageElement switch
		{
			Element.Fire => WardConfig.FireDamage.Value,
			Element.Frost => WardConfig.FrostDamage.Value,
			Element.Poison => WardConfig.PoisonDamage.Value,
			Element.Lightning => WardConfig.LightningDamage.Value,
			_ => WardConfig.SpiritDamage.Value,
		};
	}
}
