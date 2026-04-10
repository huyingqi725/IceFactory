using IceFactory.Gameplay.Puzzle;
using UnityEngine;

namespace IceFactory.Gameplay.Interaction
{
    [DisallowMultipleComponent]
    public sealed class PlayerCarryController : MonoBehaviour
    {
        [Header("Carry Point")]
        [SerializeField] private Transform holdPoint;
        [SerializeField] private Vector3 holdLocalOffset = new Vector3(0f, 0f, 0.6f);

        public HeatBatterySource HeldBattery => _heldBattery;

        private HeatBatterySource _heldBattery;
        private Rigidbody _heldRigidbody;

        private void Awake()
        {
            if (holdPoint == null)
            {
                holdPoint = transform;
            }
        }

        public bool TryPickUp(HeatBatterySource battery)
        {
            if (battery == null || _heldBattery != null)
            {
                return false;
            }

            _heldBattery = battery;
            _heldBattery.SetEmissionEnabled(false);

            var batteryTransform = _heldBattery.transform;
            batteryTransform.SetParent(holdPoint, false);
            batteryTransform.localPosition = holdLocalOffset;
            batteryTransform.localRotation = Quaternion.identity;

            _heldRigidbody = batteryTransform.GetComponent<Rigidbody>();
            if (_heldRigidbody == null)
            {
                _heldRigidbody = batteryTransform.GetComponentInParent<Rigidbody>();
            }

            if (_heldRigidbody != null)
            {
                _heldRigidbody.velocity = Vector3.zero;
                _heldRigidbody.angularVelocity = Vector3.zero;
                _heldRigidbody.isKinematic = true;
            }

            return true;
        }

        public HeatBatterySource DropHeldBattery()
        {
            if (_heldBattery == null)
            {
                return null;
            }

            var dropped = _heldBattery;
            dropped.transform.SetParent(null, true);

            if (_heldRigidbody != null)
            {
                _heldRigidbody.isKinematic = false;
            }

            _heldBattery = null;
            _heldRigidbody = null;
            return dropped;
        }

        public HeatBatterySource DetachHeldBatteryForSocket()
        {
            if (_heldBattery == null)
            {
                return null;
            }

            var battery = _heldBattery;
            battery.transform.SetParent(null, true);
            _heldBattery = null;
            _heldRigidbody = null;
            return battery;
        }
    }
}
