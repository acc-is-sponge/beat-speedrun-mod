using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using BeatSpeedrun.FlowCoordinators;
using System;
using Zenject;

namespace BeatSpeedrun.Registerers
{
    internal class ModSettingsRegisterer : IInitializable, IDisposable
    {
        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly ModSettingsFlowCoordinator _modSettingsFlowCoordinator;
        private readonly MenuButton _menuButton;

        public ModSettingsRegisterer(
            MainFlowCoordinator mainFlowCoordinator,
            ModSettingsFlowCoordinator modSettingsFlowCoordinator)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _modSettingsFlowCoordinator = modSettingsFlowCoordinator;
            _menuButton = new MenuButton("Beat Speedrun", SummonFlowCoordinator);
        }


        void IInitializable.Initialize()
        {
            MenuButtons.instance.RegisterButton(_menuButton);
        }

        void IDisposable.Dispose()
        {
            if (MenuButtons.instance != null && BSMLParser.instance != null)
            {
                MenuButtons.instance.UnregisterButton(_menuButton);
            }
        }

        private void SummonFlowCoordinator()
        {
            _mainFlowCoordinator.PresentFlowCoordinator(_modSettingsFlowCoordinator);
        }
    }
}
