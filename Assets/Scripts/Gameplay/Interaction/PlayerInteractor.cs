using StarterAssets;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IceFactory.Gameplay.Interaction
{
    [DisallowMultipleComponent]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private Transform interactionOrigin;
        [SerializeField] [Min(0.1f)] private float interactionRadius = 2.2f;
        [SerializeField] private LayerMask interactionLayers = ~0;
        [SerializeField] [Min(1)] private int maxHits = 24;

        [Header("Input")]
        [SerializeField] private KeyCode fallbackInteractKey = KeyCode.E;
        [SerializeField] private bool useKeyboardFallback = true;

        [Header("References")]
        [SerializeField] private StarterAssetsInputs starterInputs;
        [SerializeField] private PlayerCarryController carryController;

        private Collider[] _results;

        public PlayerCarryController CarryController => carryController;

        private void Awake()
        {
            if (interactionOrigin == null)
            {
                interactionOrigin = transform;
            }

            if (starterInputs == null)
            {
                starterInputs = GetComponent<StarterAssetsInputs>();
            }

            if (carryController == null)
            {
                carryController = GetComponent<PlayerCarryController>();
            }

            _results = new Collider[Mathf.Max(1, maxHits)];
        }

        private void Update()
        {
            if (!ConsumeInteractPressed())
            {
                return;
            }

            TryInteractNearest();
        }

        private bool ConsumeInteractPressed()
        {
            var pressed = false;
            if (starterInputs != null && starterInputs.interact)
            {
                pressed = true;
                starterInputs.ConsumeInteractInput();
            }

            if (pressed)
            {
                return true;
            }

#if ENABLE_INPUT_SYSTEM
            if (useKeyboardFallback && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                return true;
            }
#else
            if (useKeyboardFallback && Input.GetKeyDown(fallbackInteractKey))
            {
                return true;
            }
#endif
            return false;
        }

        private void TryInteractNearest()
        {
            var best = FindNearestInteractable();
            best?.Interact(this);
        }

        private IPlayerInteractable FindNearestInteractable()
        {
            var count = Physics.OverlapSphereNonAlloc(
                interactionOrigin.position,
                interactionRadius,
                _results,
                interactionLayers,
                QueryTriggerInteraction.Collide);

            IPlayerInteractable best = null;
            var bestSqr = float.MaxValue;

            for (var i = 0; i < count; i++)
            {
                var hit = _results[i];
                if (hit == null)
                {
                    continue;
                }

                if (hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                var interactable = hit.GetComponentInParent<IPlayerInteractable>();
                if (interactable == null || !interactable.CanInteract(this))
                {
                    continue;
                }

                var pos = hit.bounds.center;
                var sqr = (pos - interactionOrigin.position).sqrMagnitude;
                if (sqr >= bestSqr)
                {
                    continue;
                }

                best = interactable;
                bestSqr = sqr;
            }

            return best;
        }

        private void OnDrawGizmosSelected()
        {
            var origin = interactionOrigin != null ? interactionOrigin.position : transform.position;
            Gizmos.color = new Color(0.4f, 1f, 0.8f, 0.8f);
            Gizmos.DrawWireSphere(origin, interactionRadius);
        }
    }
}
