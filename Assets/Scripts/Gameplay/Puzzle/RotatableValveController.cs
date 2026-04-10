using System.Collections;
using IceFactory.Gameplay.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace IceFactory.Gameplay.Puzzle
{
    [DisallowMultipleComponent]
    public sealed class RotatableValveController : MonoBehaviour, IPlayerInteractable
    {
        [Header("Rotation Step")]
        [SerializeField] [Min(1f)] private float rotateStepDegrees = 90f;
        [SerializeField] [Min(0.01f)] private float rotateDuration = 0.5f;
        [SerializeField] private bool clockwise = true;

        [Header("Queue Acceleration")]
        [SerializeField] [Min(0f)] private float extraSpeedPerQueuedStep = 0.35f;
        [SerializeField] [Min(1f)] private float maxSpeedMultiplier = 3f;
        [SerializeField] [Min(1f)] private float angularAcceleration = 1080f;

        [Header("Events")]
        [SerializeField] private UnityEvent onRotateStarted = new UnityEvent();
        [SerializeField] private UnityEvent onRotateFinished = new UnityEvent();

        private Coroutine _rotateCoroutine;
        private bool _isRotating;
        private Quaternion _baseLocalRotation;
        private float _currentAngle;
        private float _targetAngle;
        private float _currentSpeed;

        public bool IsRotating => _isRotating;

        private void Awake()
        {
            _baseLocalRotation = transform.localRotation;
            _currentAngle = 0f;
            _targetAngle = 0f;
        }

        public void Interact()
        {
            TriggerRotation();
        }

        public bool CanInteract(PlayerInteractor interactor)
        {
            return true;
        }

        void IPlayerInteractable.Interact(PlayerInteractor interactor)
        {
            TriggerRotation();
        }

        public void TriggerRotation()
        {
            var direction = clockwise ? -1f : 1f;
            _targetAngle += rotateStepDegrees * direction;

            if (_isRotating)
            {
                return;
            }

            _rotateCoroutine = StartCoroutine(RotationRoutine());
        }

        private IEnumerator RotationRoutine()
        {
            _isRotating = true;
            _currentSpeed = 0f;
            onRotateStarted.Invoke();

            var baseSpeed = rotateStepDegrees / rotateDuration;
            var stepAbs = Mathf.Max(1f, Mathf.Abs(rotateStepDegrees));

            while (Mathf.Abs(Mathf.DeltaAngle(_currentAngle, _targetAngle)) > 0.05f)
            {
                var remaining = Mathf.Abs(Mathf.DeltaAngle(_currentAngle, _targetAngle));
                var queuedSteps = Mathf.Max(1f, remaining / stepAbs);
                var desiredMultiplier = 1f + (queuedSteps - 1f) * extraSpeedPerQueuedStep;
                desiredMultiplier = Mathf.Clamp(desiredMultiplier, 1f, maxSpeedMultiplier);

                var desiredSpeed = baseSpeed * desiredMultiplier;
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, desiredSpeed, angularAcceleration * Time.deltaTime);
                _currentAngle = Mathf.MoveTowardsAngle(_currentAngle, _targetAngle, _currentSpeed * Time.deltaTime);
                transform.localRotation = _baseLocalRotation * Quaternion.Euler(0f, _currentAngle, 0f);
                yield return null;
            }

            _currentAngle = _targetAngle;
            transform.localRotation = _baseLocalRotation * Quaternion.Euler(0f, _currentAngle, 0f);
            _rotateCoroutine = null;
            _isRotating = false;
            _currentSpeed = 0f;
            onRotateFinished.Invoke();
        }

        private void OnDisable()
        {
            if (_rotateCoroutine == null)
            {
                return;
            }

            StopCoroutine(_rotateCoroutine);
            _rotateCoroutine = null;
            _isRotating = false;
            _currentSpeed = 0f;
        }
    }
}
