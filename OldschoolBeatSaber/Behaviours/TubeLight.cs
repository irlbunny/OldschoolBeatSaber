using IPA.Utilities;
using OldschoolBeatSaber.Interfaces;
using OldschoolBeatSaber.Utilities;
using System.Linq;
using UnityEngine;
using Zenject;

namespace OldschoolBeatSaber.Behaviours
{
    public class TubeLight : MonoBehaviour, IOnActivate
    {
        private static readonly FieldAccessor<ParametricBoxController, MeshRenderer>.Accessor _meshRendererAccessor =
            FieldAccessor<ParametricBoxController, MeshRenderer>.GetAccessor("_meshRenderer");

        private static readonly FieldAccessor<BloomPrePassLight, BloomPrePassLightTypeSO>.Accessor _lightTypeAccessor
            = FieldAccessor<BloomPrePassLight, BloomPrePassLightTypeSO>.GetAccessor("_lightType");

        private static readonly FieldAccessor<TubeBloomPrePassLight, BoolSO>.Accessor _mainEffectPostProcessEnabledAccessor
            = FieldAccessor<TubeBloomPrePassLight, BoolSO>.GetAccessor("_mainEffectPostProcessEnabled");
        private static readonly FieldAccessor<TubeBloomPrePassLight, float>.Accessor _centerAccessor
            = FieldAccessor<TubeBloomPrePassLight, float>.GetAccessor("_center");
        private static readonly FieldAccessor<TubeBloomPrePassLight, float>.Accessor _colorAlphaMultiplierAccessor
            = FieldAccessor<TubeBloomPrePassLight, float>.GetAccessor("_colorAlphaMultiplier");
        private static readonly FieldAccessor<TubeBloomPrePassLight, ParametricBoxController>.Accessor _parametricBoxControllerAccessor
            = FieldAccessor<TubeBloomPrePassLight, ParametricBoxController>.GetAccessor("_parametricBoxController");

        public enum BoxLightType
        {
            None,
            Opaque,
            Transparent
        }

        [SerializeField] private float _width = .5f;
        [SerializeField] private float _length = 1f;
        [SerializeField]
        [Range(0f, 1f)] private float _center = 1f;
        [SerializeField] private Color _color = Color.cyan;
        [SerializeField] private float _colorAlphaMultiplier = 1f;
        [SerializeField] private float _bloomFogIntensityMultiplier = 1f;
        [SerializeField] private BoxLightType _boxLightType;

        private BoolSO _mainEffectPostProcessEnabled;
        private TubeBloomPrePassLight _tubeBloomPrePassLight;

        public Color color
        {
            get
            {
                if (_tubeBloomPrePassLight != null)
                    return _tubeBloomPrePassLight.color;

                return _color; // We'll fallback, just in-case.
            }
            set
            {
                _color = value;

                if (_tubeBloomPrePassLight != null)
                    _tubeBloomPrePassLight.color = _color;
            }
        }

        [Inject]
        public void Construct()
        {
            _mainEffectPostProcessEnabled = ScriptableObject.CreateInstance<BoolSO>();
            _mainEffectPostProcessEnabled.value = true;
        }

        public void OnActivate(DiContainer container)
        {
            container.Inject(this);

            if (_tubeBloomPrePassLight == null)
            {
                var activeSelf = gameObject.activeSelf;
                gameObject.SetActive(false);

                _tubeBloomPrePassLight = gameObject.AddComponent<TubeBloomPrePassLight>();

                BloomPrePassLight bloomPrePassLight = _tubeBloomPrePassLight;
                _lightTypeAccessor(ref bloomPrePassLight) = BloomPrePassLight.bloomLightsDict.Keys.First(x => x.name == "AddBloomPrePassLightType");

                _mainEffectPostProcessEnabledAccessor(ref _tubeBloomPrePassLight) = _mainEffectPostProcessEnabled;
                _centerAccessor(ref _tubeBloomPrePassLight) = _center;
                _colorAlphaMultiplierAccessor(ref _tubeBloomPrePassLight) = _colorAlphaMultiplier;
                _tubeBloomPrePassLight.width = _width * 2f;
                _tubeBloomPrePassLight.length = _length;
                _tubeBloomPrePassLight.color = _color;
                _tubeBloomPrePassLight.bloomFogIntensityMultiplier = _bloomFogIntensityMultiplier;

                if (_boxLightType != BoxLightType.None)
                {
                    var boxLightGameObject = new GameObject("BoxLight");
                    boxLightGameObject.SetActive(false);
                    boxLightGameObject.layer = 13; // NeonLight
                    var boxLightTransform = boxLightGameObject.transform;
                    boxLightTransform.SetParent(transform);
                    boxLightTransform.localRotation = Quaternion.Euler(Vector3.zero);

                    var boxLightMeshFilter = boxLightGameObject.AddComponent<MeshFilter>();
                    boxLightMeshFilter.mesh = ResourceUtility.TubeMesh;
                    var boxLightMeshRenderer = boxLightGameObject.AddComponent<MeshRenderer>();
                    boxLightMeshRenderer.sharedMaterial = _boxLightType switch
                    {
                        BoxLightType.Opaque => ResourceUtility.EnvLightOpaqueMaterial,
                        BoxLightType.Transparent => ResourceUtility.EnvLightMaterial,
                        _ => null
                    };

                    var boxLightParametricBoxController = boxLightGameObject.AddComponent<ParametricBoxController>();
                    _meshRendererAccessor(ref boxLightParametricBoxController) = boxLightMeshRenderer;

                    boxLightGameObject.SetActive(true);

                    _parametricBoxControllerAccessor(ref _tubeBloomPrePassLight) = boxLightParametricBoxController;
                }

                gameObject.SetActive(activeSelf);
            }
        }
    }
}
