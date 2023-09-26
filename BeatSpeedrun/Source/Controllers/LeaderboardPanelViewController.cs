using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSpeedrun.Services;
using BeatSpeedrun.Views;
using Zenject;

namespace BeatSpeedrun.Controllers
{
    [HotReload(RelativePathToLayout = "..\\Views\\LeaderboardPanel.bsml")]
    [ViewDefinition(LeaderboardPanelView.ResourceName)]
    internal class LeaderboardPanelViewController : BSMLAutomaticViewController, IInitializable, IDisposable
    {
        [Inject]
        private readonly SpeedrunFacilitator _speedrunFacilitator;

        [UIValue("view")]
        private readonly LeaderboardPanelView _view = new LeaderboardPanelView();

        private void Render()
        {
            var speedrun = _speedrunFacilitator.Current;
            var theme = speedrun != null
                ? LeaderboardTheme.FromSegment(speedrun.Progress.Current.Segment)
                : LeaderboardTheme.NotRunning;
            _view.IconSource = theme.IconSource;
            _view.IconColor = theme.IconColor;
        }

        public void Initialize()
        {
            Render();
            _speedrunFacilitator.OnCurrentSpeedrunChanged += Render;
            _speedrunFacilitator.OnCurrentSpeedrunUpdated += Render;
        }

        public void Dispose()
        {
            _speedrunFacilitator.OnCurrentSpeedrunUpdated -= Render;
            _speedrunFacilitator.OnCurrentSpeedrunChanged -= Render;
        }
    }
}
