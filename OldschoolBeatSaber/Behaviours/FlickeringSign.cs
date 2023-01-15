using System.Collections;
using TMPro;
using UnityEngine;

namespace OldschoolBeatSaber.Behaviours
{
    public class FlickeringSign : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _text;
        [SerializeField] private TubeLight _light;
        [SerializeField] private float _minOnDelay = .05f;
        [SerializeField] private float _maxOnDelay = .4f;
        [SerializeField] private float _minOffDelay = .05f;
        [SerializeField] private float _maxOffDelay = .4f;
        [SerializeField] private Color _onColor;
        [SerializeField] private Color _offColor;
        [SerializeField] private TMP_FontAsset _onFont;
        [SerializeField] private TMP_FontAsset _offFont;

        private void OnEnable()
        {
            StartCoroutine(FlickeringCoroutine());
        }

        private IEnumerator FlickeringCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(_minOnDelay, _maxOnDelay));
                SetOn(false);

                yield return new WaitForSeconds(UnityEngine.Random.Range(_minOffDelay, _maxOffDelay));
                SetOn(true);
            }
        }

        private void SetOn(bool on)
        {
            var color = !on ? _offColor : _onColor;
            _text.color = color;
            _text.font = !on ? _offFont : _onFont;
            _light.color = color;
        }
    }
}
