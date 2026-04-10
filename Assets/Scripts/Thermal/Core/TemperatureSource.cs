using System.Collections.Generic;
using UnityEngine;

namespace IceFactory.Thermal.Core
{
    [DisallowMultipleComponent]
    public sealed class TemperatureSource : MonoBehaviour
    {
        [Header("Output")]
        [SerializeField] private TemperatureType temperatureType = TemperatureType.Heat;
        [SerializeField] [Min(0f)] private float amountPerTick = 1f;
        [SerializeField] [Min(0.01f)] private float tickInterval = 0.2f;

        [Header("Range")]
        [SerializeField] private bool useTriggerMode = true;
        [SerializeField] [Min(0.1f)] private float overlapRadius = 1.5f;
        [SerializeField] private LayerMask targetLayers = ~0;

        public TemperatureType Type => temperatureType;
        public float AmountPerTick => amountPerTick;
        public float TickInterval => tickInterval;
        public bool UseTriggerMode => useTriggerMode;
        public float OverlapRadius => overlapRadius;
        public LayerMask TargetLayers => targetLayers;

        private readonly HashSet<IThermalInteractable> _cachedTargets = new HashSet<IThermalInteractable>();
        private readonly Collider[] _overlapResults = new Collider[32];
        private float _nextTickTime;

        public void Configure(
            TemperatureType type,
            float amount,
            float interval,
            bool triggerMode,
            float radius,
            LayerMask layers)
        {
            temperatureType = type;
            amountPerTick = Mathf.Max(0f, amount);
            tickInterval = Mathf.Max(0.01f, interval);
            useTriggerMode = triggerMode;
            overlapRadius = Mathf.Max(0.1f, radius);
            targetLayers = layers;

            _cachedTargets.Clear();
            _nextTickTime = 0f;
        }

        private void Update()
        {
            if (Time.time < _nextTickTime)
            {
                return;
            }

            _nextTickTime = Time.time + tickInterval;
            EmitTemperature();
        }

        private void EmitTemperature()
        {
            if (useTriggerMode)
            {
                EmitToCachedTargets();
                return;
            }

            EmitWithOverlapSphere();
        }

        private void EmitToCachedTargets()
        {
            if (_cachedTargets.Count == 0)
            {
                return;
            }

            var payload = new TemperaturePayload(temperatureType, amountPerTick, gameObject, transform.position);
            foreach (var target in _cachedTargets)
            {
                if (target == null || !target.CanReceiveTemperature(payload.Type))
                {
                    continue;
                }

                target.OnReceiveTemperature(payload);
            }
        }

        private void EmitWithOverlapSphere()
        {
            var count = Physics.OverlapSphereNonAlloc(
                transform.position,
                overlapRadius,
                _overlapResults,
                targetLayers,
                QueryTriggerInteraction.Collide);

            if (count <= 0)
            {
                return;
            }

            var payload = new TemperaturePayload(temperatureType, amountPerTick, gameObject, transform.position);
            for (var i = 0; i < count; i++)
            {
                var hit = _overlapResults[i];
                if (hit == null)
                {
                    continue;
                }

                var interactables = hit.GetComponents<IThermalInteractable>();
                foreach (var interactable in interactables)
                {
                    if (interactable != null && interactable.CanReceiveTemperature(payload.Type))
                    {
                        interactable.OnReceiveTemperature(payload);
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!useTriggerMode)
            {
                return;
            }

            RegisterTarget(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!useTriggerMode)
            {
                return;
            }

            UnregisterTarget(other);
        }

        private void RegisterTarget(Component targetComponent)
        {
            if (((1 << targetComponent.gameObject.layer) & targetLayers.value) == 0)
            {
                return;
            }

            var interactables = targetComponent.GetComponents<IThermalInteractable>();
            foreach (var interactable in interactables)
            {
                if (interactable != null)
                {
                    _cachedTargets.Add(interactable);
                }
            }
        }

        private void UnregisterTarget(Component targetComponent)
        {
            var interactables = targetComponent.GetComponents<IThermalInteractable>();
            foreach (var interactable in interactables)
            {
                if (interactable != null)
                {
                    _cachedTargets.Remove(interactable);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (useTriggerMode)
            {
                return;
            }

            Gizmos.color = temperatureType == TemperatureType.Heat ? new Color(1f, 0.5f, 0f, 0.6f) : new Color(0.2f, 0.9f, 1f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, overlapRadius);
        }
    }
}
