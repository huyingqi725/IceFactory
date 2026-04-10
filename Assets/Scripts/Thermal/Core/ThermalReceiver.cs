using System;
using UnityEngine;
using UnityEngine.Events;

namespace IceFactory.Thermal.Core
{
    [DisallowMultipleComponent]
    public sealed class ThermalReceiver : MonoBehaviour, IThermalInteractable
    {
        [SerializeField] private bool allowHeat = true;
        [SerializeField] private bool allowCold = true;

        [Serializable]
        public sealed class TemperatureUnityEvent : UnityEvent<TemperatureType, float>
        {
        }

        public event Action<TemperaturePayload> TemperatureReceived;

        [SerializeField] private TemperatureUnityEvent onTemperatureReceived = new TemperatureUnityEvent();

        public bool CanReceiveTemperature(TemperatureType type)
        {
            if (type == TemperatureType.Heat)
            {
                return allowHeat;
            }

            return allowCold;
        }

        public void OnReceiveTemperature(TemperaturePayload payload)
        {
            TemperatureReceived?.Invoke(payload);
            onTemperatureReceived.Invoke(payload.Type, payload.Amount);
        }
    }
}
