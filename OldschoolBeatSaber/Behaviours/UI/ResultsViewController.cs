using System;
using TMPro;
using UnityEngine;
using VRUI;

namespace OldschoolBeatSaber.Behaviours.UI
{
    public class ResultsViewController : VRUIViewController
    {
        [SerializeField] private GameObject _failedPanel;
        [SerializeField] private GameObject _clearedPanel;
        [SerializeField] private TextMeshProUGUI _failedSongNameText;
        [SerializeField] private TextMeshProUGUI _failedSongAuthorText;
        [SerializeField] private TextMeshProUGUI _failedDifficultyText;
        [SerializeField] private TextMeshProUGUI _clearedSongNameText;
        [SerializeField] private TextMeshProUGUI _clearedSongAuthorText;
        [SerializeField] private TextMeshProUGUI _clearedDifficultyText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _rankText;
        [SerializeField] private TextMeshProUGUI _goodCutsPercentageText;
        [SerializeField] private TextMeshProUGUI _fullComboText;
        [SerializeField] private TextMeshProUGUI _scoreInfoText;

        private LevelCompletionResults _levelCompletionResults;
        private IDifficultyBeatmap _difficultyLevel;

        public event Action<ResultsViewController> ResultsViewControllerDidPressContinueButtonEvent;
        public event Action<ResultsViewController> ResultsViewControllerDidPressRestartButtonEvent;

        public void Init(LevelCompletionResults levelCompletionResults, IDifficultyBeatmap difficultyLevel)
        {
            _levelCompletionResults = levelCompletionResults;
            _difficultyLevel = difficultyLevel;
        }

        protected override void DidActivate()
        {
            base.DidActivate();

            screen.screenSystem.leftScreen.SetRootViewController(null);
            screen.screenSystem.rightScreen.SetRootViewController(null);

            _failedPanel.SetActive(_levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed);
            _clearedPanel.SetActive(_levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared);

            if (_levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed)
            {
                _failedSongNameText.text = _difficultyLevel.level.songName + " <size=80%>" + _difficultyLevel.level.songSubName;
                _failedSongAuthorText.text = _difficultyLevel.level.songAuthorName;
                _failedDifficultyText.text = "Difficulty - " + _difficultyLevel.difficulty.Name();
            }
            else if (_levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared)
            {
                _clearedSongNameText.text = _difficultyLevel.level.songName + " <size=80%>" + _difficultyLevel.level.songSubName;
                _clearedSongAuthorText.text = _difficultyLevel.level.songAuthorName;
                _clearedDifficultyText.text = "Difficulty - " + _difficultyLevel.difficulty.Name();
            }

            _scoreText.text = ScoreFormatter.Format(_levelCompletionResults.modifiedScore);
            _scoreInfoText.gameObject.SetActive(_levelCompletionResults.energy == 0f);

            if (_levelCompletionResults.energy == 0f)
                _scoreInfoText.text = "Played with <color=#FF175A>NO ENERGY</color> option.";

            _rankText.text = RankModel.GetRankName(_levelCompletionResults.rank);

            var notesCount = _levelCompletionResults.goodCutsCount + _levelCompletionResults.missedCount + _levelCompletionResults.badCutsCount;
            _goodCutsPercentageText.text = _levelCompletionResults.goodCutsCount + "<size=50%> / " + notesCount + "</size>";

            _fullComboText.gameObject.SetActive(_levelCompletionResults.fullCombo);
        }

        public void RestartButtonPressed()
        {
            if (ResultsViewControllerDidPressRestartButtonEvent != null)
                ResultsViewControllerDidPressRestartButtonEvent(this);
        }

        public void ContinueButtonPressed()
        {
            if (ResultsViewControllerDidPressContinueButtonEvent != null)
                ResultsViewControllerDidPressContinueButtonEvent(this);
        }
    }
}
