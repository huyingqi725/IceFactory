using UnityEngine;

namespace IceFactory.Thermal.Core
{
    public readonly struct TemperaturePayload
    {
        public TemperaturePayload(
            TemperatureType type,
            float amount,
            GameObject sourceObject,
            Vector3 sourcePosition)
        {
            Type = type;
            Amount = amount;
            SourceObject = sourceObject;
            SourcePosition = sourcePosition;
        }

        public TemperatureType Type { get; }
        public float Amount { get; }
        public GameObject SourceObject { get; }
        public Vector3 SourcePosition { get; }
    }
}
