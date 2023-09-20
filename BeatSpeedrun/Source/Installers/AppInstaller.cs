using BeatSpeedrun.Providers;
using BeatSpeedrun.Managers;
using Zenject;

namespace BeatSpeedrun.Installers
{
    internal class AppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<RegulationProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<MapSetProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<SpeedrunManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<CurrentSpeedrunManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<LevelPlayManager>().AsSingle().NonLazy();
        }
    }
}
