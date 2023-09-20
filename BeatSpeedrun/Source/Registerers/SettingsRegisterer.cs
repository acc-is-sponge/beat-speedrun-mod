using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSpeedrun.Controllers;
using Zenject;

namespace BeatSpeedrun.Registerers
{
    internal class SettingsRegisterer : IInitializable, IDisposable
    {
        internal const string TabName = "Beat Speedrun";

        private readonly SettingsViewController _settingsViewController;

        internal SettingsRegisterer(SettingsViewController settingsViewController)
        {
            _settingsViewController = settingsViewController;
        }

        void IInitializable.Initialize()
        {
            GameplaySetup.instance.AddTab(TabName, SettingsViewController.TabResource, _settingsViewController);
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
