using BeatSpeedrun.Services;
using BeatSpeedrun.Controllers;
using BeatSpeedrun.Registerers;
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

            Container.BindInterfacesAndSelfTo<SettingsRegisterer>().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardRegisterer>().AsSingle();
            Container.BindInterfacesAndSelfTo<SelectionState>().AsSingle();
        }
    }
}
