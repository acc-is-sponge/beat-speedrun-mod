using BeatSaberMarkupLanguage.Attributes;
using BeatSpeedrun.Extensions;

namespace BeatSpeedrun.Views
{
    internal class LeaderboardPanelView : BSMLView
    {
        internal const string ResourceName = "BeatSpeedrun.Source.Views.LeaderboardPanel.bsml";

        private string _iconColor;

        [UIValue("icon-color")]
        internal string IconColor
        {
            get => _iconColor;
            set => ChangeProperty(ref _iconColor, value);
        }
    }
}

