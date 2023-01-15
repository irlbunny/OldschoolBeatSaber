using OldschoolBeatSaber.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HMUI
{
	[RequireComponent(typeof(RectTransform))]
	public class TableCell : UIBehaviour, IPointerClickHandler, ISubmitHandler, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
	{
		public enum TransitionType
		{
			Instant = 0,
			Animated = 1
		}

		private Signal _tableCellWasPressedSignal;

		private string _reuseIdentifier;

		private int _row;

		private TableView _tableView;

		private bool _selected;

		private bool _highlighted;

		public string reuseIdentifier
		{
			get
			{
				return _reuseIdentifier;
			}
			set
			{
				_reuseIdentifier = value;
			}
		}

		public int row
		{
			get
			{
				return _row;
			}
		}

		public bool selected
		{
			get
			{
				return _selected;
			}
			set
			{
				ChangeSelection(value, TransitionType.Animated, true, false);
			}
		}

		public bool highlighted
		{
			get
			{
				return _highlighted;
			}
		}

		protected override void Start()
		{
			base.Start();
			if (_tableCellWasPressedSignal == null)
				_tableCellWasPressedSignal = ResourceUtility.Find<Signal>("TableCellWasPressed");
			SelectionDidChange(TransitionType.Instant);
			HighlightDidChange(TransitionType.Instant);
		}

		public void TableViewSetup(TableView tableView, int row)
		{
			_tableView = tableView;
			_row = row;
		}

		public void ChangeSelection(bool value, TransitionType transitionType, bool callbackTable, bool ignoreCurrentValue)
		{
			if (ignoreCurrentValue || _selected != value)
			{
				_selected = value;
				SelectionDidChange(transitionType);
				if (callbackTable)
				{
					_tableView.CellSelectionStateDidChange(this);
				}
			}
		}

		public void ChangeHighlight(bool value, TransitionType transitionType, bool ignoreCurrentValue)
		{
			if (ignoreCurrentValue || _highlighted != value)
			{
				_highlighted = value;
				HighlightDidChange(transitionType);
			}
		}

		private void InternalToggle()
		{
			if (IsActive() && _tableView.selectionType != 0)
			{
				if (selected && _tableView.selectionType == TableView.SelectionType.Multiple)
				{
					selected = !selected;
				}
				else if (!selected)
				{
					selected = true;
				}
			}
		}

		protected virtual void SelectionDidChange(TransitionType transitionType)
		{
		}

		protected virtual void HighlightDidChange(TransitionType transitionType)
		{
		}

		public virtual void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				InternalToggle();
				if (_tableCellWasPressedSignal != null)
				{
					_tableCellWasPressedSignal.Raise();
				}
			}
		}

		public virtual void OnSubmit(BaseEventData eventData)
		{
			InternalToggle();
			if (_tableCellWasPressedSignal != null)
			{
				_tableCellWasPressedSignal.Raise();
			}
		}

		public virtual void OnPointerEnter(PointerEventData eventData)
		{
			ChangeHighlight(true, TransitionType.Animated, false);
		}

		public virtual void OnPointerExit(PointerEventData eventData)
		{
			ChangeHighlight(false, TransitionType.Animated, false);
		}
	}
}
