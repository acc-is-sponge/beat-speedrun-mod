using BeatSpeedrun.Source.Views;
using BeatSpeedrun.Views;
using HMUI;
using BeatSaberMarkupLanguage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace BeatSpeedrun.Source.Controllers
{
    internal class ModFlowCoordinator: FlowCoordinator
    {
        private MainFlowCoordinator _mainFlowCoordinator;
        private MainSettingsView _mainSettingsView;
        private TimerInGameViewController _timerInGameViewController;

        // Normal constructor with no Inject attribute would cause MainSettingsView to be null!
        [Inject]
        public void Construct(MainFlowCoordinator mainFlowCoordinator, MainSettingsView mainSettingsView, TimerInGameViewController timerInGameViewController)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _mainSettingsView = mainSettingsView;
            _timerInGameViewController = timerInGameViewController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation && addedToHierarchy)
            {
                Plugin.Log?.Info("Activating ModFlowCoordinator");
                SetTitle("BeetSpeedrun");
                ProvideInitialViewControllers(_mainSettingsView);
                showBackButton = true;
            }
            _timerInGameViewController.Enable();
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            base.BackButtonWasPressed(topViewController);
            _mainFlowCoordinator.DismissFlowCoordinator(this, null, ViewController.AnimationDirection.Vertical, false);
            _timerInGameViewController.Disable();
        }
    }
}
