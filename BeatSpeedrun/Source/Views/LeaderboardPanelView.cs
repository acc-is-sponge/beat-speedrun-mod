using BeatSaberMarkupLanguage.Attributes;
using BeatSpeedrun.Extensions;

namespace BeatSpeedrun.Views
{
    internal class LeaderboardPanelView : BSMLView
    {
        internal const string ResourceName = "BeatSpeedrun.Source.Views.LeaderboardPanel.bsml";

        private string _titleText = "Beat Speedrun";

        [UIValue("title-text")]
        internal string TitleText
        {
            get => _titleText;
            set => ChangeProperty(ref _titleText, value);
        }

        // TODO: Change icons by currrent segment
    }
}

