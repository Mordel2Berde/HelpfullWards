using System.Collections.Generic;
using UnityEngine;

namespace HelpfullWards
{
	/// <summary>
	/// Periodically heals all players and tamed creatures within the ward radius.
	/// Runs only on the ZNet owner (server in multiplayer).
	/// </summary>
	public class HealingWardBehavior : WardBehavior
	{
		public override float Radius   => WardConfig.HealRadius.Value;
		protected override float Interval => WardConfig.HealInterval.Value;
		protected override string? TickSoundPrefab => "sfx_dverger_heal_finish";

		// Only harmful effects: removing everything would also strip equipment SEs
		// (e.g. Megingjord), which vanilla never re-applies until the gear changes.
		private static readonly int[] CleansedEffects =
		{
			SEMan.s_statusEffectBurning, SEMan.s_statusEffectPoison,
			SEMan.s_statusEffectFrost,   SEMan.s_statusEffectLightning,
			SEMan.s_statusEffectSpirit,  SEMan.s_statusEffectSmoked,
			SEMan.s_statusEffectWet,     SEMan.s_statusEffectTared,
		};

		protected override bool Tick()
		{
			playFlash = false;
			bool acted = false;
			var characters = new List<Character>();
			Character.GetCharactersInRange(transform.position, Radius, characters);
			foreach (var c in characters)
			{
				if (c == null || c.IsMonsterFaction(Time.time)) continue;
				if (c.GetHealth() >= c.GetMaxHealth()) continue;
				acted = true;
				foreach (var hash in CleansedEffects)
					c.GetSEMan().RemoveStatusEffect(hash, true);
				c.Heal(WardConfig.HealAmount.Value, true);
				HealFlashStatusEffect.Register();   // defensive: SE may have been wiped by another ObjectDB hook
				c.GetSEMan().AddStatusEffect(HealFlashStatusEffect.Hash, resetTime: true);
			}
			return acted;
		}
	}
}
