using System;
using UnityEngine;
using VRUI;

namespace OldschoolBeatSaber.Behaviours.UI
{
    public class SongListViewController : VRUIViewController
    {
        [SerializeField] private SongListTableView _songListTableView;

        private bool _firstTimeActivated = true;

        private IPreviewBeatmapLevel[] _beatmapLevels;

        public string LevelId { get; private set; }
        public event Action<SongListViewController> DidSelectSongEvent;

        protected override void DidActivate()
        {
            base.DidActivate();

            if (_firstTimeActivated)
            {
                _firstTimeActivated = false;

                _songListTableView.SongListTableViewDidSelectRow += HandleSongListTableViewDidSelectRow;
            }

            if (movingToParentController)
            {
                LevelId = null;

                _songListTableView.SetLevels(_beatmapLevels);

                var listTableParentRectTransform = _songListTableView.transform.parent.gameObject.GetComponent<RectTransform>();
                var value = (listTableParentRectTransform.rect.height - _songListTableView.NumberOfRows() * _songListTableView.RowHeight()) * .5f;

                var listTableRectTransform = _songListTableView.GetComponent<RectTransform>();
                listTableRectTransform.offsetMin = new(listTableRectTransform.offsetMin.x, Mathf.Max(value, 0f));
                listTableRectTransform.offsetMax = new(listTableRectTransform.offsetMax.x, Mathf.Min(0f - value, 0f));

                _songListTableView.ClearSelection();
            }
        }

        private void HandleSongListTableViewDidSelectRow(SongListTableView tableView, int row)
        {
            LevelId = _beatmapLevels[row].levelID;

            if (DidSelectSongEvent != null)
                DidSelectSongEvent(this);
        }

        public void Init(IPreviewBeatmapLevel[] levelsStaticData)
        {
            _beatmapLevels = levelsStaticData;
        }

        public void SelectSong(int songIndex)
        {
            LevelId = _beatmapLevels[songIndex].levelID;

            _songListTableView.SelectRow(songIndex);
        }
    }
}
