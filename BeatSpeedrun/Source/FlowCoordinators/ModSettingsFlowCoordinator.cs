using BeatSaberMarkupLanguage;
using BeatSpeedrun.Controllers;
using HMUI;
using Zenject;

namespace BeatSpeedrun.FlowCoordinators
{
    internal class ModSettingsFlowCoordinator : FlowCoordinator
    {
        private MainFlowCoordinator _mainFlowCoordinator;
        private ModSettingsViewController _modSettingsViewController;

        [Inject]
        internal void Construct(MainFlowCoordinator mainFlowCoordinator, ModSettingsViewController modSettingsViewController)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _modSettingsViewController = modSettingsViewController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation && addedToHierarchy)
            {
                SetTitle("BeetSpeedrun");
                ProvideInitialViewControllers(_modSettingsViewController);
                showBackButton = true;
            }
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            base.BackButtonWasPressed(topViewController);
            _mainFlowCoordinator.DismissFlowCoordinator(this, null, ViewController.AnimationDirection.Horizontal, false);
        }
    }
}
