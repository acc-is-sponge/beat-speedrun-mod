using BeatSaberMarkupLanguage;
using BeatSpeedrun.Views;
using HMUI;
using Zenject;

namespace BeatSpeedrun.Controllers
{
    internal class ModFlowCoordinator : FlowCoordinator
    {
        private MainFlowCoordinator _mainFlowCoordinator;
        private ModSettingsView _modSettingsView;

        [Inject]
        internal void Construct(MainFlowCoordinator mainFlowCoordinator, ModSettingsView modSettingsView)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _modSettingsView = modSettingsView;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation && addedToHierarchy)
            {
                SetTitle("BeetSpeedrun");
                ProvideInitialViewControllers(_modSettingsView);
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
