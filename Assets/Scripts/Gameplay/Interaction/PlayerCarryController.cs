using IceFactory.Gameplay.Puzzle;
using UnityEngine;

namespace IceFactory.Gameplay.Interaction
{
    [DisallowMultipleComponent]
    public sealed class PlayerCarryController : MonoBehaviour
    {
        [Header("Carry Point")]
        [SerializeField] private Transform holdPoint;
        [SerializeField] private bool autoCreateHoldPointIfMissing = true;
        [SerializeField] private string autoHoldPointName = "BatteryHoldPoint";
        [SerializeField] private Vector3 holdLocalOffset = new Vector3(0.55f, 1.55f, 0.25f);
        [SerializeField] private Vector3 holdLocalEuler = new Vector3(0f, 20f, 0f);
        [SerializeField] [Min(0f)] private float followLerpSpeed = 14f;
        [SerializeField] [Min(0f)] private float rotateLerpSpeed = 16f;

        public HeatBatterySource HeldBattery => _heldBattery;

        private HeatBatterySource _heldBattery;
        private Rigidbody _heldRigidbody;
        private Collider[] _heldColliders;
        private Transform _lastHoldParent;

        private void Awake()
        {
            if (holdPoint == null)
            {
                if (autoCreateHoldPointIfMissing)
                {
                    var go = new GameObject(autoHoldPointName);
                    holdPoint = go.transform;
                    holdPoint.SetParent(transform, false);
                }
                else
                {
                    holdPoint = transform;
                }
            }

            holdPoint.localPosition = holdLocalOffset;
            holdPoint.localRotation = Quaternion.Euler(holdLocalEuler);
        }

        private void LateUpdate()
        {
            if (_heldBattery == null || holdPoint == null)
            {
                return;
            }

            var targetPos = holdPoint.position;
            var targetRot = holdPoint.rotation;
            var heldTransform = _heldBattery.transform;

            if (followLerpSpeed <= 0f)
            {
                heldTransform.position = targetPos;
            }
            else
            {
                heldTransform.position = Vector3.Lerp(
                    heldTransform.position,
                    targetPos,
                    1f - Mathf.Exp(-followLerpSpeed * Time.deltaTime));
            }

            if (rotateLerpSpeed <= 0f)
            {
                heldTransform.rotation = targetRot;
            }
            else
            {
                heldTransform.rotation = Quaternion.Slerp(
                    heldTransform.rotation,
                    targetRot,
                    1f - Mathf.Exp(-rotateLerpSpeed * Time.deltaTime));
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
            _lastHoldParent = batteryTransform.parent;
            batteryTransform.SetParent(null, true);
            batteryTransform.position = holdPoint.position;
            batteryTransform.rotation = holdPoint.rotation;

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

            _heldColliders = batteryTransform.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < _heldColliders.Length; i++)
            {
                var col = _heldColliders[i];
                if (col != null)
                {
                    col.enabled = false;
                }
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
            dropped.transform.SetParent(_lastHoldParent, true);

            if (_heldRigidbody != null)
            {
                _heldRigidbody.isKinematic = false;
            }

            if (_heldColliders != null)
            {
                for (var i = 0; i < _heldColliders.Length; i++)
                {
                    var col = _heldColliders[i];
                    if (col != null)
                    {
                        col.enabled = true;
                    }
                }
            }

            _heldBattery = null;
            _heldRigidbody = null;
            _heldColliders = null;
            _lastHoldParent = null;
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

            if (_heldColliders != null)
            {
                for (var i = 0; i < _heldColliders.Length; i++)
                {
                    var col = _heldColliders[i];
                    if (col != null)
                    {
                        col.enabled = true;
                    }
                }
            }

            _heldBattery = null;
            _heldRigidbody = null;
            _heldColliders = null;
            _lastHoldParent = null;
            return battery;
        }
    }
}
