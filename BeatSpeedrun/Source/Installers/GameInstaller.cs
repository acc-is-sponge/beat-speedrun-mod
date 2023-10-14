using BeatSpeedrun.Controllers;
using BeatSpeedrun.Registerers;
using Zenject;

namespace BeatSpeedrun.Installers
{
    internal class GameInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<FloatingTimerViewController>().FromNewComponentAsViewController().AsSingle();

            Container.BindInterfacesAndSelfTo<FloatingTimerRegisterer>().AsSingle();
        }
    }
}
