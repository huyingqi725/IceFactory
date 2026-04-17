using System.Collections.Generic;
using System.Collections;
using IceFactory.Gameplay.Puzzle;
using IceFactory.Thermal.Core;
using UnityEngine;
using UnityEngine.Events;

namespace IceFactory.Gameplay.Power
{
    [DisallowMultipleComponent]
    public sealed class SocketPowerOrchestrator : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private BatterySocketController batterySocket;
        [SerializeField] private bool autoFindSocketOnSameObject = true;

        [Header("Power Targets")]
        [SerializeField] private List<ElectricLinePowerVisual> electricLines = new List<ElectricLinePowerVisual>();
        [SerializeField] private List<PoweredContinuousRotator> poweredGears = new List<PoweredContinuousRotator>();
        [SerializeField] private List<PoweredDoorRotator> poweredGates = new List<PoweredDoorRotator>();
        [SerializeField] private List<TemperatureSource> heatSources = new List<TemperatureSource>();

        [Header("Transmission Timeline")]
        [SerializeField] private bool delayDownstreamActionsByTransmission = true;
        [SerializeField] [Min(0f)] private float electricLineDuration = 1.5f;

        [Header("Ice Melt Options")]
        [SerializeField] private bool useDirectPowerMelt = true;
        [SerializeField] private List<PoweredIceMeltTarget> poweredIceTargets = new List<PoweredIceMeltTarget>();
        [SerializeField] private bool meltIceImmediatelyOnPower;
        [SerializeField] private List<IceObstacleThermalBehavior> iceObstacles = new List<IceObstacleThermalBehavior>();

        [Header("Events")]
        [SerializeField] private UnityEvent onPowered = new UnityEvent();
        [SerializeField] private UnityEvent onUnpowered = new UnityEvent();
        [SerializeField] private UnityEvent onTransmissionStarted = new UnityEvent();
        [SerializeField] private UnityEvent onTransmissionFinished = new UnityEvent();

        private bool _isPowered;
        private Coroutine _powerSequenceRoutine;

        private void Awake()
        {
            if (batterySocket == null && autoFindSocketOnSameObject)
            {
                batterySocket = GetComponent<BatterySocketController>();
            }
        }

        private void OnEnable()
        {
            if (batterySocket != null)
            {
                batterySocket.SocketActivationChanged += HandleSocketPowerChanged;
                ApplyPowerState(batterySocket.IsActivated);
            }
            else
            {
                ApplyPowerState(false);
            }
        }

        private void OnDisable()
        {
            if (batterySocket != null)
            {
                batterySocket.SocketActivationChanged -= HandleSocketPowerChanged;
            }

            StopPowerSequence();
        }

        private void HandleSocketPowerChanged(bool powered)
        {
            ApplyPowerState(powered);
        }

        private void ApplyPowerState(bool powered)
        {
            _isPowered = powered;

            SetElectricLinePowered(_isPowered);

            if (_isPowered)
            {
                if (delayDownstreamActionsByTransmission)
                {
                    RestartPowerSequence();
                    onPowered.Invoke();
                    return;
                }

                ApplyDownstreamPoweredState(true);
                onPowered.Invoke();
                return;
            }

            StopPowerSequence();
            ApplyDownstreamPoweredState(false);
            onUnpowered.Invoke();
        }

        private void SetElectricLinePowered(bool powered)
        {
            for (var i = 0; i < electricLines.Count; i++)
            {
                var line = electricLines[i];
                if (line != null)
                {
                    line.SetPowered(powered);
                }
            }
        }

        private void ApplyDownstreamPoweredState(bool powered)
        {
            for (var i = 0; i < poweredGears.Count; i++)
            {
                var gear = poweredGears[i];
                if (gear != null)
                {
                    gear.SetPowered(powered);
                }
            }

            for (var i = 0; i < poweredGates.Count; i++)
            {
                var gate = poweredGates[i];
                if (gate != null)
                {
                    gate.SetPowered(powered);
                }
            }

            for (var i = 0; i < heatSources.Count; i++)
            {
                var source = heatSources[i];
                if (source != null)
                {
                    source.enabled = powered;
                }
            }

            if (!powered)
            {
                return;
            }

            if (useDirectPowerMelt)
            {
                for (var i = 0; i < poweredIceTargets.Count; i++)
                {
                    var iceTarget = poweredIceTargets[i];
                    if (iceTarget == null)
                    {
                        continue;
                    }

                    if (meltIceImmediatelyOnPower)
                    {
                        iceTarget.ForceMeltImmediate();
                    }
                    else
                    {
                        iceTarget.BeginMelt();
                    }
                }
            }

            if (meltIceImmediatelyOnPower)
            {
                for (var i = 0; i < iceObstacles.Count; i++)
                {
                    var ice = iceObstacles[i];
                    if (ice != null)
                    {
                        ice.ForceMelt();
                    }
                }
            }
        }

        private void RestartPowerSequence()
        {
            StopPowerSequence();
            _powerSequenceRoutine = StartCoroutine(PowerTransmissionRoutine());
        }

        private void StopPowerSequence()
        {
            if (_powerSequenceRoutine == null)
            {
                return;
            }

            StopCoroutine(_powerSequenceRoutine);
            _powerSequenceRoutine = null;
        }

        private IEnumerator PowerTransmissionRoutine()
        {
            onTransmissionStarted.Invoke();
            if (electricLineDuration > 0f)
            {
                yield return new WaitForSeconds(electricLineDuration);
            }

            _powerSequenceRoutine = null;
            if (!_isPowered)
            {
                yield break;
            }

            ApplyDownstreamPoweredState(true);
            onTransmissionFinished.Invoke();
        }
    }
}
