using System.Collections;
using UnityEngine;

namespace HelpfullWards
{
	/// <summary>
	/// Abstract base class for all ward behaviors.
	/// Handles ZNetView, randomized timer, and the Update loop.
	/// Subclasses only need to implement the interval and the action to trigger.
	/// </summary>
	public abstract class WardBehavior : MonoBehaviour
	{
		private ZNetView        _nview    = null!;
		private HelpfulWardArea _wardArea = null!;
		private float           _timer;
		private Light?          _wardLight;
		private float           _baseIntensity;

		public float Radius;
		protected abstract float Interval { get; }
		protected abstract bool Tick();
		protected bool playFlash = true;

		/// <summary>
		/// Optional additional sound prefab played on tick (via ZNetScene).
		/// Override in subclasses to play a specific sound effect.
		/// </summary>
		protected virtual string? TickSoundPrefab => null;

		private float RandomOffset()
			=> (Random.Range(0f, 2f * Interval) - Interval) / 10f;

		private void Awake()
		{
			_nview    = GetComponent<ZNetView>();
			_wardArea = GetComponent<HelpfulWardArea>();
			_timer    = RandomOffset();

			_wardLight     = GetComponentInChildren<Light>();
			_baseIntensity = _wardLight != null ? _wardLight.intensity : 1f;

			// RPC triggered on all clients to play the visual effect
			_nview.Register("HW_PlayTickEffect", (long _) => PlayTickEffect());
		}

		private void Update()
		{
			if (_nview == null || !_nview.IsValid() || !_nview.IsOwner()) return;
			if (!_wardArea.IsEnabled()) return;

			_timer += Time.deltaTime;
			if (_timer < Interval) return;
			_timer = RandomOffset();

			if (Tick())
				_nview.InvokeRPC(ZNetView.Everybody, "HW_PlayTickEffect");
		}

		private void PlayTickEffect()
		{
			if (playFlash)
				_wardArea.m_flashEffect.Create(transform.position, Quaternion.identity);

			if (TickSoundPrefab != null)
			{
				var sfxPrefab = ZNetScene.instance?.GetPrefab(TickSoundPrefab);
				if (sfxPrefab != null)
					UnityEngine.Object.Instantiate(sfxPrefab, transform.position, Quaternion.identity);
			}

			if (_wardLight == null) return;
			StopCoroutine("PulseLight");
			StartCoroutine(PulseLight());
		}

		private IEnumerator PulseLight()
		{
			const float duration      = 0.6f;
			const float peakMultiplier = 4f;

			// Rise
			float elapsed = 0f;
			while (elapsed < duration / 2f)
			{
				float t = elapsed / (duration / 2f);
				_wardLight!.intensity = _baseIntensity * Mathf.Lerp(1f, peakMultiplier, t);
				elapsed += Time.deltaTime;
				yield return null;
			}

			// Fall
			elapsed = 0f;
			while (elapsed < duration / 2f)
			{
				float t = elapsed / (duration / 2f);
				_wardLight!.intensity = _baseIntensity * Mathf.Lerp(peakMultiplier, 1f, t);
				elapsed += Time.deltaTime;
				yield return null;
			}

			_wardLight!.intensity = _baseIntensity;
		}
	}
}
