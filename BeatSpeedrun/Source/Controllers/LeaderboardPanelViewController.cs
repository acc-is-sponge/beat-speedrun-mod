using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSpeedrun.Views;
using Zenject;

namespace BeatSpeedrun.Controllers
{
    [HotReload(RelativePathToLayout = "..\\Views\\LeaderboardPanel.bsml")]
    [ViewDefinition(LeaderboardPanelView.ResourceName)]
    internal class LeaderboardPanelViewController : BSMLAutomaticViewController, IInitializable
    {
        [UIValue("view")]
        private readonly LeaderboardPanelView _view = new LeaderboardPanelView();

        public void Initialize()
        {
            MockUp();
        }

        private void MockUp()
        {
            var theme = LeaderboardTheme.Running;
            _view.TitleText = theme.ReplaceRichText("<$p>Beat Speedrun");
        }
    }
}
