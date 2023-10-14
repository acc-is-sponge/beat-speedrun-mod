using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

namespace BeatSpeedrun.Views
{
    [ViewDefinition("BeatSpeedrun.Source.Views.ModSettings.bsml")]
    [HotReload(RelativePathToLayout = @".\ModSettings.bsml")]
    internal class ModSettingsView : BSMLAutomaticViewController
    {
        private bool modEnable = true;
        [UIValue("main-enabled")]
        public bool ModEnable
        {
            get => modEnable;
            set => modEnable = value;
        }

        private bool floatingtimerEnable = true;
        [UIValue("floating-timer-enabled")]
        public bool FloatingTimerEnable
        {
            get => floatingtimerEnable;
            set => floatingtimerEnable = value;
        }

        [UIAction("#post-parse")]
        protected void Parsed()
        {
        }
    }
}
