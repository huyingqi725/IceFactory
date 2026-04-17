using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace IceFactory.Gameplay.Power
{
    [DisallowMultipleComponent]
    public sealed class PoweredDoorRotator : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform doorTransform;

        [Header("Rotation")]
        [SerializeField] private Vector3 openAxis = Vector3.up;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] [Min(0.01f)] private float rotateDuration = 1f;
        [SerializeField] private bool closeWhenUnpowered = false;

        [Header("Events")]
        [SerializeField] private UnityEvent onOpened = new UnityEvent();
        [SerializeField] private UnityEvent onClosed = new UnityEvent();

        private Quaternion _closedRotation;
        private Quaternion _openedRotation;
        private Coroutine _rotateCoroutine;
        private bool _isOpen;

        private void Awake()
        {
            if (doorTransform == null)
            {
                doorTransform = transform;
            }

            if (openAxis.sqrMagnitude < 0.0001f)
            {
                openAxis = Vector3.up;
            }

            _closedRotation = doorTransform.localRotation;
            _openedRotation = _closedRotation * Quaternion.AngleAxis(openAngle, openAxis.normalized);
        }

        public void SetPowered(bool powered)
        {
            if (powered)
            {
                SetOpen(true);
                return;
            }

            if (closeWhenUnpowered)
            {
                SetOpen(false);
            }
        }

        public void SetOpen(bool open)
        {
            if (_isOpen == open)
            {
                return;
            }

            _isOpen = open;
            if (_rotateCoroutine != null)
            {
                StopCoroutine(_rotateCoroutine);
            }

            _rotateCoroutine = StartCoroutine(RotateDoorRoutine(_isOpen ? _openedRotation : _closedRotation));
        }

        private IEnumerator RotateDoorRoutine(Quaternion target)
        {
            var from = doorTransform.localRotation;
            var elapsed = 0f;
            while (elapsed < rotateDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / rotateDuration);
                var eased = Mathf.SmoothStep(0f, 1f, t);
                doorTransform.localRotation = Quaternion.Slerp(from, target, eased);
                yield return null;
            }

            doorTransform.localRotation = target;
            _rotateCoroutine = null;
            if (_isOpen)
            {
                onOpened.Invoke();
                yield break;
            }

            onClosed.Invoke();
        }
    }
}
