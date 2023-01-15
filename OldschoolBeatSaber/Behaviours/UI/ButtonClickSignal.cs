using IPA.Utilities;
using OldschoolBeatSaber.Interfaces;
using OldschoolBeatSaber.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace OldschoolBeatSaber.Behaviours.UI
{
    public class ButtonClickSignal : MonoBehaviour, IOnActivate
    {
        private static readonly FieldAccessor<SignalOnUIButtonClick, Signal>.Accessor _buttonClickedSignalAccessor =
            FieldAccessor<SignalOnUIButtonClick, Signal>.GetAccessor("_buttonClickedSignal");
        private static readonly FieldAccessor<SignalOnUIButtonClick, Button>.Accessor _buttonAccessor
            = FieldAccessor<SignalOnUIButtonClick, Button>.GetAccessor("_button");

        private Signal _uiButtonWasPressedSignal;
        private SignalOnUIButtonClick _signalOnUIButtonClick;

        public void OnActivate(DiContainer container)
        {
            container.Inject(this);

            if (_uiButtonWasPressedSignal == null)
                _uiButtonWasPressedSignal = ResourceUtility.Find<Signal>("UIButtonWasPressed");

            if (_signalOnUIButtonClick == null)
            {
                var activeSelf = gameObject.activeSelf;
                gameObject.SetActive(false);

                _signalOnUIButtonClick = gameObject.AddComponent<SignalOnUIButtonClick>();
                _buttonClickedSignalAccessor(ref _signalOnUIButtonClick) = _uiButtonWasPressedSignal;
                _buttonAccessor(ref _signalOnUIButtonClick) = gameObject.GetComponent<Button>();

                gameObject.SetActive(activeSelf);
            }
        }
    }
}
