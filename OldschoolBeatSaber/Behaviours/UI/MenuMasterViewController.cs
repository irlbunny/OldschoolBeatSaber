using OldschoolBeatSaber.Interfaces;
using Polyglot;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRUI;
using Zenject;

namespace OldschoolBeatSaber.Behaviours.UI
{
    public class MenuMasterViewController : VRUIViewController, IOnActivate
    {
        [SerializeField] private MainMenuViewController _mainMenuViewController;
        [SerializeField] private SoloModeSelectionViewController _soloModeSelectionViewController;
        [SerializeField] private SongSelectionMasterViewController _songSelectionMasterViewController;
        [SerializeField] private DismissableViewController _comingSoonViewController;
        [SerializeField] private DismissableViewController _aboutViewController;
        [SerializeField] private ResultsViewController _resultsViewController;

        private bool _firstTimeActivated = true;

        private PlayerDataModel _playerDataModel;
        private BeatmapLevelsModel _beatmapLevelsModel;
        private MenuTransitionsHelper _menuTransitionsHelper;
        private PlatformLeaderboardsModel _platformLeaderboardsModel;
        private IDifficultyBeatmap _lastPlayedDifficultyLevel;

        [Inject]
        public void Construct(PlayerDataModel playerDataModel, BeatmapLevelsModel beatmapLevelsModel,
            MenuTransitionsHelper menuTransitionsHelper, PlatformLeaderboardsModel platformLeaderboardsModel)
        {
            _playerDataModel = playerDataModel;
            _beatmapLevelsModel = beatmapLevelsModel;
            _menuTransitionsHelper = menuTransitionsHelper;
            _platformLeaderboardsModel = platformLeaderboardsModel;
        }

        public void OnActivate(DiContainer container)
        {
            container.Inject(this);
        }

        protected override void DidActivate()
        {
            base.DidActivate();

            if (!_firstTimeActivated)
                return;

            _firstTimeActivated = false;

            _mainMenuViewController.DidSelectSoloEvent += HandleMainMenuViewControllerDidSelectSolo;
            _mainMenuViewController.DidSelectPartyEvent += HandleMainMenuViewControllerDidSelectParty;
            _mainMenuViewController.DidSelectAboutEvent += HandleMainMenuViewControllerDidSelectAbout;

            _soloModeSelectionViewController.DidSelectModeEvent += HandleSoloModeSelectionViewControllerDidSelectMode;
            _soloModeSelectionViewController.DidFinishEvent += HandleSoloModeSelectionViewControllerDidFinish;

            _songSelectionMasterViewController.DidFinishEvent += HandleSongSelectionMasterViewControllerDidFinish;
            _songSelectionMasterViewController.DidPressPlayButtonEvent += HandleSongSelectionMasterViewControllerDidPressPlayButton;

            _comingSoonViewController.DidFinishEvent += HandleDismissableViewControllerDidFinish;
            _aboutViewController.DidFinishEvent += HandleDismissableViewControllerDidFinish;

            _resultsViewController.ResultsViewControllerDidPressRestartButtonEvent += HandleResultsViewControllerDidPressRestartButton;
            _resultsViewController.ResultsViewControllerDidPressContinueButtonEvent += HandleResultsViewControllerDidPressContinueButton;

            PresentModalViewController(_mainMenuViewController, null, true);
        }

        private IPreviewBeatmapLevel[] GetLevelsForGameplayMode(GameplayMode gameplayMode)
        {
            // Get all OSTs & Extras + custom levels, DLC is excluded due to a few issues.
            var previewBeatmapLevels = new List<IPreviewBeatmapLevel>();
            previewBeatmapLevels.AddRange(_beatmapLevelsModel.ostAndExtrasPackCollection.beatmapLevelPacks.SelectMany(x => x.beatmapLevelCollection.beatmapLevels));
            previewBeatmapLevels.AddRange(_beatmapLevelsModel.customLevelPackCollection.beatmapLevelPacks[0].beatmapLevelCollection.beatmapLevels);

            var beatmapCharacteristicName = gameplayMode switch
            {
                GameplayMode.SoloOneSaber => "OneSaber",
                GameplayMode.SoloNoArrows => "NoArrows",
                _ => "Standard"
            };
            return previewBeatmapLevels.Where(x => x.previewDifficultyBeatmapSets.Any(y => y.beatmapCharacteristic.serializedName == beatmapCharacteristicName)).ToArray();
        }

        private void HandleMainMenuViewControllerDidSelectSolo(MainMenuViewController viewController)
        {
            viewController.PresentModalViewController(_soloModeSelectionViewController, null);
        }

        private void HandleMainMenuViewControllerDidSelectParty(MainMenuViewController viewController)
        {
            viewController.PresentModalViewController(_comingSoonViewController, null);
        }

        private void HandleMainMenuViewControllerDidSelectAbout(MainMenuViewController viewController)
        {
            viewController.PresentModalViewController(_aboutViewController, null);
        }

        private void HandleSoloModeSelectionViewControllerDidSelectMode(SoloModeSelectionViewController viewController, GameplayMode gameplayMode)
        {
            _songSelectionMasterViewController.Init(null, BeatmapDifficulty.Normal, GetLevelsForGameplayMode(gameplayMode), true, gameplayMode);
            viewController.PresentModalViewController(_songSelectionMasterViewController, null);
        }

        private void HandleSoloModeSelectionViewControllerDidFinish(SoloModeSelectionViewController viewController)
        {
            viewController.DismissModalViewController(null);
        }

        private void HandleSongSelectionMasterViewControllerDidFinish(SongSelectionMasterViewController viewController)
        {
            viewController.DismissModalViewController(null);
        }

        private void HandleSongSelectionMasterViewControllerDidPressPlayButton(SongSelectionMasterViewController viewController)
        {
            _lastPlayedDifficultyLevel = viewController.DifficultyLevels.Where(x => x.difficulty == viewController.Difficulty).FirstOrDefault();
            _menuTransitionsHelper.StartStandardLevel("", _lastPlayedDifficultyLevel, _lastPlayedDifficultyLevel.level, _playerDataModel.playerData.overrideEnvironmentSettings, _playerDataModel.playerData.colorSchemesSettings.GetOverrideColorScheme(), _playerDataModel.playerData.gameplayModifiers, _playerDataModel.playerData.playerSpecificSettings, null, Localization.Get("BUTTON_MENU"), false, false, null, HandleStandardLevelDidFinish, null);
        }

        private void HandleDismissableViewControllerDidFinish(DismissableViewController viewController)
        {
            viewController.DismissModalViewController(null);
        }

        private void HandleResultsViewControllerDidPressRestartButton(ResultsViewController viewController)
        {
            _menuTransitionsHelper.StartStandardLevel("", _lastPlayedDifficultyLevel, _lastPlayedDifficultyLevel.level, _playerDataModel.playerData.overrideEnvironmentSettings, _playerDataModel.playerData.colorSchemesSettings.GetOverrideColorScheme(), _playerDataModel.playerData.gameplayModifiers, _playerDataModel.playerData.playerSpecificSettings, null, Localization.Get("BUTTON_MENU"), false, false, null, HandleStandardLevelDidFinish, null);
        }

        private void HandleResultsViewControllerDidPressContinueButton(ResultsViewController viewController)
        {
            viewController.DismissModalViewController(null);
        }

        private bool HandleBasicLevelCompletionResults(LevelCompletionResults levelCompletionResults)
        {
            return levelCompletionResults.levelEndStateType != LevelCompletionResults.LevelEndStateType.Failed && levelCompletionResults.levelEndStateType != LevelCompletionResults.LevelEndStateType.Cleared;
        }

        private void HandleStandardLevelDidFinish(StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupData, LevelCompletionResults levelCompletionResults)
        {
            var transformedBeatmapData = standardLevelScenesTransitionSetupData.transformedBeatmapData;
            var difficultyLevel = standardLevelScenesTransitionSetupData.difficultyBeatmap;

            _playerDataModel.playerData.playerAllOverallStatsData.UpdateSoloFreePlayOverallStatsData(levelCompletionResults, difficultyLevel);

            if (HandleBasicLevelCompletionResults(levelCompletionResults))
            {
                _playerDataModel.Save();
                return;
            }

            LevelCompletionResultsHelper.ProcessScore(_playerDataModel.playerData, _playerDataModel.playerData.GetPlayerLevelStatsData(difficultyLevel), levelCompletionResults, transformedBeatmapData, difficultyLevel, _platformLeaderboardsModel);

            _resultsViewController.Init(levelCompletionResults, difficultyLevel);
            _songSelectionMasterViewController.PresentModalViewController(_resultsViewController, null, true);

            _playerDataModel.Save();
        }
    }
}
