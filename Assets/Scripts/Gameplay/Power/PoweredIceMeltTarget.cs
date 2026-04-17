using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace IceFactory.Gameplay.Power
{
    [DisallowMultipleComponent]
    public sealed class PoweredIceMeltTarget : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private Renderer[] targetRenderers;
        [SerializeField] private bool includeChildrenIfEmpty = true;
        [SerializeField] private bool fadeAlpha = true;
        [SerializeField] private bool scaleDown = true;
        [SerializeField] [Min(0.01f)] private float meltDuration = 2f;

        [Header("Disable Targets")]
        [SerializeField] private Collider[] collidersToDisable;
        [SerializeField] private GameObject[] objectsToDisable;
        [SerializeField] private bool disableThisGameObjectAtEnd = false;

        [Header("Events")]
        [SerializeField] private UnityEvent onMeltStarted = new UnityEvent();
        [SerializeField] private UnityEvent onMeltFinished = new UnityEvent();

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private MaterialPropertyBlock _block;
        private Vector3 _initialLocalScale;
        private bool _isMelted;
        private Coroutine _meltRoutine;

        public bool IsMelted => _isMelted;

        private void Awake()
        {
            _block = new MaterialPropertyBlock();
            _initialLocalScale = transform.localScale;
            CacheRenderers();
        }

        public void BeginMelt()
        {
            if (_isMelted)
            {
                return;
            }

            if (_meltRoutine != null)
            {
                StopCoroutine(_meltRoutine);
            }

            _meltRoutine = StartCoroutine(MeltRoutine());
        }

        public void ForceMeltImmediate()
        {
            if (_isMelted)
            {
                return;
            }

            if (_meltRoutine != null)
            {
                StopCoroutine(_meltRoutine);
                _meltRoutine = null;
            }

            ApplyBlend(1f);
            FinalizeMelt();
        }

        private IEnumerator MeltRoutine()
        {
            onMeltStarted.Invoke();

            var elapsed = 0f;
            while (elapsed < meltDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / meltDuration);
                var eased = Mathf.SmoothStep(0f, 1f, t);
                ApplyBlend(eased);
                yield return null;
            }

            ApplyBlend(1f);
            _meltRoutine = null;
            FinalizeMelt();
        }

        private void ApplyBlend(float blend)
        {
            if (scaleDown)
            {
                transform.localScale = Vector3.LerpUnclamped(_initialLocalScale, Vector3.zero, blend);
            }

            if (!fadeAlpha)
            {
                return;
            }

            var alpha = 1f - blend;
            if (_block == null)
            {
                _block = new MaterialPropertyBlock();
            }

            for (var i = 0; i < targetRenderers.Length; i++)
            {
                var rend = targetRenderers[i];
                if (rend == null)
                {
                    continue;
                }

                rend.GetPropertyBlock(_block);
                var baseColor = _block.GetColor(BaseColorId);
                if (baseColor == default)
                {
                    baseColor = Color.white;
                }

                baseColor.a = alpha;
                _block.SetColor(BaseColorId, baseColor);
                _block.SetColor(ColorId, baseColor);
                rend.SetPropertyBlock(_block);
            }
        }

        private void FinalizeMelt()
        {
            _isMelted = true;

            for (var i = 0; i < collidersToDisable.Length; i++)
            {
                var col = collidersToDisable[i];
                if (col != null)
                {
                    col.enabled = false;
                }
            }

            for (var i = 0; i < objectsToDisable.Length; i++)
            {
                var obj = objectsToDisable[i];
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }

            if (disableThisGameObjectAtEnd)
            {
                gameObject.SetActive(false);
            }

            onMeltFinished.Invoke();
        }

        private void CacheRenderers()
        {
            if (targetRenderers != null && targetRenderers.Length > 0)
            {
                return;
            }

            if (includeChildrenIfEmpty)
            {
                targetRenderers = GetComponentsInChildren<Renderer>(true);
                return;
            }

            var rend = GetComponent<Renderer>();
            targetRenderers = rend != null ? new[] { rend } : new Renderer[0];
        }
    }
}
