using BeatSpeedrun.Source.Views;
using BeatSpeedrun.Views;
using HMUI;
using BeatSaberMarkupLanguage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSpeedrun.Source.Controllers
{
    internal class ModFlowCoordinator: FlowCoordinator
    {
        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private MainSettingsView _mainSettingsView;

        public ModFlowCoordinator(MainFlowCoordinator mainFlowCoordinator, MainSettingsView mainSettingsView)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _mainSettingsView = mainSettingsView;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if(_mainSettingsView == null)
            {
                Plugin.Log.Info("_mainSettingsView is null");
                _mainSettingsView = BeatSaberUI.CreateViewController<MainSettingsView>();
            }
            if (firstActivation && addedToHierarchy)
            {
                Plugin.Log?.Info("Activating ModFlowCoordinator");
                SetTitle("BeetSpeedrun");
                ProvideInitialViewControllers(_mainSettingsView);
                showBackButton = true;
            }
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            base.BackButtonWasPressed(topViewController);
            _mainFlowCoordinator.DismissFlowCoordinator(this, null, ViewController.AnimationDirection.Vertical, false);
        }
    }
}
