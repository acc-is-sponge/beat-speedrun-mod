using BeatSpeedrun.Views;
using Zenject;

namespace BeatSpeedrun.Installers
{
    internal class GameInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<FloatingTimerViewController>().AsSingle();
            Container.BindInterfacesAndSelfTo<FloatingTimerView>().FromNewComponentAsViewController().AsSingle();
        }
    }
}
