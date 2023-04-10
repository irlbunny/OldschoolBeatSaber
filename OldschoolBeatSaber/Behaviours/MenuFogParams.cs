using IPA.Utilities;
using OldschoolBeatSaber.Interfaces;
using System.Linq;
using UnityEngine;
using Zenject;

namespace OldschoolBeatSaber.Behaviours
{
    public class MenuFogParams : MonoBehaviour, IOnActivate, IOnDeactivate
    {
        private static readonly FieldAccessor<BloomFogEnvironment, BloomFogEnvironmentParams>.Accessor _fogParamsAccessor
            = FieldAccessor<BloomFogEnvironment, BloomFogEnvironmentParams>.GetAccessor("_fogParams");

        [SerializeField] private float _attenuation = .1f;
        [SerializeField] private float _offset;
        [SerializeField] private float _heightFogStartY = -300f;
        [SerializeField] private float _heightFogHeight = 10f;
        [SerializeField]
        [Min(0f)] private float _brightness = .1f;
        [SerializeField]
        [Range(0f, 1f)] private float _autoExposureIntensity = 1f;

        private BloomFogEnvironmentParams _bloomFogEnvironmentParams;
        private BloomFogEnvironmentParams _customBloomFogEnvironmentParams;
        private BloomFogEnvironment _bloomFogEnvironment;

        public void OnActivate(DiContainer container)
        {
            // not good but whatever.
            if (_bloomFogEnvironment == null)
            {
                _bloomFogEnvironment = Resources.FindObjectsOfTypeAll<BloomFogEnvironment>()
                    .Where(x => x.gameObject.scene.name == "MainMenu")
                    .FirstOrDefault();
            }

            if (_customBloomFogEnvironmentParams == null)
            {
                _customBloomFogEnvironmentParams = ScriptableObject.CreateInstance<BloomFogEnvironmentParams>();
                _customBloomFogEnvironmentParams.attenuation = _attenuation;
                _customBloomFogEnvironmentParams.offset = _offset;
                _customBloomFogEnvironmentParams.heightFogStartY = _heightFogStartY;
                _customBloomFogEnvironmentParams.heightFogHeight = _heightFogHeight;
                //_customBloomFogEnvironmentParams.brightness = _brightness;
                //_customBloomFogEnvironmentParams.autoExposureIntensity = _autoExposureIntensity;
                _customBloomFogEnvironmentParams.autoExposureLimit = _autoExposureIntensity * 1000f;
            }

            _bloomFogEnvironment.enabled = false;
            _bloomFogEnvironmentParams = _fogParamsAccessor(ref _bloomFogEnvironment);
            _fogParamsAccessor(ref _bloomFogEnvironment) = _customBloomFogEnvironmentParams;
            _bloomFogEnvironment.enabled = true;
        }

        public void OnDeactivate()
        {
            _bloomFogEnvironment.enabled = false;
            _fogParamsAccessor(ref _bloomFogEnvironment) = _bloomFogEnvironmentParams;
            UnityEngine.Object.Destroy(_customBloomFogEnvironmentParams);
            _bloomFogEnvironment.enabled = true;
        }
    }
}
