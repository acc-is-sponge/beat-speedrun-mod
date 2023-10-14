using BeatSaberMarkupLanguage;
using BeatSpeedrun.Views;
using HMUI;
using Zenject;

namespace BeatSpeedrun.Controllers
{
    internal class ModFlowCoordinator : FlowCoordinator
    {
        private MainFlowCoordinator _mainFlowCoordinator;
        private MainSettingsView _mainSettingsView;

        [Inject]
        internal void Construct(MainFlowCoordinator mainFlowCoordinator, MainSettingsView mainSettingsView)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _mainSettingsView = mainSettingsView;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation && addedToHierarchy)
            {
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
