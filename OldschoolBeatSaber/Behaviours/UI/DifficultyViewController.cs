using System;
using UnityEngine;
using VRUI;

namespace OldschoolBeatSaber.Behaviours.UI
{
    public class DifficultyViewController : VRUIViewController
    {
        [SerializeField] private DifficultyTableView _difficultyTableView;

        private bool _firstTimeActivated = true;

        private bool _difficultySelected;
        private IDifficultyBeatmap[] _difficultyLevels;
        private BeatmapDifficulty _preferredDifficulty = BeatmapDifficulty.Normal;

        public BeatmapDifficulty Difficulty { get; private set; }

        public Action<DifficultyViewController> DidSelectDifficultyEvent;

        protected override void DidActivate()
        {
            base.DidActivate();

            if (_firstTimeActivated)
            {
                _firstTimeActivated = false;

                _difficultyTableView.DifficultyTableViewDidSelectRow += HandleDifficultyTableViewDidSelectRow;
            }

            if (movingToParentController)
            {
                _difficultySelected = false;
                _difficultyTableView.SetDifficultyLevels(_difficultyLevels);
                _difficultyTableView.ClearSelection();

                RecenterTableView();
            }
        }

        private int GetClosestDifficultyIndex(BeatmapDifficulty difficulty)
        {
            var result = -1;

            foreach (var difficultyLevel in _difficultyLevels)
            {
                if (difficulty < difficultyLevel.difficulty)
                    break;

                result++;
            }

            if (result == -1)
                result = 0;

            return result;
        }

        private int GetDifficultyLevelIndex(BeatmapDifficulty difficulty)
        {
            for (var i = 0; i < _difficultyLevels.Length; i++)
            {
                if (_difficultyLevels[i].difficulty == difficulty)
                    return i;
            }

            return -1;
        }

        private void RecenterTableView()
        {
            var difficultyTableRectTransform = _difficultyTableView.GetComponent<RectTransform>();
            var y = (rectTransform.rect.height - _difficultyTableView.NumberOfRows() * _difficultyTableView.RowHeight()) * .5f;
            difficultyTableRectTransform.offsetMin = new(difficultyTableRectTransform.offsetMin.x, y);
            difficultyTableRectTransform.offsetMax = new(difficultyTableRectTransform.offsetMax.x, 0f - y);
        }

        private void HandleDifficultyTableViewDidSelectRow(DifficultyTableView tableView, int row)
        {
            Difficulty = (_preferredDifficulty = _difficultyLevels[row].difficulty);
            _difficultySelected = true;

            if (DidSelectDifficultyEvent != null)
                DidSelectDifficultyEvent(this);
        }

        public void Init(IDifficultyBeatmap[] difficultyLevels)
        {
            _difficultyLevels = difficultyLevels;
        }

        public void SetDifficultyLevels(IDifficultyBeatmap[] difficultyLevels)
        {
            _difficultyLevels = difficultyLevels;
            _difficultyTableView.SetDifficultyLevels(_difficultyLevels);

            if (_difficultySelected)
            {
                var closestDifficultyIndex = GetClosestDifficultyIndex(_preferredDifficulty);
                Difficulty = _difficultyLevels[closestDifficultyIndex].difficulty;
                _difficultyTableView.SelectRow(closestDifficultyIndex);
            }

            RecenterTableView();
        }

        public void SelectDifficulty(BeatmapDifficulty difficulty)
        {
            Difficulty = _preferredDifficulty = difficulty;
            _difficultySelected = true;

            var difficultyLevelIndex = GetDifficultyLevelIndex(difficulty);
            _difficultyTableView.SelectRow(difficultyLevelIndex);
        }
    }
}
