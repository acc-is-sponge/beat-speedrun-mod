using BeatSpeedrun.Providers;
using BeatSpeedrun.Repositories;
using BeatSpeedrun.Services;
using Zenject;

namespace BeatSpeedrun.Installers
{
    internal class AppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<RegulationProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<MapSetProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<SpeedrunRepository>().AsSingle();
            Container.BindInterfacesAndSelfTo<LocalLeaderboardRepository>().AsSingle();
            Container.BindInterfacesAndSelfTo<SpeedrunFacilitator>().AsSingle();
            Container.BindInterfacesAndSelfTo<LevelPlayObserver>().AsSingle();
        }
    }
}
