using IceFactory.Thermal.Core;
using UnityEngine;
using UnityEngine.Events;

namespace IceFactory.Gameplay.Puzzle
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ThermalReceiver))]
    public sealed class IceObstacleThermalBehavior : MonoBehaviour
    {
        [Header("Melt Settings")]
        [SerializeField] [Min(0.01f)] private float heatRequiredToMelt = 5f;
        [SerializeField] private bool ignoreCold = true;

        [Header("Targets")]
        [SerializeField] private Collider[] collidersToDisable;
        [SerializeField] private Renderer[] renderersToDisable;
        [SerializeField] private GameObject[] objectsToDisable;
        [SerializeField] private Animator animator;
        [SerializeField] private string meltTriggerName = "Melt";

        [Header("Events")]
        [SerializeField] private UnityEvent onMelted = new UnityEvent();

        private ThermalReceiver _receiver;
        private float _currentHeat;
        private bool _isMelted;

        public bool IsMelted => _isMelted;

        private void Awake()
        {
            _receiver = GetComponent<ThermalReceiver>();
            _receiver.TemperatureReceived += HandleTemperatureReceived;
        }

        private void OnDestroy()
        {
            if (_receiver != null)
            {
                _receiver.TemperatureReceived -= HandleTemperatureReceived;
            }
        }

        private void HandleTemperatureReceived(TemperaturePayload payload)
        {
            if (_isMelted)
            {
                return;
            }

            if (payload.Type == TemperatureType.Cold && ignoreCold)
            {
                return;
            }

            if (payload.Type != TemperatureType.Heat)
            {
                return;
            }

            _currentHeat += payload.Amount;
            if (_currentHeat >= heatRequiredToMelt)
            {
                Melt();
            }
        }

        private void Melt()
        {
            _isMelted = true;

            if (animator != null && !string.IsNullOrEmpty(meltTriggerName))
            {
                animator.SetTrigger(meltTriggerName);
            }

            foreach (var col in collidersToDisable)
            {
                if (col != null)
                {
                    col.enabled = false;
                }
            }

            foreach (var rend in renderersToDisable)
            {
                if (rend != null)
                {
                    rend.enabled = false;
                }
            }

            foreach (var obj in objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }

            onMelted.Invoke();
        }

        public void ForceMelt()
        {
            if (_isMelted)
            {
                return;
            }

            Melt();
        }
    }
}
