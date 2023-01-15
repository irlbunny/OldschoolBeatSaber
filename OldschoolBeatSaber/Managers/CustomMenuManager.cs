using HMUI;
using IPA.Utilities;
using OldschoolBeatSaber.Interfaces;
using OldschoolBeatSaber.Utilities;
using UnityEngine;
using Zenject;

namespace OldschoolBeatSaber.Managers
{
    public class CustomMenuManager : IInitializable
    {
        private static readonly FieldAccessor<SongPreviewPlayer, AudioClip>.Accessor _defaultAudioClipAccessor =
            FieldAccessor<SongPreviewPlayer, AudioClip>.GetAccessor("_defaultAudioClip");

        private readonly DiContainer _container;
        private readonly HierarchyManager _hierarchyManager;
        private readonly MenuEnvironmentManager _menuEnvironmentManager;
        private SongPreviewPlayer _songPreviewPlayer;

        private bool _didInitialize;
        private GameObject _menuCoreGameObject;
        private GameObject _menuEnvironmentGameObject;
        private AudioClip _originalMenuAudioClip;

        public CustomMenuManager(DiContainer container, HierarchyManager hierarchyManager,
            MenuEnvironmentManager menuEnvironmentManager, SongPreviewPlayer songPreviewPlayer)
        {
            _container = container;
            _hierarchyManager = hierarchyManager;
            _menuEnvironmentManager = menuEnvironmentManager;
            _songPreviewPlayer = songPreviewPlayer;
        }

        public void Initialize()
        {
            Plugin.MenuCorePrefab.ReplaceMaterialsInChildren();
            Plugin.MenuEnvironmentPrefab.ReplaceMaterialsInChildren();

            _menuCoreGameObject = _container.InstantiatePrefab(Plugin.MenuCorePrefab);
            _menuEnvironmentGameObject = _container.InstantiatePrefab(Plugin.MenuEnvironmentPrefab);

            _originalMenuAudioClip = _defaultAudioClipAccessor(ref _songPreviewPlayer);

            // We need to toggle the custom menu at least once.
            EnableCustomMenu(true);
            EnableCustomMenu(false);

            _didInitialize = true;
        }

        public void EnableCustomMenu(bool enabled, bool onlyScreenSystem = false)
        {
            if (enabled)
            {
                if (_didInitialize)
                    _hierarchyManager.gameObject.SetActive(false);

                foreach (IOnActivate onActivate in _menuCoreGameObject.GetComponentsInChildren<IOnActivate>(true))
                {
                    onActivate.OnActivate(_container);
                }

                if (!onlyScreenSystem)
                {
                    if (_didInitialize)
                        _menuEnvironmentManager.ShowEnvironmentType(MenuEnvironmentManager.MenuEnvironmentType.None);

                    foreach (IOnActivate onActivate in _menuEnvironmentGameObject.GetComponentsInChildren<IOnActivate>(true))
                    {
                        onActivate.OnActivate(_container);
                    }

                    _menuEnvironmentGameObject.SetActive(true);
                }

                _menuCoreGameObject.SetActive(true);

                SetDefaultAudioClip(Plugin.MenuAudioClip);
            }
            else
            {
                if (_didInitialize)
                    _hierarchyManager.gameObject.SetActive(true);

                foreach (IOnDeactivate onDeactivate in _menuCoreGameObject.GetComponentsInChildren<IOnDeactivate>(true))
                {
                    onDeactivate.OnDeactivate();
                }

                if (!onlyScreenSystem)
                {
                    if (_didInitialize)
                        _menuEnvironmentManager.ShowEnvironmentType(MenuEnvironmentManager.MenuEnvironmentType.Default);

                    foreach (IOnDeactivate onDeactivate in _menuEnvironmentGameObject.GetComponentsInChildren<IOnDeactivate>(true))
                    {
                        onDeactivate.OnDeactivate();
                    }

                    _menuEnvironmentGameObject.SetActive(false);
                }

                _menuCoreGameObject.SetActive(false);

                SetDefaultAudioClip(_originalMenuAudioClip);
            }
        }

        private void SetDefaultAudioClip(AudioClip audioClip)
        {
            _songPreviewPlayer.enabled = false;
            _defaultAudioClipAccessor(ref _songPreviewPlayer) = audioClip;
            _songPreviewPlayer.enabled = true;
        }
    }
}
