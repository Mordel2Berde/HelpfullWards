using UnityEngine;

namespace HelpfullWards
{
	/// <summary>
	/// Brief visual-only status effect played on each character healed by the
	/// Healing Ward. Reuses Valheim's vanilla potion heal VFX
	/// (vfx_Potion_health_medium), parented to the character so it follows.
	/// The status effect itself is replicated automatically via SEMan/ZDO,
	/// so each client spawns the visual locally.
	/// </summary>
	public class HealFlashStatusEffect : StatusEffect
	{
		public const string Name = "HW_HealFlash";
		public static int Hash => Name.GetStableHashCode();

		private const string FxPrefabName = "vfx_Potion_health_medium";

		private static HealFlashStatusEffect? _cached;

		private GameObject? _vfxInstance;

		/// <summary>
		/// Creates the SE asset (once) and registers it in ObjectDB if not
		/// already present. Idempotent across world reloads.
		/// </summary>
		public static void Register()
		{
			var odb = ObjectDB.instance;
			if (odb == null)
			{
				Plugin.Logger.LogWarning("[HealFlashSE] ObjectDB not ready");
				return;
			}
			if (odb.GetStatusEffect(Hash) != null)
			{
				Plugin.Logger.LogInfo($"[HealFlashSE] Already registered (hash={Hash})");
				return;
			}

			if (_cached == null)
			{
				_cached = ScriptableObject.CreateInstance<HealFlashStatusEffect>();
				_cached.name   = Name;          // drives NameHash()
				_cached.m_name = "$hw_heal_flash";
				_cached.m_ttl  = 1.0f;
				_cached.m_icon = null;          // hide from buff bar
				Object.DontDestroyOnLoad(_cached);
			}

			odb.m_StatusEffects.Add(_cached);
			Plugin.Logger.LogInfo(
				$"[HealFlashSE] Registered " +
				$"(hash={Hash}, ODB#{odb.GetInstanceID()}, " +
				$"list#{odb.m_StatusEffects.GetHashCode()}, count={odb.m_StatusEffects.Count})");
		}

		// We bypass base.Setup's TriggerStartEffects path because the vfx prefab
		// carries a ZNetView. If it were instantiated through m_startEffects on
		// every client, each instance would create its own ZDO and replicate,
		// resulting in N² visuals. Instead, we instantiate locally on each
		// client with ZNetView init disabled.
		public override void Setup(Character character)
		{
			Plugin.Logger.LogInfo($"[HealFlashSE] Setup ENTER on {character?.name ?? "null"} (type={GetType().Name})");
			try
			{
				m_character = character;

				var prefab = ZNetScene.instance?.GetPrefab(FxPrefabName);
				if (prefab == null)
				{
					Plugin.Logger.LogWarning($"[HealFlashSE] Prefab not found: {FxPrefabName}");
					return;
				}

				bool prev = ZNetView.m_forceDisableInit;
				ZNetView.m_forceDisableInit = true;
				try
				{
					_vfxInstance = Object.Instantiate(prefab, character.transform);
					_vfxInstance.transform.localPosition = Vector3.zero;
					_vfxInstance.transform.localRotation = Quaternion.identity;
					_vfxInstance.SetActive(true);
					Plugin.Logger.LogInfo(
						$"[HealFlashSE] Instantiated {_vfxInstance.name} " +
						$"world={_vfxInstance.transform.position} " +
						$"active={_vfxInstance.activeInHierarchy}");
				}
				finally { ZNetView.m_forceDisableInit = prev; }
			}
			catch (System.Exception e)
			{
				Plugin.Logger.LogError($"[HealFlashSE] Setup exception: {e}");
			}
		}

		public override void Stop()
		{
			// Intentionally do NOT destroy _vfxInstance here. The vfx prefab
			// (vfx_Potion_health_medium) carries ParticleSystems whose stopAction
			// destroys the GameObject when their emission cycle finishes. Cutting
			// the visual at SE TTL would clip the animation. The vfx remains
			// parented to the character transform and follows / dies with it.
			//
			// Skip base.Stop: it dereferences m_character.transform, which can
			// be null when the SE expires after the character was destroyed.
			// We have no startEffects / stopEffects / stopMessage to handle.
			_vfxInstance = null;
		}
	}
}
