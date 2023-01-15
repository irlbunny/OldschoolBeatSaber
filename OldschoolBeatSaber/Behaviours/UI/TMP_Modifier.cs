using OldschoolBeatSaber.Interfaces;
using TMPro;
using UnityEngine;
using Zenject;

namespace OldschoolBeatSaber.Behaviours.UI
{
    /// <summary>
    /// Required due to a really dumb Unity serialization issue.
    /// </summary>
    public class TMP_Modifier : MonoBehaviour, IOnActivate
    {
        [SerializeField] private TextAlignmentOptions _alignment = TextAlignmentOptions.Center;

        // This used to be run on Awake instead of OnActivate, but this is probably better for performance. 
        public void OnActivate(DiContainer container)
        {
            var tmpText = GetComponent<TMP_Text>();
            if (tmpText == null)
                return;

            tmpText.alignment = _alignment;
        }
    }
}
