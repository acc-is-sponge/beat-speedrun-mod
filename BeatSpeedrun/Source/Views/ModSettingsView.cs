using BeatSaberMarkupLanguage.Attributes;
using BeatSpeedrun.Extensions;

namespace BeatSpeedrun.Views
{
    internal class ModSettingsView : BSMLView
    {
        internal const string ResourceName = "BeatSpeedrun.Source.Views.ModSettings.bsml";

        private bool _showFloatingTimer;

        [UIValue("show-floating-timer")]
        public bool ShowFloatingTimer
        {
            get => _showFloatingTimer;
            set => ChangeProperty(ref _showFloatingTimer, value);
        }
    }
}
