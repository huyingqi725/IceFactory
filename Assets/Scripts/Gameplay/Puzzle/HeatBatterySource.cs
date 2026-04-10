using IceFactory.Thermal.Core;
using UnityEngine;

namespace IceFactory.Gameplay.Puzzle
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TemperatureSource))]
    [RequireComponent(typeof(Collider))]
    public sealed class HeatBatterySource : MonoBehaviour
    {
        [Header("Heat Battery Preset")]
        [SerializeField] [Min(0f)] private float amountPerTick = 1f;
        [SerializeField] [Min(0.01f)] private float tickInterval = 0.2f;
        [SerializeField] private bool useTriggerMode = true;
        [SerializeField] [Min(0.1f)] private float overlapRadius = 1.5f;
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private bool forceTriggerCollider = true;

        [Header("Activation")]
        [SerializeField] private bool startEmittingOnAwake = false;

        private TemperatureSource _temperatureSource;
        private Collider _batteryCollider;

        private void Awake()
        {
            CacheComponents();
            ApplyPreset();
            SetEmissionEnabled(startEmittingOnAwake);
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

        [ContextMenu("Apply Heat Battery Preset")]
        public void ApplyPreset()
        {
            if (_temperatureSource == null)
            {
                return;
            }

            _temperatureSource.Configure(
                TemperatureType.Heat,
                amountPerTick,
                tickInterval,
                useTriggerMode,
                overlapRadius,
                targetLayers);

            if (forceTriggerCollider && _batteryCollider != null)
            {
                _batteryCollider.isTrigger = true;
            }
        }

        public void SetEmissionEnabled(bool enabled)
        {
            if (_temperatureSource == null)
            {
                CacheComponents();
            }

            if (_temperatureSource != null)
            {
                _temperatureSource.enabled = enabled;
            }
        }

        private void CacheComponents()
        {
            if (_temperatureSource == null)
            {
                _temperatureSource = GetComponent<TemperatureSource>();
            }

            if (_batteryCollider == null)
            {
                _batteryCollider = GetComponent<Collider>();
            }
        }
    }
}
