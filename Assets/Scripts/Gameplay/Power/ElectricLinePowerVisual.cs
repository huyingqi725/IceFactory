using System.Collections;
using UnityEngine;

namespace IceFactory.Gameplay.Power
{
    [DisallowMultipleComponent]
    public sealed class ElectricLinePowerVisual : MonoBehaviour
    {
        [Header("Renderers")]
        [SerializeField] private Renderer[] targetRenderers;
        [SerializeField] private bool includeChildrenIfEmpty = true;

        [Header("Color")]
        [SerializeField] private Color unpoweredColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color poweredColor = new Color(1f, 0.45f, 0.05f, 1f);
        [SerializeField] private float colorTransitionDuration = 0.35f;

        [Header("Emission")]
        [SerializeField] private bool controlEmission = true;
        [SerializeField] private Color unpoweredEmission = Color.black;
        [SerializeField] private Color poweredEmission = new Color(1.6f, 0.6f, 0.1f, 1f);

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private MaterialPropertyBlock _propertyBlock;
        private Coroutine _transitionCoroutine;
        private bool _isPowered;
        private float _blend;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            CacheRenderers();
            ApplyVisualImmediate(_isPowered ? 1f : 0f);
        }

        public void SetPowered(bool powered)
        {
            if (targetRenderers == null || targetRenderers.Length == 0)
            {
                CacheRenderers();
            }

            _isPowered = powered;
            var targetBlend = _isPowered ? 1f : 0f;

            if (colorTransitionDuration <= 0f)
            {
                ApplyVisualImmediate(targetBlend);
                return;
            }

            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
            }

            _transitionCoroutine = StartCoroutine(TransitionRoutine(targetBlend));
        }

        private IEnumerator TransitionRoutine(float targetBlend)
        {
            var from = _blend;
            var elapsed = 0f;
            while (elapsed < colorTransitionDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / colorTransitionDuration);
                var eased = Mathf.SmoothStep(0f, 1f, t);
                var value = Mathf.Lerp(from, targetBlend, eased);
                ApplyVisualImmediate(value);
                yield return null;
            }

            ApplyVisualImmediate(targetBlend);
            _transitionCoroutine = null;
        }

        private void ApplyVisualImmediate(float blend)
        {
            if (_propertyBlock == null)
            {
                _propertyBlock = new MaterialPropertyBlock();
            }

            _blend = Mathf.Clamp01(blend);
            var baseColor = Color.Lerp(unpoweredColor, poweredColor, _blend);
            var emission = Color.Lerp(unpoweredEmission, poweredEmission, _blend);

            for (var i = 0; i < targetRenderers.Length; i++)
            {
                var rend = targetRenderers[i];
                if (rend == null)
                {
                    continue;
                }

                rend.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(BaseColorId, baseColor);
                _propertyBlock.SetColor(ColorId, baseColor);
                if (controlEmission)
                {
                    _propertyBlock.SetColor(EmissionColorId, emission);
                }

                rend.SetPropertyBlock(_propertyBlock);
            }
        }

        private void CacheRenderers()
        {
            if (targetRenderers != null && targetRenderers.Length > 0)
            {
                return;
            }

            if (!includeChildrenIfEmpty)
            {
                targetRenderers = new[] { GetComponent<Renderer>() };
                if (targetRenderers[0] != null)
                {
                    return;
                }
            }

            targetRenderers = GetComponentsInChildren<Renderer>(true);
        }
    }
}
