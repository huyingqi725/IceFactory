using UnityEngine;

namespace IceFactory.Gameplay.Power
{
    [DisallowMultipleComponent]
    public sealed class PoweredContinuousRotator : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform rotateTarget;

        [Header("Spin")]
        [SerializeField] private Vector3 localAxis = Vector3.forward;
        [SerializeField] [Min(0f)] private float degreesPerSecond = 120f;
        [SerializeField] private bool useWorldSpace;

        private bool _isPowered;

        private void Awake()
        {
            if (rotateTarget == null)
            {
                rotateTarget = transform;
            }
        }

        private void Update()
        {
            if (!_isPowered || rotateTarget == null || degreesPerSecond <= 0f)
            {
                return;
            }

            var delta = degreesPerSecond * Time.deltaTime;
            if (useWorldSpace)
            {
                rotateTarget.Rotate(localAxis, delta, Space.World);
                return;
            }

            rotateTarget.Rotate(localAxis, delta, Space.Self);
        }

        public void SetPowered(bool powered)
        {
            _isPowered = powered;
        }
    }
}
