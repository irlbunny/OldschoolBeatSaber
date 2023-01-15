using UnityEngine;
using VRUI;

namespace OldschoolBeatSaber.Behaviours.UI
{
    public class HowToPlayViewController : VRUIViewController
    {
        private bool _firstTimeActivated = true;

        protected override void DidActivate()
        {
            base.DidActivate();

            if (_firstTimeActivated)
            {
                _firstTimeActivated = false;

                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }
        }
    }
}
