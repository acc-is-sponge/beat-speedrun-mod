using BeatSpeedrun.Controllers;
using BeatSpeedrun.Managers;
using Zenject;

namespace BeatSpeedrun.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SettingsViewController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LeaderboardMainViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardPanelViewController>().FromNewComponentAsViewController().AsSingle();

            Container.BindInterfacesAndSelfTo<LeaderboardManager>().AsSingle();
        }
    }
}
