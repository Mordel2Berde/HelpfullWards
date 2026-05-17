using BepInEx;
using HarmonyLib;
using Jotunn.Managers;

namespace HelpfullWards
{
	[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
	[BepInDependency(Jotunn.Main.ModGuid)]
	public class Plugin : BaseUnityPlugin
	{
		public const string PluginGUID    = "Mordel2Berde.HelpfullWards";
		public const string PluginName    = "HelpfullWards";
		public const string PluginVersion = "0.6.0";

		internal static new BepInEx.Logging.ManualLogSource Logger = null!;
		private readonly Harmony _harmony = new Harmony(PluginGUID);

		private void Awake()
		{
			Logger = base.Logger;
			WardConfig.Init(Config);
			TranslationLoader.Load();
			PrefabManager.OnVanillaPrefabsAvailable += WardFactory.RegisterWards;
			PrefabManager.OnVanillaPrefabsAvailable += HealFlashStatusEffect.Register;
			_harmony.PatchAll();
		}

		private void OnDestroy() => _harmony.UnpatchSelf();
	}

	[HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB))]
	internal static class ObjectDBCopyOtherDBPatch
	{
		[HarmonyPostfix]
		private static void Postfix() => HealFlashStatusEffect.Register();
	}
}
