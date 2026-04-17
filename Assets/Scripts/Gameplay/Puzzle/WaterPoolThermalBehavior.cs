using IceFactory.Thermal.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace IceFactory.Gameplay.Puzzle
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ThermalReceiver))]
    public sealed class WaterPoolThermalBehavior : MonoBehaviour
    {
        [Header("Freeze Settings")]
        [SerializeField] [Min(0.01f)] private float coldRequiredToFreeze = 3f;
        [SerializeField] [Min(0.1f)] private float meltDelaySeconds = 3f;
        [SerializeField] [Min(0.02f)] private float coldSignalTimeout = 0.35f;
        [SerializeField] private bool allowHeatToUnfreeze = false;
        [SerializeField] [Min(0.01f)] private float heatRequiredToUnfreeze = 3f;

        [Header("Surface Targets")]
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Collider targetCollider;
        [SerializeField] private Collider solidIceCollider;
        [SerializeField] private Material waterMaterial;
        [SerializeField] private Material iceMaterial;
        [SerializeField] private PhysicMaterial waterPhysicMaterial;
        [SerializeField] private PhysicMaterial icePhysicMaterial;

        [Header("Events")]
        [SerializeField] private UnityEvent onFrozen = new UnityEvent();
        [SerializeField] private UnityEvent onUnfrozen = new UnityEvent();

        private ThermalReceiver _receiver;
        private float _coldAccumulated;
        private float _heatAccumulated;
        private bool _isFrozen;
        private float _lastColdSignalTime = float.NegativeInfinity;
        private Coroutine _meltDelayCoroutine;

        private void Awake()
        {
            _receiver = GetComponent<ThermalReceiver>();
            _receiver.TemperatureReceived += HandleTemperatureReceived;
            ApplyVisualState();
        }

        private void OnDestroy()
        {
            if (_receiver != null)
            {
                _receiver.TemperatureReceived -= HandleTemperatureReceived;
            }

            StopMeltDelay();
        }

        private void Update()
        {
            if (!_isFrozen)
            {
                return;
            }

            if (_meltDelayCoroutine != null)
            {
                return;
            }

            if (Time.time - _lastColdSignalTime >= coldSignalTimeout)
            {
                _meltDelayCoroutine = StartCoroutine(MeltDelayRoutine());
            }
        }

        private void HandleTemperatureReceived(TemperaturePayload payload)
        {
            if (payload.Type == TemperatureType.Cold)
            {
                _lastColdSignalTime = Time.time;
                StopMeltDelay();
            }

            if (!_isFrozen)
            {
                if (payload.Type != TemperatureType.Cold)
                {
                    return;
                }

                _coldAccumulated += payload.Amount;
                if (_coldAccumulated >= coldRequiredToFreeze)
                {
                    SetFrozen(true);
                }

                return;
            }

            if (!allowHeatToUnfreeze || payload.Type != TemperatureType.Heat)
            {
                return;
            }

            _heatAccumulated += payload.Amount;
            if (_heatAccumulated >= heatRequiredToUnfreeze)
            {
                SetFrozen(false);
            }
        }

        private void SetFrozen(bool frozen)
        {
            if (_isFrozen == frozen)
            {
                return;
            }

            _isFrozen = frozen;
            _coldAccumulated = 0f;
            _heatAccumulated = 0f;
            if (!_isFrozen)
            {
                StopMeltDelay();
            }

            ApplyVisualState();

            if (_isFrozen)
            {
                onFrozen.Invoke();
                return;
            }

            onUnfrozen.Invoke();
        }

        private void ApplyVisualState()
        {
            if (targetRenderer != null)
            {
                var nextMat = _isFrozen ? iceMaterial : waterMaterial;
                if (nextMat != null)
                {
                    targetRenderer.material = nextMat;
                }
            }

            if (targetCollider != null)
            {
                targetCollider.sharedMaterial = _isFrozen ? icePhysicMaterial : waterPhysicMaterial;
            }

            if (solidIceCollider != null)
            {
                solidIceCollider.enabled = _isFrozen;
            }
        }

        private IEnumerator MeltDelayRoutine()
        {
            yield return new WaitForSeconds(meltDelaySeconds);

            _meltDelayCoroutine = null;
            if (!_isFrozen)
            {
                yield break;
            }

            if (Time.time - _lastColdSignalTime < coldSignalTimeout)
            {
                yield break;
            }

            SetFrozen(false);
        }

        private void StopMeltDelay()
        {
            if (_meltDelayCoroutine == null)
            {
                return;
            }

            StopCoroutine(_meltDelayCoroutine);
            _meltDelayCoroutine = null;
        }
    }
}
