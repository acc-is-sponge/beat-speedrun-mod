using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSpeedrun.Managers;
using BeatSpeedrun.Models.Speedrun;
using BeatSpeedrun.Views;
using Zenject;

namespace BeatSpeedrun.Controllers
{
    [HotReload(RelativePathToLayout = "..\\Views\\LeaderboardPanel.bsml")]
    [ViewDefinition(LeaderboardPanelView.ResourceName)]
    internal class LeaderboardPanelViewController : BSMLAutomaticViewController
    {
        [Inject]
        private readonly CurrentSpeedrunManager _currentSpeedrunManager;

        [UIValue("view")]
        private readonly LeaderboardPanelView _view = new LeaderboardPanelView();

        private LeaderboardTheme CurrentTheme =>
            _currentSpeedrunManager.Current is Speedrun speedrun
                ? LeaderboardTheme.FromSegment(speedrun.Progress.GetCurrentSegment().Segment)
                : LeaderboardTheme.NotRunning;

        private void Render()
        {
            var theme = CurrentTheme;
            _view.TitleText = theme.ReplaceRichText("<$p>Beat Speedrun");
        }

        public void Initialize()
        {
            Render();
            _currentSpeedrunManager.OnCurrentSpeedrunChanged += Render;
            _currentSpeedrunManager.OnCurrentSpeedrunUpdated += Render;
        }

        public void Dispose()
        {
            _currentSpeedrunManager.OnCurrentSpeedrunUpdated -= Render;
            _currentSpeedrunManager.OnCurrentSpeedrunChanged -= Render;
        }
    }
}
