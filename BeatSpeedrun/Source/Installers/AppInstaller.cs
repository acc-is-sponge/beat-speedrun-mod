using BeatSpeedrun.Managers;
using Zenject;

namespace BeatSpeedrun.Installers
{
    internal class AppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<RegulationManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<MapSetManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<SpeedrunManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<CurrentSpeedrunManager>().AsSingle();
        }
    }
}
