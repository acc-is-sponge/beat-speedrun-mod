using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSpeedrun.Models.Speedrun;
using BeatSpeedrun.Services;
using BeatSpeedrun.Views;
using Zenject;

namespace BeatSpeedrun.Controllers
{
    [HotReload(RelativePathToLayout = "..\\Views\\LeaderboardPanel.bsml")]
    [ViewDefinition(LeaderboardPanelView.ResourceName)]
    internal class LeaderboardPanelViewController : BSMLAutomaticViewController, IInitializable, IDisposable
    {
        private SpeedrunFacilitator _speedrunFacilitator;

        [Inject]
        internal void Construct(SpeedrunFacilitator speedrunFacilitator)
        {
            _speedrunFacilitator = speedrunFacilitator;
        }

        [UIValue("view")]
        private readonly LeaderboardPanelView _view = new LeaderboardPanelView();

        private void Render()
        {
            var theme = LeaderboardTheme.FromSpeedrun(_speedrunFacilitator.Current);
            _view.IconSource = theme.IconSource;
            _view.IconColor = theme.IconColor;
        }

        void IInitializable.Initialize()
        {
            Render();
            _speedrunFacilitator.OnCurrentSpeedrunChanged += OnCurrentSpeedrunChanged;
            _speedrunFacilitator.OnCurrentSpeedrunUpdated += Render;
        }

        void IDisposable.Dispose()
        {
            _speedrunFacilitator.OnCurrentSpeedrunUpdated -= Render;
            _speedrunFacilitator.OnCurrentSpeedrunChanged -= OnCurrentSpeedrunChanged;
        }

        private void OnCurrentSpeedrunChanged((Speedrun, Speedrun) _) => Render();
    }
}
