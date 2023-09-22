using BeatSpeedrun.Providers;
using BeatSpeedrun.Repositories;
using BeatSpeedrun.Services;
using BeatSpeedrun.Source.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace BeatSpeedrun.Installers
{
    internal class GameInstaller: Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<TimerInGame>().AsSingle();
            Container.BindInterfacesAndSelfTo<TimerInGameView>().FromNewComponentAsViewController().AsSingle();
        }
    }
}
