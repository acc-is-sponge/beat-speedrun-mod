using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSpeedrun.Controllers;
using Zenject;

namespace BeatSpeedrun.Registerers
{
    internal class TabRegisterer : IInitializable, IDisposable
    {
        internal const string TabName = "Beat Speedrun";

        private readonly StartStopViewController _startStopViewController;

        internal TabRegisterer(StartStopViewController startStopViewController)
        {
            _startStopViewController = startStopViewController;
        }

        void IInitializable.Initialize()
        {
            GameplaySetup.instance.AddTab(TabName, StartStopViewController.TabResource, _startStopViewController);
        }

        void IDisposable.Dispose()
        {
            if (GameplaySetup.instance != null && BSMLParser.instance != null)
            {
                GameplaySetup.instance.RemoveTab(TabName);
            }
        }
    }
}
