using OldschoolBeatSaber.Interfaces;
using OldschoolBeatSaber.Managers;
using System;
using UnityEngine;
using VRUI;
using Zenject;

namespace OldschoolBeatSaber.Behaviours.UI
{
    public class MainMenuViewController : VRUIViewController, IOnActivate
    {
        [SerializeField] private HowToPlayViewController _howToPlayViewController;
        [SerializeField] private VRUIViewController _releaseInfoViewController;

        private PlayerDataModel _playerDataModel;
        private CustomMenuManager _customMenuManager;
        private MenuTransitionsHelper _menuTransitionsHelper;

        public Action<MainMenuViewController> DidSelectSoloEvent;
        public Action<MainMenuViewController> DidSelectPartyEvent;
        public Action<MainMenuViewController> DidSelectAboutEvent;
        public Action<MainMenuViewController> DidSelectSettingsEvent;

        [Inject]
        public void Construct(PlayerDataModel playerDataModel, CustomMenuManager customMenuManager,
            MenuTransitionsHelper menuTransitionsHelper)
        {
            _playerDataModel = playerDataModel;
            _customMenuManager = customMenuManager;
            _menuTransitionsHelper = menuTransitionsHelper;
        }

        public void OnActivate(DiContainer container)
        {
            container.Inject(this);
        }

        protected override void DidActivate()
        {
            base.DidActivate();

            screen.screenSystem.leftScreen.SetRootViewController(_howToPlayViewController);
            screen.screenSystem.rightScreen.SetRootViewController(_releaseInfoViewController);
        }

        public void SoloButtonPressed()
        {
            if (DidSelectSoloEvent != null)
                DidSelectSoloEvent(this);
        }

        public void PartyButtonPressed()
        {
            if (DidSelectPartyEvent != null)
                DidSelectPartyEvent(this);
        }

        public void TutorialButtonPressed()
        {
            _menuTransitionsHelper.StartTutorial(_playerDataModel.playerData.playerSpecificSettings);
        }

        public void AboutButtonPressed()
        {
            if (DidSelectAboutEvent != null)
                DidSelectAboutEvent(this);
        }

        public void SettingsButtonPressed()
        {
            if (DidSelectSettingsEvent != null)
                DidSelectSettingsEvent(this);
        }

        public void QuitGameButtonPressed()
        {
            _customMenuManager.EnableCustomMenu(false);
        }
    }
}
