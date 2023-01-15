using OldschoolBeatSaber.Utilities;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HMUI
{
	public class Toggle : UIBehaviour, IPointerClickHandler, ISubmitHandler, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
	{
		[SerializeField]
		private string _onText;

		[SerializeField]
		private string _offText;

		[SerializeField]
		private Color _onTextColor = Color.black;

		[SerializeField]
		private Color _offTextColor = Color.white;

		[SerializeField]
		private Color _highlightColor = Color.gray;

		[SerializeField]
		private Color _selectedColor = Color.white;

		[Space]
		[SerializeField]
		private UnityEngine.UI.Image _offImage;

		[SerializeField]
		private UnityEngine.UI.Image _onImage;

		[SerializeField]
		private TextMeshProUGUI _offTextUI;

		[SerializeField]
		private TextMeshProUGUI _onTextUI;

        private Signal _uiButtonWasPressedSignal;

        private bool _isOn;

		private bool _isHighlighted;

		public bool isOn
		{
			get
			{
				return _isOn;
			}
			set
			{
				_isOn = value;
				RefreshColors();
			}
		}

		public event Action<Toggle, bool> didSwitchEvent;

		protected override void Start()
		{
			base.Start();
            if (_uiButtonWasPressedSignal == null)
                _uiButtonWasPressedSignal = ResourceUtility.Find<Signal>("UIButtonWasPressed");
            _offTextUI.text = _offText;
			_onTextUI.text = _onText;
			RefreshColors();
		}

		private void InternalToggle()
		{
			if (IsActive())
			{
				_isOn = !_isOn;
				RefreshColors();
				if (this.didSwitchEvent != null)
				{
					this.didSwitchEvent(this, _isOn);
				}
			}
		}

		private void RefreshColors()
		{
			_offTextUI.color = (_isOn ? _offTextColor : _onTextColor);
			_onTextUI.color = ((!_isOn) ? _offTextColor : _onTextColor);
			_onImage.color = (_isOn ? _selectedColor : ((!_isHighlighted) ? Color.clear : _highlightColor));
			_offImage.color = ((!_isOn) ? _selectedColor : ((!_isHighlighted) ? Color.clear : _highlightColor));
		}

		public virtual void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				InternalToggle();
				if (_uiButtonWasPressedSignal != null)
                    _uiButtonWasPressedSignal.Raise();
			}
		}

		public virtual void OnSubmit(BaseEventData eventData)
		{
			InternalToggle();
			if (_uiButtonWasPressedSignal != null)
                _uiButtonWasPressedSignal.Raise();
		}

		public virtual void OnPointerEnter(PointerEventData eventData)
		{
			_isHighlighted = true;
			RefreshColors();
		}

		public virtual void OnPointerExit(PointerEventData eventData)
		{
			_isHighlighted = false;
			RefreshColors();
		}
	}
}
