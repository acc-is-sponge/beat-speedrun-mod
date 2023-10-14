using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using System;
using Zenject;

namespace BeatSpeedrun.Controllers
{
    internal class ModSettingsViewController : IInitializable, IDisposable
    {
        private readonly MenuButton menuButton;
        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly ModFlowCoordinator _modFlowCoordinator;

        public ModSettingsViewController(MainFlowCoordinator mainFlowCoordinator, ModFlowCoordinator modFlowCoordinator)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _modFlowCoordinator = modFlowCoordinator;
            menuButton = new MenuButton("BeatSpeedrun", SummonFlowCoordinator);
        }

        public void Initialize()
        {
            MenuButtons.instance.RegisterButton(menuButton);
        }

        public void Dispose()
        {
            if (MenuButtons.instance != null && BSMLParser.instance != null)
            {
                MenuButtons.instance.UnregisterButton(menuButton);
            }
        }

        private void SummonFlowCoordinator()
        {
            _mainFlowCoordinator.PresentFlowCoordinator(_modFlowCoordinator);
        }
    }
}
