using OldschoolBeatSaber.Managers;
using Zenject;

namespace OldschoolBeatSaber.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            // Managers
            Container.BindInterfacesAndSelfTo<CustomMenuManager>().AsSingle();
            Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
        }
    }
}
