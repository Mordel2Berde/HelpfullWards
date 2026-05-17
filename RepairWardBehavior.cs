using UnityEngine;

namespace HelpfullWards
{
	/// <summary>
	/// Automatically repairs damaged pieces within the ward radius.
	/// Runs only on the ZNet owner (server in multiplayer).
	/// Repair is free (no materials consumed).
	/// </summary>
	public class RepairWardBehavior : WardBehavior
	{
		protected override float Interval => WardConfig.RepairInterval.Value;

		private void Start()
		{
			// Seed the tracker with pieces already damaged at load time
			DamagedPieceTracker.SeedFromAllInstances();
		}

		protected override bool Tick()
		{
			playFlash = false;
			float   r2  = Radius * Radius;
			Vector3 pos = transform.position;

			foreach (var wnt in DamagedPieceTracker.Damaged)
			{
				if (wnt == null) continue;
				if ((wnt.transform.position - pos).sqrMagnitude > r2) continue;

				var nview = wnt.GetComponent<ZNetView>();
				if (nview == null || !nview.IsValid()) continue;

				nview.InvokeRPC(nview.GetZDO().GetOwner(), "RPC_Repair");

				var piece = wnt.GetComponent<Piece>();
				piece?.m_placeEffect.Create(wnt.transform.position, wnt.transform.rotation);

				return true;
			}
			return false;
		}
	}
}
