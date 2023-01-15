using OldschoolBeatSaber.Interfaces;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using VRUI;
using Zenject;

namespace OldschoolBeatSaber.Behaviours.UI
{
    public class SongSelectionMasterViewController : VRUINavigationController, IOnActivate
    {
        [SerializeField] private GameObject _dismissButton;
        [SerializeField] private GameplayModeIndicator _gameplayModeIndicator;
        [SerializeField] private SongListViewController _songListViewController;
        [SerializeField] private DifficultyViewController _difficultyViewController;
        [SerializeField] private SongDetailViewController _songDetailViewController;
        [SerializeField] private GameplayOptionsViewController _gameplayOptionsViewController;
        [SerializeField] private VRUIViewController _leaderboardsComingSoonViewController;

        private bool _firstTimeActivated = true;

        private SongPreviewPlayer _songPreviewPlayer;
        private AudioClipAsyncLoader _audioClipAsyncLoader;
        private PerceivedLoudnessPerLevelModel _perceivedLoudnessPerLevelModel;
        private BeatmapLevelsModel _beatmapLevelsModel;

        private string _levelId;
        private bool _difficultySelected;
        private IBeatmapLevel _beatmapLevel;
        private BeatmapDifficulty _difficulty;
        private IDifficultyBeatmap[] _difficultyLevels;
        private IPreviewBeatmapLevel[] _beatmapLevels;

        private GameplayMode _gameplayMode;

        public BeatmapDifficulty Difficulty => _difficulty;
        public IDifficultyBeatmap[] DifficultyLevels => _difficultyLevels;

        public event Action<SongSelectionMasterViewController> DidFinishEvent;
        public event Action<SongSelectionMasterViewController> DidPressPlayButtonEvent;

        [Inject]
        public void Construct(SongPreviewPlayer songPreviewPlayer, AudioClipAsyncLoader audioClipAsyncLoader,
            PerceivedLoudnessPerLevelModel perceivedLoudnessPerLevelModel, BeatmapLevelsModel beatmapLevelsModel)
        {
            _songPreviewPlayer = songPreviewPlayer;
            _audioClipAsyncLoader = audioClipAsyncLoader;
            _perceivedLoudnessPerLevelModel = perceivedLoudnessPerLevelModel;
            _beatmapLevelsModel = beatmapLevelsModel;
        }

        public void OnActivate(DiContainer container)
        {
            container.Inject(this);
        }

        protected override void DidActivate()
        {
            base.DidActivate();

            if (_firstTimeActivated)
            {
                _firstTimeActivated = false;

                _songListViewController.DidSelectSongEvent += HandleSongListDidSelectSong;
                _difficultyViewController.DidSelectDifficultyEvent += HandleDifficultyViewControllerDidSelectDifficulty;
                _songDetailViewController.DidPressPlayButtonEvent += HandleSongDetailViewControllerDidPressPlayButton;
            }

            if (beingPresented)
            {
                PushViewController(_songListViewController, true);

                _gameplayModeIndicator.SetupForGameplayMode(_gameplayMode);

                if (_levelId != null)
                {
                    _difficultySelected = true;

                    _difficultyViewController.Init(_difficultyLevels);

                    PushViewController(_difficultyViewController, true);
                    PushViewController(_songDetailViewController, true);

                    _songListViewController.SelectSong(GetSelectedSongIndex());
                    _difficultyViewController.SelectDifficulty(_difficulty);

                    RefreshSongDetail();
                }
            }

            screen.screenSystem.leftScreen.SetRootViewController(_gameplayOptionsViewController);
            screen.screenSystem.rightScreen.SetRootViewController(_leaderboardsComingSoonViewController);
        }

        protected override void DidDeactivate()
        {
            base.DidDeactivate();

            if (beingDismissed)
            {
                _levelId = null;
                _difficultySelected = false;
                _songPreviewPlayer.CrossfadeToDefault();
            }
        }

        public void DismissButtonWasPressed()
        {
            if (DidFinishEvent != null)
                DidFinishEvent(this);
        }

        private int GetSelectedSongIndex()
        {
            for (var i = 0; i < _beatmapLevels.Length; i++)
            {
                if (_beatmapLevels[i].levelID == _levelId)
                    return i;
            }

            return -1;
        }

        private IPreviewBeatmapLevel GetPreviewBeatmapLevelForSelectedSong()
        {
            var selectedSongIndex = GetSelectedSongIndex();
            if (selectedSongIndex != -1)
                return _beatmapLevels[selectedSongIndex];

            return null;
        }

        private string GetBeatmapCharacteristicNameForGameplayMode()
        {
            return _gameplayMode switch
            {
                GameplayMode.SoloOneSaber => "OneSaber",
                GameplayMode.SoloNoArrows => "NoArrows",
                _ => "Standard"
            };
        }

        private async void HandleSongListDidSelectSong(SongListViewController songListViewController)
        {
            var wasLevelIdNull = _levelId == null;
            _levelId = songListViewController.LevelId;
            var previewBeatmapLevel = GetPreviewBeatmapLevelForSelectedSong();

            var audioClip = await _audioClipAsyncLoader.LoadPreview(previewBeatmapLevel);
            var musicVolume = _perceivedLoudnessPerLevelModel.GetLoudnessCorrectionByLevelId(previewBeatmapLevel.levelID);
            _songPreviewPlayer.CrossfadeTo(audioClip, musicVolume, previewBeatmapLevel.previewStartTime, previewBeatmapLevel.previewDuration, () =>
            {
                _audioClipAsyncLoader.UnloadPreview(previewBeatmapLevel);
            });

            // TODO: Cancellation tokens.
            var beatmapLevelResult = await _beatmapLevelsModel.GetBeatmapLevelAsync(_levelId, CancellationToken.None);
            _beatmapLevel = beatmapLevelResult.beatmapLevel;
            var difficultyBeatmapSet = _beatmapLevel.beatmapLevelData.difficultyBeatmapSets
                .Where(x => x.beatmapCharacteristic.serializedName == GetBeatmapCharacteristicNameForGameplayMode()).FirstOrDefault();
            _difficultyLevels = difficultyBeatmapSet.difficultyBeatmaps.ToArray();
            if (wasLevelIdNull)
            {
                _difficultyViewController.Init(_difficultyLevels);
                PushViewController(_difficultyViewController);
            }

            _difficultyViewController.SetDifficultyLevels(_difficultyLevels);
            if (_difficultySelected)
                RefreshSongDetail();
        }

        private void HandleDifficultyViewControllerDidSelectDifficulty(DifficultyViewController difficultyViewController)
        {
            var wasDifficultySelected = !_difficultySelected;
            _difficultySelected = true;
            _difficulty = difficultyViewController.Difficulty;

            if (wasDifficultySelected)
                PushViewController(_songDetailViewController);

            RefreshSongDetail();
        }

        private void HandleSongDetailViewControllerDidPressPlayButton(SongDetailViewController viewController)
        {
            if (DidPressPlayButtonEvent != null)
                DidPressPlayButtonEvent(this);
        }

        public void Init(string levelId, BeatmapDifficulty difficulty, IPreviewBeatmapLevel[] beatmapLevels, bool showDismissButton, GameplayMode gameplayMode)
        {
            _levelId = levelId;
            _difficulty = difficulty;
            _beatmapLevels = beatmapLevels;

            _gameplayMode = gameplayMode;

            if (GetSelectedSongIndex() == -1)
                _levelId = null;

            _songListViewController.Init(_beatmapLevels);

            _dismissButton.SetActive(showDismissButton);
        }

        public void RefreshSongDetail()
        {
            var previewBeatmapLevel = GetPreviewBeatmapLevelForSelectedSong();
            var difficultyLevel = _difficultyLevels.Where(x => x.difficulty == _difficulty).FirstOrDefault();
            _songDetailViewController.SetLevelData(previewBeatmapLevel, difficultyLevel);
        }
    }
}
