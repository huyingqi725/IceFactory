using IceFactory.Thermal.Core;
using UnityEngine;

namespace IceFactory.Gameplay.Trap
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TemperatureSource))]
    [RequireComponent(typeof(Collider))]
    public sealed class ColdCondensePlateSource : MonoBehaviour
    {
        [Header("Cold Plate Preset")]
        [SerializeField] [Min(0f)] private float amountPerTick = 1f;
        [SerializeField] [Min(0.01f)] private float tickInterval = 0.1f;
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private bool forceTriggerCollider = true;

        [Header("Optional")]
        [SerializeField] private bool autoApplyOnAwake = true;

        private TemperatureSource _temperatureSource;
        private Collider _plateCollider;

        private void Awake()
        {
            CacheComponents();
            if (autoApplyOnAwake)
            {
                ApplyPreset();
            }
        }

        private void Reset()
        {
            CacheComponents();
            ApplyPreset();
        }

        private void OnValidate()
        {
            CacheComponents();
            ApplyPreset();
        }

        [ContextMenu("Apply Cold Plate Preset")]
        public void ApplyPreset()
        {
            if (_temperatureSource == null)
            {
                return;
            }

            _temperatureSource.Configure(
                TemperatureType.Cold,
                amountPerTick,
                tickInterval,
                true,
                1f,
                targetLayers);

            if (forceTriggerCollider && _plateCollider != null)
            {
                _plateCollider.isTrigger = true;
            }
        }

        private void CacheComponents()
        {
            if (_temperatureSource == null)
            {
                _temperatureSource = GetComponent<TemperatureSource>();
            }

            if (_plateCollider == null)
            {
                _plateCollider = GetComponent<Collider>();
            }
        }
    }
}
