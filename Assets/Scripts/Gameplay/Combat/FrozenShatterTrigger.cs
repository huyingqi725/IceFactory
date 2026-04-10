using IceFactory.Gameplay.Enemy;
using UnityEngine;

namespace IceFactory.Gameplay.Combat
{
    [DisallowMultipleComponent]
    public sealed class FrozenShatterTrigger : MonoBehaviour
    {
        [SerializeField] private ShatterSourceType sourceType = ShatterSourceType.HeavyAttack;

        private void OnTriggerEnter(Collider other)
        {
            if (other == null)
            {
                return;
            }

            var shatterable = other.GetComponentInParent<IShatterableWhenFrozen>();
            if (shatterable == null)
            {
                return;
            }

            shatterable.TryShatter(sourceType);
        }
    }
}
