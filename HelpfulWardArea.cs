using System;
using UnityEngine;

namespace HelpfullWards
{
	/// <summary>
	/// Replaces PrivateArea on custom wards.
	/// Handles enable/disable toggling and visual feedback only.
	/// Does NOT participate in access control (door blocking, building restriction, player list).
	/// </summary>
	public class HelpfulWardArea : MonoBehaviour, Hoverable, Interactable
	{
		public string           m_name             = "";
		public float            m_radius           = 10f;
		public bool             m_enabledByDefault = true;
		public GameObject?      m_enabledEffect;
		public MeshRenderer?    m_model;
		public CircleProjector? m_areaMarker;
		public EffectList       m_flashEffect      = new EffectList();
		public EffectList       m_activateEffect   = new EffectList();
		public EffectList       m_deactivateEffect = new EffectList();

		private ZNetView m_nview       = null!;
		private Piece    m_piece       = null!;
		private bool     m_flashAvailable = true;


		private void Awake()
		{
			m_nview = GetComponent<ZNetView>();
			if (!m_nview.IsValid()) return;

			m_piece = GetComponent<Piece>();

			var wnt = GetComponent<WearNTear>();
			if (wnt != null)
				wnt.m_onDamaged = (Action)Delegate.Combine(wnt.m_onDamaged, new Action(OnDamaged));

			if (m_areaMarker != null)
				m_areaMarker.gameObject.SetActive(false);

			InvokeRepeating(nameof(UpdateStatus), 0f, 1f);
			m_nview.Register<long>("ToggleEnabled", RPC_ToggleEnabled);
			m_nview.Register("FlashShield", RPC_FlashShield);

			if (m_enabledByDefault && m_nview.IsOwner())
				m_nview.GetZDO().Set(ZDOVars.s_enabled, true);
		}

		public bool IsEnabled()
		{
			if (!m_nview.IsValid()) return false;
			return m_nview.GetZDO().GetBool(ZDOVars.s_enabled);
		}

		private void SetEnabled(bool enabled)
		{
			m_nview.GetZDO().Set(ZDOVars.s_enabled, enabled);
			UpdateStatus();
			if (enabled)
				m_activateEffect.Create(transform.position, transform.rotation);
			else
				m_deactivateEffect.Create(transform.position, transform.rotation);
		}

		private void UpdateStatus()
		{
			bool enabled = IsEnabled();

			if (m_enabledEffect != null)
				m_enabledEffect.SetActive(enabled);

			m_flashAvailable = true;

			if (m_model != null)
			{
				foreach (var mat in m_model.materials)
				{
					if (enabled) mat.EnableKeyword("_EMISSION");
					else         mat.DisableKeyword("_EMISSION");
				}
			}
		}

		public string GetHoverName() => m_name;

		public string GetHoverText()
		{
			if (!m_nview.IsValid() || Player.m_localPlayer == null)
				return "";

			ShowAreaMarker();

			string status = IsEnabled() ? "$piece_guardstone_active" : "$piece_guardstone_inactive";
			string text   = $"{m_name} ( {status} )";

			if (m_piece.IsCreator())
			{
				string action = IsEnabled() ? "$piece_guardstone_deactivate" : "$piece_guardstone_activate";
				text += $"\n[<color=yellow><b>$KEY_Use</b></color>] {action}";
			}

			return Localization.instance.Localize(text);
		}

		public bool Interact(Humanoid human, bool hold, bool alt)
		{
			if (hold) return false;
			if (!m_piece.IsCreator()) return false;
			m_nview.InvokeRPC("ToggleEnabled", ((Player)human).GetPlayerID());
			return true;
		}

		public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;

		private void RPC_ToggleEnabled(long uid, long playerID)
		{
			if (m_nview.IsOwner() && m_piece.GetCreator() == playerID)
				SetEnabled(!IsEnabled());
		}

		private void RPC_FlashShield(long uid)
		{
			m_flashEffect.Create(transform.position, Quaternion.identity);
		}

		public void ShowAreaMarker()
		{
			if (m_areaMarker == null) return;
			m_areaMarker.gameObject.SetActive(true);
			CancelInvoke(nameof(HideMarker));
			Invoke(nameof(HideMarker), 0.5f);
		}

		private void HideMarker()
		{
			if (m_areaMarker != null)
				m_areaMarker.gameObject.SetActive(false);
		}

		private void OnDamaged()
		{
			if (!IsEnabled() || !m_flashAvailable) return;
			m_flashAvailable = false;
			m_nview.InvokeRPC(ZNetView.Everybody, "FlashShield");
		}
	}
}
