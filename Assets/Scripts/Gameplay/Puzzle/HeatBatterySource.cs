using IceFactory.Thermal.Core;
using UnityEngine;

namespace IceFactory.Gameplay.Puzzle
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TemperatureSource))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class HeatBatterySource : MonoBehaviour
    {
        [Header("Heat Battery Preset")]
        [SerializeField] [Min(0f)] private float amountPerTick = 1f;
        [SerializeField] [Min(0.01f)] private float tickInterval = 0.2f;
        [SerializeField] private bool useTriggerMode = false;
        [SerializeField] [Min(0.1f)] private float overlapRadius = 1.5f;
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private bool forceTriggerCollider = false;

        [Header("Physics")]
        [SerializeField] private bool forceSolidCollider = true;
        [SerializeField] private bool autoSetupRigidbody = true;

        [Header("Activation")]
        [SerializeField] private bool startEmittingOnAwake = false;

        private TemperatureSource _temperatureSource;
        private Collider _batteryCollider;
        private Rigidbody _batteryRigidbody;

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

            if (forceSolidCollider && _batteryCollider != null)
            {
                _batteryCollider.isTrigger = false;
            }

            if (autoSetupRigidbody && _batteryRigidbody != null)
            {
                _batteryRigidbody.isKinematic = false;
                _batteryRigidbody.useGravity = true;
                _batteryRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                _batteryRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
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

            if (_batteryRigidbody == null)
            {
                _batteryRigidbody = GetComponent<Rigidbody>();
            }
        }
    }
}
