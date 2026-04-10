using IceFactory.Gameplay.Interaction;
using UnityEngine;

namespace IceFactory.Gameplay.Puzzle
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HeatBatterySource))]
    public sealed class HeatBatteryInteractable : MonoBehaviour, IPlayerInteractable
    {
        private HeatBatterySource _battery;

        private void Awake()
        {
            _battery = GetComponent<HeatBatterySource>();
        }

        public bool CanInteract(PlayerInteractor interactor)
        {
            if (interactor == null || interactor.CarryController == null)
            {
                return false;
            }

            var held = interactor.CarryController.HeldBattery;
            return held == null || held == _battery;
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (interactor == null || interactor.CarryController == null)
            {
                return;
            }

            var carry = interactor.CarryController;
            var held = carry.HeldBattery;
            if (held == _battery)
            {
                carry.DropHeldBattery();
                return;
            }

            if (held == null)
            {
                carry.TryPickUp(_battery);
            }
        }
    }
}
