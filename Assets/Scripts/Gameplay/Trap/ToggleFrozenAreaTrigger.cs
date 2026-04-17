using System.Collections;
using IceFactory.Thermal.Core;
using UnityEngine;
using UnityEngine.Events;

namespace IceFactory.Gameplay.Trap
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class ToggleFrozenAreaTrigger : MonoBehaviour
    {
        [Header("Trigger Filter")]
        [SerializeField] private string triggerTag = "Player";

        [Header("Frozen Area")]
        [SerializeField] private TemperatureSource frozenAreaSource;
        [SerializeField] private bool forceSourceOffOnAwake = true;
        [SerializeField] private bool deactivateOnExit;
        [SerializeField] [Min(0f)] private float autoDeactivateDelay = 0f;

        [Header("Steam Visual")]
        [SerializeField] private GameObject steamVisual;
        [SerializeField] private bool forceSteamHiddenOnAwake = true;

        [Header("Events")]
        [SerializeField] private UnityEvent onActivated = new UnityEvent();
        [SerializeField] private UnityEvent onDeactivated = new UnityEvent();

        private int _insideCount;
        private Coroutine _autoDeactivateRoutine;

        private void Awake()
        {
            var triggerCollider = GetComponent<Collider>();
            triggerCollider.isTrigger = true;

            if (forceSourceOffOnAwake && frozenAreaSource != null)
            {
                frozenAreaSource.enabled = false;
            }

            if (forceSteamHiddenOnAwake && steamVisual != null)
            {
                steamVisual.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsValidTrigger(other))
            {
                return;
            }

            _insideCount++;
            if (_insideCount == 1)
            {
                ActivateFrozenArea();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsValidTrigger(other))
            {
                return;
            }

            _insideCount = Mathf.Max(0, _insideCount - 1);
            if (_insideCount == 0 && deactivateOnExit)
            {
                if (autoDeactivateDelay <= 0f)
                {
                    DeactivateFrozenArea();
                }
                else
                {
                    if (_autoDeactivateRoutine != null)
                    {
                        StopCoroutine(_autoDeactivateRoutine);
                    }

                    _autoDeactivateRoutine = StartCoroutine(AutoDeactivateRoutine());
                }
            }
        }

        private IEnumerator AutoDeactivateRoutine()
        {
            yield return new WaitForSeconds(autoDeactivateDelay);
            _autoDeactivateRoutine = null;

            if (_insideCount == 0)
            {
                DeactivateFrozenArea();
            }
        }

        private bool IsValidTrigger(Collider other)
        {
            if (other == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(triggerTag))
            {
                return true;
            }

            return other.CompareTag(triggerTag) || other.transform.root.CompareTag(triggerTag);
        }

        private void ActivateFrozenArea()
        {
            if (_autoDeactivateRoutine != null)
            {
                StopCoroutine(_autoDeactivateRoutine);
                _autoDeactivateRoutine = null;
            }

            if (frozenAreaSource != null)
            {
                frozenAreaSource.enabled = true;
            }

            if (steamVisual != null)
            {
                steamVisual.SetActive(true);
            }

            onActivated.Invoke();
        }

        private void DeactivateFrozenArea()
        {
            if (frozenAreaSource != null)
            {
                frozenAreaSource.enabled = false;
            }

            if (steamVisual != null)
            {
                steamVisual.SetActive(false);
            }

            onDeactivated.Invoke();
        }
    }
}
