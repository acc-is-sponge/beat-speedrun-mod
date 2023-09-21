using BeatSpeedrun.Controllers;
using BeatSpeedrun.Controllers.Support;
using BeatSpeedrun.Registerers;
using BeatSpeedrun.Source.Views;
using Zenject;

namespace BeatSpeedrun.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SelectionState>().AsSingle();

            Container.BindInterfacesAndSelfTo<SettingsViewController>().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardMainViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardPanelViewController>().FromNewComponentAsViewController().AsSingle();

            Container.BindInterfacesAndSelfTo<SettingsRegisterer>().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardRegisterer>().AsSingle();

            Container.BindInterfacesAndSelfTo<StarDisplay>().AsSingle();
        }
    }
}
