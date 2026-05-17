using System.Collections.Generic;
using HarmonyLib;

namespace HelpfullWards
{
	/// <summary>
	/// Maintains a static set of damaged pieces.
	/// Fed by Harmony patches on WearNTear.
	/// </summary>
	public static class DamagedPieceTracker
	{
		public static readonly HashSet<WearNTear> Damaged = new HashSet<WearNTear>();

		/// <summary>
		/// Seeds the set with pieces already damaged at load time.
		/// Call from Start() of each RepairWardBehavior.
		/// </summary>
		public static void SeedFromAllInstances()
		{
			foreach (var wnt in WearNTear.GetAllInstances())
				if (wnt != null && wnt.GetHealthPercentage() < 0.999f)
					Damaged.Add(wnt);
		}
	}

	/// <summary>Adds or removes a piece from the tracker on every health change.</summary>
	[HarmonyPatch(typeof(WearNTear), "RPC_HealthChanged")]
	static class WearNTearHealthChangedPatch
	{
		static void Postfix(WearNTear __instance, float health)
		{
			if (health < __instance.m_health * 0.999f)
				DamagedPieceTracker.Damaged.Add(__instance);
			else
				DamagedPieceTracker.Damaged.Remove(__instance);
		}
	}

	/// <summary>Removes the piece from the tracker when it is destroyed.</summary>
	[HarmonyPatch(typeof(WearNTear), "OnDestroy")]
	static class WearNTearDestroyPatch
	{
		static void Prefix(WearNTear __instance)
		{
			DamagedPieceTracker.Damaged.Remove(__instance);
		}
	}
}
