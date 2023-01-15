using HMUI;
using System;
using UnityEngine;

namespace OldschoolBeatSaber.Behaviours.UI
{
#pragma warning disable CS0436
    public class DifficultyTableView : MonoBehaviour, TableView.IDataSource
#pragma warning restore CS0436
    {
        [SerializeField] private TableView _tableView;
        [SerializeField] private DifficultyTableCell _cellPrefab;
        [SerializeField] private float _cellHeight = 8f;

		private IDifficultyBeatmap[] _difficultyLevels;

		public event Action<DifficultyTableView, int> DifficultyTableViewDidSelectRow;

		protected void Start()
		{
			_tableView.DidSelectRowEvent += HandleDidSelectRowEvent;
		}

		public float RowHeight()
		{
			return _cellHeight;
		}

		public int NumberOfRows()
		{
			if (_difficultyLevels == null)
				return 0;

			return _difficultyLevels.Length;
		}

		public TableCell CellForRow(int row)
		{
			var difficultyTableCell = _tableView.DequeueReusableCellForIdentifier("Cell") as DifficultyTableCell;
			if (difficultyTableCell == null)
			{
				difficultyTableCell = UnityEngine.Object.Instantiate(_cellPrefab);
				difficultyTableCell.reuseIdentifier = "Cell";
			}
			difficultyTableCell.DifficultyText = _difficultyLevels[row].difficulty.Name();
			return difficultyTableCell;
		}

		private void HandleDidSelectRowEvent(TableView tableView, int row)
		{
			if (DifficultyTableViewDidSelectRow != null)
				DifficultyTableViewDidSelectRow(this, row);
		}

		public void SetDifficultyLevels(IDifficultyBeatmap[] difficultyLevels)
		{
			_difficultyLevels = difficultyLevels;

			_tableView.dataSource = this;
		}

		public void SelectRow(int row)
		{
			_tableView.SelectRow(row);
		}

		public void ClearSelection()
		{
			_tableView.ClearSelection();
		}
	}
}
