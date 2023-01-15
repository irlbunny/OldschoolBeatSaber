using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HMUI
{
	[RequireComponent(typeof(ScrollRect))]
	public class TableView : MonoBehaviour
	{
		public enum SelectionType
		{
			None = 0,
			Single = 1,
			Multiple = 2
		}

		public interface IDataSource
		{
			float RowHeight();

			int NumberOfRows();

			TableCell CellForRow(int row);
		}

		[SerializeField]
		[NullAllowed]
		private Button _pageUpButton;

		[SerializeField]
		[NullAllowed]
		private Button _pageDownButton;

		[SerializeField]
		private SelectionType _selectionType = SelectionType.Single;

		private ScrollRect _scrollRect;

		private RectTransform _scrollRectTransform;

		private RectTransform _contentTransform;

		private IDataSource _dataSource;

		private int _numberOfRows;

		private float _rowHeight;

		private List<TableCell> _visibleCells;

		private Dictionary<string, List<TableCell>> _reusableCells;

		private HashSet<int> _selectedRows;

		private int _prevMinRow = -1;

		private int _prevMaxRow = -1;

		private float _targetVerticalNormalizedPosition;

		public SelectionType selectionType
		{
			get
			{
				return _selectionType;
			}
			set
			{
				_selectionType = value;
			}
		}

		public IDataSource dataSource
		{
			get
			{
				return _dataSource;
			}
			set
			{
				_dataSource = value;
				ReloadData();
			}
		}

		public event Action<TableView, int> DidSelectRowEvent;

		private void Awake()
		{
			if ((bool)_pageUpButton)
			{
				_pageUpButton.onClick.AddListener(PageScrollUp);
			}
			if ((bool)_pageDownButton)
			{
				_pageDownButton.onClick.AddListener(PageScrollDown);
			}
			_visibleCells = new List<TableCell>();
			_reusableCells = new Dictionary<string, List<TableCell>>();
			_selectedRows = new HashSet<int>();
			_scrollRect = GetComponent<ScrollRect>();
			_contentTransform = _scrollRect.content;
			if (!_contentTransform)
			{
				GameObject gameObject = new GameObject("Content", typeof(RectTransform));
				if (_scrollRect.viewport == null)
				{
					gameObject.transform.SetParent(base.transform, false);
				}
				else
				{
					gameObject.transform.SetParent(_scrollRect.viewport, false);
				}
				_contentTransform = gameObject.transform as RectTransform;
				_scrollRect.content = _contentTransform;
			}
			_scrollRect.horizontal = false;
			_scrollRect.onValueChanged.AddListener(ScrollRectOnValueChanged);
			_scrollRectTransform = _scrollRect.transform as RectTransform;
			_contentTransform.anchorMax = new Vector2(1f, 1f);
			_contentTransform.anchorMin = new Vector2(0f, 1f);
			_contentTransform.sizeDelta = new Vector2(0f, 0f);
			_contentTransform.pivot = new Vector2(0.5f, 1f);
			_contentTransform.anchoredPosition = new Vector2(0f, 0f);
			_targetVerticalNormalizedPosition = _scrollRect.verticalNormalizedPosition;
		}

		private void OnDestroy()
		{
			if ((bool)_pageUpButton)
			{
				_pageUpButton.onClick.RemoveListener(PageScrollUp);
			}
			if ((bool)_pageDownButton)
			{
				_pageDownButton.onClick.RemoveListener(PageScrollDown);
			}
		}

		private void RefreshScrollButtons()
		{
			float height = _scrollRectTransform.rect.height;
			float num = (float)_numberOfRows * _rowHeight - height;
			if ((bool)_pageDownButton)
			{
				_pageDownButton.interactable = !Mathf.Approximately(_targetVerticalNormalizedPosition, 0f);
				if (num < 0.01f)
				{
					_pageDownButton.gameObject.SetActive(false);
				}
				else
				{
					_pageDownButton.gameObject.SetActive(true);
				}
			}
			if ((bool)_pageUpButton)
			{
				_pageUpButton.interactable = !Mathf.Approximately(_targetVerticalNormalizedPosition, 1f);
				if (num < 0.01f)
				{
					_pageUpButton.gameObject.SetActive(false);
				}
				else
				{
					_pageUpButton.gameObject.SetActive(true);
				}
			}
		}

		private void RefreshContentSize()
		{
			_contentTransform.sizeDelta = new Vector2(0f, (float)_numberOfRows * _rowHeight);
		}

		private void RefreshCells(bool forced)
		{
			float height = _scrollRectTransform.rect.height;
			float num = (float)_numberOfRows * _rowHeight - height;
			if (num < 0f)
			{
				num = 0f;
			}
			float num2 = (1f - _scrollRect.verticalNormalizedPosition) * num;
			int num3 = Mathf.FloorToInt(num2 / _rowHeight);
			if (num3 < 0)
			{
				num3 = 0;
			}
			int num4 = Mathf.FloorToInt((num2 + height) / _rowHeight);
			if (num4 > _numberOfRows - 1)
			{
				num4 = _numberOfRows - 1;
			}
			if (num3 == _prevMinRow && num4 == _prevMaxRow && !forced)
			{
				return;
			}
			for (int num5 = _visibleCells.Count - 1; num5 >= 0; num5--)
			{
				TableCell tableCell = _visibleCells[num5];
				if (tableCell.row < num3 || tableCell.row > num4)
				{
					tableCell.gameObject.SetActive(false);
					_visibleCells.RemoveAt(num5);
					AddCellToReusableCells(tableCell);
				}
			}
			for (int i = num3; i <= num4; i++)
			{
				TableCell tableCell2 = null;
				for (int j = 0; j < _visibleCells.Count; j++)
				{
					if (_visibleCells[j].row == i)
					{
						tableCell2 = _visibleCells[j];
						break;
					}
				}
				if (!(tableCell2 != null) || forced)
				{
					if (tableCell2 == null)
					{
						tableCell2 = _dataSource.CellForRow(i);
						_visibleCells.Add(tableCell2);
					}
					tableCell2.gameObject.SetActive(true);
					tableCell2.TableViewSetup(this, i);
					tableCell2.ChangeSelection(_selectedRows.Contains(i), TableCell.TransitionType.Instant, false, true);
					tableCell2.ChangeHighlight(false, TableCell.TransitionType.Instant, true);
					if (tableCell2.transform.parent != _contentTransform)
					{
						tableCell2.transform.SetParent(_contentTransform, false);
					}
					LayoutCellForRow(tableCell2, i);
					if (_visibleCells.Count == num4 - num3 + 1 && !forced)
					{
						break;
					}
				}
			}
			_prevMinRow = num3;
			_prevMaxRow = num4;
		}

		private void LayoutCellForRow(TableCell cell, int row)
		{
			RectTransform rectTransform = cell.transform as RectTransform;
			rectTransform.anchorMax = new Vector2(1f, 1f);
			rectTransform.anchorMin = new Vector2(0f, 1f);
			rectTransform.pivot = new Vector2(0.5f, 1f);
			rectTransform.sizeDelta = new Vector2(0f, _rowHeight);
			rectTransform.anchoredPosition = new Vector2(0f, (float)(-row) * _rowHeight);
		}

		private void AddCellToReusableCells(TableCell cell)
		{
			List<TableCell> value = null;
			if (!_reusableCells.TryGetValue(cell.reuseIdentifier, out value))
			{
				value = new List<TableCell>();
				_reusableCells.Add(cell.reuseIdentifier, value);
			}
			value.Add(cell);
		}

		private void ScrollRectOnValueChanged(Vector2 pos)
		{
			RefreshCells(false);
		}

		public void CellSelectionStateDidChange(TableCell changedCell)
		{
			if (selectionType == SelectionType.None)
			{
				return;
			}
			if (selectionType != SelectionType.Multiple)
			{
				foreach (TableCell visibleCell in _visibleCells)
				{
					if (!(changedCell == visibleCell))
					{
						visibleCell.ChangeSelection(false, TableCell.TransitionType.Instant, false, false);
					}
				}
			}
			if (changedCell.selected)
			{
				if (selectionType != SelectionType.Multiple)
				{
					_selectedRows.Clear();
				}
				_selectedRows.Add(changedCell.row);
				if (this.DidSelectRowEvent != null)
				{
					this.DidSelectRowEvent(this, changedCell.row);
				}
			}
			else
			{
				_selectedRows.Remove(changedCell.row);
			}
		}

		public void ReloadData()
		{
			foreach (TableCell visibleCell in _visibleCells)
			{
				UnityEngine.Object.Destroy(visibleCell.gameObject);
			}
			foreach (KeyValuePair<string, List<TableCell>> reusableCell in _reusableCells)
			{
				List<TableCell> value = reusableCell.Value;
				foreach (TableCell item in value)
				{
					UnityEngine.Object.Destroy(item.gameObject);
				}
			}
			_visibleCells.Clear();
			_reusableCells.Clear();
			_selectedRows.Clear();
			_numberOfRows = _dataSource.NumberOfRows();
			_rowHeight = _dataSource.RowHeight();
			RefreshContentSize();
			RefreshCells(true);
			RefreshScrollButtons();
		}

		public TableCell DequeueReusableCellForIdentifier(string identifier)
		{
			TableCell result = null;
			List<TableCell> value = null;
			if (_reusableCells.TryGetValue(identifier, out value) && value.Count > 0)
			{
				result = value[0];
				value.RemoveAt(0);
			}
			return result;
		}

		public void SelectRow(int row)
		{
			foreach (TableCell visibleCell in _visibleCells)
			{
				visibleCell.ChangeSelection(false, TableCell.TransitionType.Instant, false, false);
			}
			_selectedRows.Clear();
			_selectedRows.Add(row);
			RefreshCells(true);
		}

		public void ClearSelection()
		{
			foreach (TableCell visibleCell in _visibleCells)
			{
				visibleCell.ChangeSelection(false, TableCell.TransitionType.Instant, false, false);
			}
			_selectedRows.Clear();
			RefreshCells(true);
		}

		public void ScrollToRow(int row, bool animated)
		{
			float height = _scrollRectTransform.rect.height;
			float num = (float)_numberOfRows * _rowHeight - height;
			int num2 = Mathf.FloorToInt(height / _rowHeight);
			float num3 = (float)(row + 1 + num2 / 2) * _rowHeight;
			float num4 = (_targetVerticalNormalizedPosition = Mathf.Max(0f, 1f - Mathf.Max(0f, num3 - height) / num));
			if (!animated)
			{
				_scrollRect.verticalNormalizedPosition = _targetVerticalNormalizedPosition;
			}
			RefreshScrollButtons();
		}

		private void Update()
		{
			float height = _scrollRectTransform.rect.height;
			float b = (float)_numberOfRows * _rowHeight - height;
			if (Mathf.Abs(_targetVerticalNormalizedPosition - _scrollRect.verticalNormalizedPosition) < 0.1f / Mathf.Max(1f, b))
			{
				if (_targetVerticalNormalizedPosition != _scrollRect.verticalNormalizedPosition)
				{
					_scrollRect.verticalNormalizedPosition = _targetVerticalNormalizedPosition;
				}
			}
			else
			{
				_scrollRect.verticalNormalizedPosition = Mathf.Lerp(_scrollRect.verticalNormalizedPosition, _targetVerticalNormalizedPosition, Time.deltaTime * 8f);
			}
		}

		private float GetScrollStep()
		{
			float height = _scrollRectTransform.rect.height;
			float num = (float)_numberOfRows * _rowHeight - height;
			int num2 = Mathf.CeilToInt(num / _rowHeight);
			return 1f / (float)num2;
		}

		private float GetNumberOfVisibleRows()
		{
			float height = _scrollRectTransform.rect.height;
			return Mathf.CeilToInt(height / _rowHeight);
		}

		public void PageScrollUp()
		{
			float scrollStep = GetScrollStep();
			_targetVerticalNormalizedPosition = (float)Mathf.RoundToInt(_targetVerticalNormalizedPosition / scrollStep + Mathf.Max(1f, GetNumberOfVisibleRows() - 1f)) * scrollStep;
			if (_targetVerticalNormalizedPosition > 1f)
			{
				_targetVerticalNormalizedPosition = 1f;
			}
			RefreshScrollButtons();
		}

		public void PageScrollDown()
		{
			float scrollStep = GetScrollStep();
			_targetVerticalNormalizedPosition = (float)Mathf.RoundToInt(_targetVerticalNormalizedPosition / scrollStep - Mathf.Max(1f, GetNumberOfVisibleRows() - 1f)) * scrollStep;
			if (_targetVerticalNormalizedPosition < 0f)
			{
				_targetVerticalNormalizedPosition = 0f;
			}
			RefreshScrollButtons();
		}
	}
}
