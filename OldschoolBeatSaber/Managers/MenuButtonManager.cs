using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace OldschoolBeatSaber.Managers
{
    internal class MenuButtonManager : IInitializable, IDisposable
    {
        private const string BUTTONTEXT = "Oldschool Beat Saber";
        private const string BUTTONHINT = "Go back in time.";

        private readonly CustomMenuManager _customMenuManager;
        private readonly MenuButton _menuButton;

        public MenuButtonManager(CustomMenuManager customMenuManager)
        {
            _customMenuManager = customMenuManager;
            _menuButton = new MenuButton(BUTTONTEXT, BUTTONHINT, OnMenuButtonClick);
        }

        public async void Initialize()
        {
            await Task.Run(() => Thread.Sleep(100));

            MenuButtons.instance.RegisterButton(_menuButton);
        }

        public void Dispose()
        {
            if (BSMLParser.IsSingletonAvailable && MenuButtons.IsSingletonAvailable)
                MenuButtons.instance.UnregisterButton(_menuButton);
        }

        private void OnMenuButtonClick()
        {
            _customMenuManager.EnableCustomMenu(true);
        }
    }
}
