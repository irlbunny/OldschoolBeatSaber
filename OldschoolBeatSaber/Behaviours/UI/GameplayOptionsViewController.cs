using HMUI;
using UnityEngine;
using VRUI;
using Zenject;

namespace OldschoolBeatSaber.Behaviours.UI
{
    public class GameplayOptionsViewController : VRUIViewController
    {
        [SerializeField] private Toggle _noEnergyToggle;
        [SerializeField] private Toggle _mirrorToggle;

        private PlayerDataModel _playerDataModel;
        private bool _firstTimeActivated = true;

        [Inject]
        public void Construct(PlayerDataModel playerDataModel)
        {
            _playerDataModel = playerDataModel;
        }

        protected override void DidActivate()
        {
            base.DidActivate();

            if (_firstTimeActivated)
            {
                _firstTimeActivated = false;

                _noEnergyToggle.didSwitchEvent += HandleNoEnergyToggleDidSwitch;
                _mirrorToggle.didSwitchEvent += HandleMirrorToggleDidSwitch;
            }

            if (beingPresented)
                RefreshToggles();
        }

        private void HandleNoEnergyToggleDidSwitch(Toggle toggle, bool isOn)
        {
            _playerDataModel.playerData.SetGameplayModifiers(_playerDataModel.playerData.gameplayModifiers.CopyWith(noFailOn0Energy: isOn));
            _playerDataModel.Save();
        }

        private void HandleMirrorToggleDidSwitch(Toggle toggle, bool isOn)
        {
            _playerDataModel.playerData.SetPlayerSpecificSettings(_playerDataModel.playerData.playerSpecificSettings.CopyWith(leftHanded: isOn));
            _playerDataModel.Save();
        }

        private void RefreshToggles()
        {
            _noEnergyToggle.isOn = _playerDataModel.playerData.gameplayModifiers.noFailOn0Energy;
            _mirrorToggle.isOn = _playerDataModel.playerData.playerSpecificSettings.leftHanded;
        }

        public void DefaultsButtonWasPressed()
        {
            _playerDataModel.playerData.SetGameplayModifiers(_playerDataModel.playerData.gameplayModifiers.CopyWith(noFailOn0Energy: false));
            _playerDataModel.playerData.SetPlayerSpecificSettings(_playerDataModel.playerData.playerSpecificSettings.CopyWith(leftHanded: false));
            _playerDataModel.Save();

            RefreshToggles();
        }
    }
}
