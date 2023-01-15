using HMUI;
using System;
using System.Threading;
using UnityEngine;

namespace OldschoolBeatSaber.Behaviours.UI
{
#pragma warning disable CS0436
    public class SongListTableView : MonoBehaviour, TableView.IDataSource
#pragma warning restore CS0436
    {
        [SerializeField] private TableView _tableView;
        [SerializeField] private SongListTableCell _cellPrefab;
        [SerializeField] private float _cellHeight = 12f;

        private IPreviewBeatmapLevel[] _levels;

        public event Action<SongListTableView, int> SongListTableViewDidSelectRow;

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
            if (_levels == null)
                return 0;

            return _levels.Length;
        }

        public TableCell CellForRow(int row)
        {
            var songListTableCell = _tableView.DequeueReusableCellForIdentifier("Cell") as SongListTableCell;
            if (songListTableCell == null)
            {
                songListTableCell = UnityEngine.Object.Instantiate(_cellPrefab);
                songListTableCell.reuseIdentifier = "Cell";
            }
            songListTableCell.SetDataFromLevelAsync(_levels[row]);
            return songListTableCell;
        }

        private void HandleDidSelectRowEvent(TableView tableView, int row)
        {
            if (SongListTableViewDidSelectRow != null)
                SongListTableViewDidSelectRow(this, row);
        }

        public void SetLevels(IPreviewBeatmapLevel[] levels)
        {
            _levels = levels;

            _tableView.dataSource = this;
            _tableView.ScrollToRow(0, false);
        }

        public void SelectRow(int row)
        {
            _tableView.SelectRow(row);
            _tableView.ScrollToRow(row, false);
        }

        public void ClearSelection()
        {
            _tableView.ClearSelection();
            _tableView.ScrollToRow(0, false);
        }
    }
}
