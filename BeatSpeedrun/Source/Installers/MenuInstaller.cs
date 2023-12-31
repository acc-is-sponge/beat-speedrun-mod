using BeatSpeedrun.Controllers;
using BeatSpeedrun.Controllers.Support;
using BeatSpeedrun.FlowCoordinators;
using BeatSpeedrun.Registerers;
using Zenject;

namespace BeatSpeedrun.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SelectionState>().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardState>().AsSingle();

            Container.BindInterfacesAndSelfTo<ModSettingsViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<StartStopViewController>().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardMainViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardPanelViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<SpeedrunStarViewController>().AsSingle();

            Container.BindInterfacesAndSelfTo<ModSettingsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();

            Container.BindInterfacesAndSelfTo<TabRegisterer>().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardRegisterer>().AsSingle();
            Container.BindInterfacesAndSelfTo<ModSettingsRegisterer>().AsSingle();
        }
    }
}
