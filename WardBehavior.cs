using System.Collections;
using UnityEngine;

namespace HelpfullWards
{
	public abstract class WardBehavior : MonoBehaviour
	{
		private const float MaxCharge = 1f;

		private const float MinGlyphEmission = 0.5f;
		private const float MaxGlyphEmission = 3f;
		private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

		private ZNetView _nview = null!;
		private HelpfulWardArea _wardArea = null!;
		private Light? _wardLight;
		private float _baseIntensity;
		private Material? _glyphMaterial;
		private Color _glyphBaseEmission;
		private float _charge = MaxCharge;
		private float _rechargeDuration;

		public float Radius;
		protected abstract float Interval { get; }
		protected abstract bool Tick();
		protected bool playFlash = true;
		protected virtual string? TickSoundPrefab => null;

		private void Awake()
		{
			_nview = GetComponent<ZNetView>();
			_wardArea = GetComponent<HelpfulWardArea>();
			_wardLight = GetComponentInChildren<Light>();
			_baseIntensity = _wardLight != null ? _wardLight.intensity : 1f;
			foreach (var rend in GetComponentsInChildren<Renderer>(includeInactive: true))
			{
				var shared = rend.sharedMaterials;
				for (int i = 0; i < shared.Length; i++)
				{
					if (shared[i] == null
						|| !shared[i].name.StartsWith("Guardstone_OdenGlow_mat")
						|| !shared[i].HasProperty(EmissionColorId))
						continue;

					_glyphMaterial = rend.materials[i];
					_glyphBaseEmission = _glyphMaterial.GetColor(EmissionColorId);
				}
			}
			ApplyGlyphCharge(_charge);
			_nview.Register<float>("HW_Discharge", RPC_Discharge);
		}

		private void ApplyGlyphCharge(float charge)
		{
			if (_glyphMaterial == null) return;
			float factor = Mathf.Lerp(MinGlyphEmission, MaxGlyphEmission, charge);
			_glyphMaterial.SetColor(EmissionColorId, _glyphBaseEmission * factor);
		}

		private void Update()
		{
			if (_nview == null || !_nview.IsValid() || !_nview.IsOwner()) return;
			if (!_wardArea.IsEnabled()) return;

			if (_charge < MaxCharge) return;

			if (!Tick()) return;

			float duration = Interval + (Random.Range(0f, 2f * Interval) - Interval) / 10f;
			_nview.InvokeRPC(ZNetView.Everybody, "HW_Discharge", duration);
		}

		private void RPC_Discharge(long sender, float duration)
		{
			PlayTickEffect();

			_charge = 0f;
			_rechargeDuration = Mathf.Max(0.01f, duration);

			if (_wardLight != null)
				_wardLight.intensity = 0f;
			ApplyGlyphCharge(_charge);

			StopCoroutine(nameof(Recharge));
			StartCoroutine(nameof(Recharge));
		}

		private IEnumerator Recharge()
		{
			float elapsed = 0f;
			while (elapsed < _rechargeDuration)
			{
				elapsed += Time.deltaTime;
				_charge = Mathf.Min(MaxCharge, MaxCharge * elapsed / _rechargeDuration);
				if (_wardLight != null)
					_wardLight.intensity = _baseIntensity * _charge;
				ApplyGlyphCharge(_charge);
				yield return null;
			}
			_charge = MaxCharge;
			if (_wardLight != null)
				_wardLight.intensity = _baseIntensity;
			ApplyGlyphCharge(_charge);
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
		}
	}
}
