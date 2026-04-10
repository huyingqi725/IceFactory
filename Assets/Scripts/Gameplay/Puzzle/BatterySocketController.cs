using System;
using IceFactory.Gameplay.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace IceFactory.Gameplay.Puzzle
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class BatterySocketController : MonoBehaviour, IPlayerInteractable
    {
        [Header("Detection")]
        [SerializeField] private bool useTriggerDetection = false;
        [SerializeField] private string batteryTag = "HeatBattery";

        [Header("Snap")]
        [SerializeField] private Transform snapPoint;
        [SerializeField] private bool parentBatteryToSocket = true;
        [SerializeField] private bool lockBatteryPhysicsWhenInserted = true;

        [Header("Events")]
        [SerializeField] private UnityEvent onBatteryInserted = new UnityEvent();
        [SerializeField] private UnityEvent onBatteryRemoved = new UnityEvent();
        [SerializeField] private UnityEvent onSocketActivated = new UnityEvent();
        [SerializeField] private UnityEvent onSocketDeactivated = new UnityEvent();

        public event Action<HeatBatterySource> BatteryInserted;
        public event Action<HeatBatterySource> BatteryRemoved;
        public event Action<bool> SocketActivationChanged;

        public HeatBatterySource CurrentBattery => _currentBattery;
        public bool IsActivated => _currentBattery != null;

        private HeatBatterySource _currentBattery;
        private Collider _socketCollider;
        private Rigidbody _currentBatteryRigidbody;

        private void Awake()
        {
            _socketCollider = GetComponent<Collider>();
            if (useTriggerDetection)
            {
                _socketCollider.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!useTriggerDetection || _currentBattery != null || other == null)
            {
                return;
            }

            var battery = other.GetComponentInParent<HeatBatterySource>();
            if (battery == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(batteryTag) && !battery.CompareTag(batteryTag))
            {
                return;
            }

            InsertBattery(battery);
        }

        public bool CanInteract(PlayerInteractor interactor)
        {
            if (interactor == null || interactor.CarryController == null)
            {
                return false;
            }

            var held = interactor.CarryController.HeldBattery;
            if (_currentBattery == null)
            {
                return held != null;
            }

            return held == null;
        }

        void IPlayerInteractable.Interact(PlayerInteractor interactor)
        {
            if (interactor == null || interactor.CarryController == null)
            {
                return;
            }

            var carry = interactor.CarryController;
            if (_currentBattery == null)
            {
                var held = carry.DetachHeldBatteryForSocket();
                if (held != null)
                {
                    InsertBattery(held);
                }

                return;
            }

            var removed = EjectBattery();
            if (removed != null)
            {
                carry.TryPickUp(removed);
            }
        }

        public bool InsertBattery(HeatBatterySource battery)
        {
            if (battery == null || _currentBattery != null)
            {
                return false;
            }

            _currentBattery = battery;
            _currentBattery.SetEmissionEnabled(true);
            SnapBatteryTransform(_currentBattery.transform);
            CacheAndLockBatteryRigidbody(_currentBattery.transform);

            onBatteryInserted.Invoke();
            onSocketActivated.Invoke();
            BatteryInserted?.Invoke(_currentBattery);
            SocketActivationChanged?.Invoke(true);
            return true;
        }

        public HeatBatterySource EjectBattery()
        {
            if (_currentBattery == null)
            {
                return null;
            }

            var removed = _currentBattery;
            removed.SetEmissionEnabled(false);

            if (parentBatteryToSocket && removed.transform.parent == transform)
            {
                removed.transform.SetParent(null, true);
            }

            UnlockBatteryRigidbody();
            _currentBattery = null;

            onBatteryRemoved.Invoke();
            onSocketDeactivated.Invoke();
            BatteryRemoved?.Invoke(removed);
            SocketActivationChanged?.Invoke(false);
            return removed;
        }

        private void SnapBatteryTransform(Transform batteryTransform)
        {
            if (batteryTransform == null)
            {
                return;
            }

            var target = snapPoint != null ? snapPoint : transform;
            batteryTransform.SetPositionAndRotation(target.position, target.rotation);

            if (parentBatteryToSocket)
            {
                batteryTransform.SetParent(transform, true);
            }
        }

        private void CacheAndLockBatteryRigidbody(Transform batteryTransform)
        {
            if (!lockBatteryPhysicsWhenInserted || batteryTransform == null)
            {
                return;
            }

            _currentBatteryRigidbody = batteryTransform.GetComponent<Rigidbody>();
            if (_currentBatteryRigidbody == null)
            {
                _currentBatteryRigidbody = batteryTransform.GetComponentInParent<Rigidbody>();
            }

            if (_currentBatteryRigidbody != null)
            {
                _currentBatteryRigidbody.velocity = Vector3.zero;
                _currentBatteryRigidbody.angularVelocity = Vector3.zero;
                _currentBatteryRigidbody.isKinematic = true;
            }
        }

        private void UnlockBatteryRigidbody()
        {
            if (_currentBatteryRigidbody == null)
            {
                return;
            }

            _currentBatteryRigidbody.isKinematic = false;
            _currentBatteryRigidbody = null;
        }
    }
}
