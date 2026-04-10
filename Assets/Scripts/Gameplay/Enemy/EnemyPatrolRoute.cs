using UnityEngine;

namespace IceFactory.Gameplay.Enemy
{
    [DisallowMultipleComponent]
    public sealed class EnemyPatrolRoute : MonoBehaviour
    {
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;

        public bool IsValid => pointA != null && pointB != null;
        public Transform PointA => pointA;
        public Transform PointB => pointB;
    }
}
