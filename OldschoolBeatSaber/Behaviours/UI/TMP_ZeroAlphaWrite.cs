using OldschoolBeatSaber.Interfaces;
using UnityEngine;
using Zenject;

namespace OldschoolBeatSaber.Behaviours.UI
{
    public class TMP_ZeroAlphaWrite : MonoBehaviour, IOnActivate
    {
        [SerializeField] private Material[] _materials;

        private static Shader _zeroAlphaWriteShader;

        // This also used to be run on Awake instead of OnActivate, but this is probably better for performance. 
        public void OnActivate(DiContainer container)
        {
            if (_zeroAlphaWriteShader == null)
                _zeroAlphaWriteShader = Shader.Find("TextMeshPro/Mobile/Distance Field Zero Alpha Write");

            foreach (var material in _materials)
            {
                material.shader = _zeroAlphaWriteShader;
            }
        }
    }
}
