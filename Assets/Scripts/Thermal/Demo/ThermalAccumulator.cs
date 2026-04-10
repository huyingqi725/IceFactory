using UnityEngine;

namespace IceFactory.Thermal.Core
{
    [DisallowMultipleComponent]
    public sealed class ThermalAccumulator : MonoBehaviour
    {
        [Header("Debug Values")]
        [SerializeField] private float currentHeat;
        [SerializeField] private float currentCold;
        [SerializeField] private bool printLogs = true;

        [Header("Optional Thresholds")]
        [SerializeField] [Min(0f)] private float heatThresholdToActivate;
        [SerializeField] [Min(0f)] private float coldThresholdToActivate;

        private ThermalReceiver _receiver;

        private void Awake()
        {
            _receiver = GetComponent<ThermalReceiver>();
            if (_receiver == null)
            {
                Debug.LogError($"{nameof(ThermalAccumulator)} requires {nameof(ThermalReceiver)} on {name}.", this);
                enabled = false;
                return;
            }

            _receiver.TemperatureReceived += OnTemperatureReceived;
        }

        private void OnDestroy()
        {
            if (_receiver != null)
            {
                _receiver.TemperatureReceived -= OnTemperatureReceived;
            }
        }

        private void OnTemperatureReceived(TemperaturePayload payload)
        {
            if (payload.Type == TemperatureType.Heat)
            {
                currentHeat += payload.Amount;
            }
            else
            {
                currentCold += payload.Amount;
            }

            if (printLogs)
            {
                Debug.Log(
                    $"[{name}] receive {payload.Type} +{payload.Amount:F2} | heat={currentHeat:F2}, cold={currentCold:F2}",
                    this);
            }

            if (heatThresholdToActivate > 0f && currentHeat >= heatThresholdToActivate)
            {
                Debug.Log($"[{name}] heat threshold reached.", this);
                heatThresholdToActivate = 0f;
            }

            if (coldThresholdToActivate > 0f && currentCold >= coldThresholdToActivate)
            {
                Debug.Log($"[{name}] cold threshold reached.", this);
                coldThresholdToActivate = 0f;
            }
        }
    }
}
