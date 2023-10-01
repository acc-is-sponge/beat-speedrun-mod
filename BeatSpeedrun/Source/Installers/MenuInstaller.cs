using BeatSpeedrun.Controllers;
using BeatSpeedrun.Controllers.Support;
using BeatSpeedrun.Registerers;
using BeatSpeedrun.Source.Controllers;
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
            
            Container.BindInterfacesAndSelfTo<ModFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();

            Container.BindInterfacesAndSelfTo<MainSettingsViewController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SettingsRegisterer>().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardRegisterer>().AsSingle();

            Container.BindInterfacesAndSelfTo<StarDisplay>().AsSingle();

            Container.BindInterfacesAndSelfTo<FloatingTimerViewController>().AsSingle();
            Container.BindInterfacesAndSelfTo<FloatingTimerView>().FromNewComponentAsViewController().AsSingle();
        }
    }
}
