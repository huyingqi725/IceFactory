using System.Collections;
using System.Collections.Generic;
using IceFactory.Gameplay.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace IceFactory.Gameplay.Puzzle
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class ValveLinkageConsole : MonoBehaviour, IPlayerInteractable
    {
        [Header("Linked Valves")]
        [SerializeField] private List<RotatableValveController> linkedValves = new List<RotatableValveController>();

        [Header("Activation")]
        [SerializeField] private bool triggerByCollider = false;
        [SerializeField] private string triggerTag = "PlayerAttack";
        [SerializeField] private bool allowTriggerSpam = true;

        [Header("Press Feedback")]
        [SerializeField] private Transform pressVisual;
        [SerializeField] [Min(0f)] private float pressDepth = 0.08f;
        [SerializeField] [Min(0.01f)] private float pressDownDuration = 0.08f;
        [SerializeField] [Min(0.01f)] private float reboundDuration = 0.12f;

        [Header("Events")]
        [SerializeField] private UnityEvent onConsoleTriggered = new UnityEvent();
        [SerializeField] private UnityEvent onPressStarted = new UnityEvent();
        [SerializeField] private UnityEvent onPressFinished = new UnityEvent();

        private Coroutine _pressCoroutine;
        private Vector3 _visualStartLocalPos;
        private bool _isTriggering;

        private void Awake()
        {
            if (pressVisual == null)
            {
                pressVisual = transform;
            }

            _visualStartLocalPos = pressVisual.localPosition;
        }

        public void Interact()
        {
            TriggerConsole();
        }

        public bool CanInteract(PlayerInteractor interactor)
        {
            return true;
        }

        void IPlayerInteractable.Interact(PlayerInteractor interactor)
        {
            TriggerConsole();
        }

        public void TriggerConsole()
        {
            if (_isTriggering && !allowTriggerSpam)
            {
                return;
            }

            _isTriggering = true;

            for (var i = 0; i < linkedValves.Count; i++)
            {
                var valve = linkedValves[i];
                if (valve == null)
                {
                    continue;
                }

                valve.TriggerRotation();
            }

            onConsoleTriggered.Invoke();
            PlayPressFeedback();
            _isTriggering = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!triggerByCollider || other == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(triggerTag) && !other.CompareTag(triggerTag))
            {
                return;
            }

            TriggerConsole();
        }

        private void PlayPressFeedback()
        {
            if (pressVisual == null)
            {
                return;
            }

            if (_pressCoroutine != null)
            {
                StopCoroutine(_pressCoroutine);
            }

            _pressCoroutine = StartCoroutine(PressFeedbackRoutine());
        }

        private IEnumerator PressFeedbackRoutine()
        {
            onPressStarted.Invoke();

            var downPos = _visualStartLocalPos + Vector3.down * pressDepth;
            yield return MoveVisual(pressVisual.localPosition, downPos, pressDownDuration);
            yield return MoveVisual(pressVisual.localPosition, _visualStartLocalPos, reboundDuration);

            pressVisual.localPosition = _visualStartLocalPos;
            _pressCoroutine = null;
            onPressFinished.Invoke();
        }

        private IEnumerator MoveVisual(Vector3 from, Vector3 to, float duration)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var eased = Mathf.SmoothStep(0f, 1f, t);
                pressVisual.localPosition = Vector3.LerpUnclamped(from, to, eased);
                yield return null;
            }

            pressVisual.localPosition = to;
        }

        private void OnDisable()
        {
            if (_pressCoroutine != null)
            {
                StopCoroutine(_pressCoroutine);
                _pressCoroutine = null;
            }

            if (pressVisual != null)
            {
                pressVisual.localPosition = _visualStartLocalPos;
            }

            _isTriggering = false;
        }
    }
}
