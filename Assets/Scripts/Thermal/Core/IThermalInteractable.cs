namespace IceFactory.Thermal.Core
{
    public interface IThermalInteractable
    {
        bool CanReceiveTemperature(TemperatureType type);
        void OnReceiveTemperature(TemperaturePayload payload);
    }
}
