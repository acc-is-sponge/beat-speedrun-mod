using BeatSpeedrun.Controllers;
using BeatSpeedrun.Controllers.Support;
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

            Container.BindInterfacesAndSelfTo<StartStopViewController>().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardMainViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardPanelViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<SpeedrunStarViewController>().AsSingle();

            Container.BindInterfacesAndSelfTo<TabRegisterer>().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardRegisterer>().AsSingle();

            Container.BindInterfacesAndSelfTo<ModFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesAndSelfTo<ModSettingsViewController>().AsSingle();
        }
    }
}
