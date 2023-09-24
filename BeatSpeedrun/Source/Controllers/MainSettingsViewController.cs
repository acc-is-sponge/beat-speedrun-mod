using BeatSaberMarkupLanguage.MenuButtons;
using BeatSaberMarkupLanguage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;
using HMUI;
using BeatSpeedrun.Source.Views;

namespace BeatSpeedrun.Source.Controllers
{
    internal class MainSettingsViewController: IInitializable, IDisposable
    {
        private readonly MenuButton menuButton;
        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly ModFlowCoordinator _modFlowCoordinator;

        public MainSettingsViewController(MainFlowCoordinator mainFlowCoordinator, ModFlowCoordinator modFlowCoordinator)
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
            Plugin.Log?.Info("Summoning ModFlowCoordinator");
            _mainFlowCoordinator.PresentFlowCoordinator(_modFlowCoordinator);
        }
    }
}
