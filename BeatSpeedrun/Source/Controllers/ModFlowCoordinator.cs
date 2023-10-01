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
        private FloatingTimerViewController _floatingTimerViewController;

        // Normal constructor with no Inject attribute would cause MainSettingsView to be null!
        [Inject]
        public void Construct(MainFlowCoordinator mainFlowCoordinator, MainSettingsView mainSettingsView, FloatingTimerViewController floatingTimerViewController)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _mainSettingsView = mainSettingsView;
            _floatingTimerViewController = floatingTimerViewController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation && addedToHierarchy)
            {
                SetTitle("BeetSpeedrun");
                ProvideInitialViewControllers(_mainSettingsView);
                showBackButton = true;
            }
            _floatingTimerViewController.Display();
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            base.BackButtonWasPressed(topViewController);
            _mainFlowCoordinator.DismissFlowCoordinator(this, null, ViewController.AnimationDirection.Vertical, false);
            _floatingTimerViewController.Hide();
        }
    }
}
