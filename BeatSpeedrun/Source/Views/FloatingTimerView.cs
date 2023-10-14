using BeatSaberMarkupLanguage.Attributes;
using BeatSpeedrun.Extensions;

namespace BeatSpeedrun.Views
{
    internal class FloatingTimerView : BSMLView
    {
        internal const string ResourceName = "BeatSpeedrun.Source.Views.FloatingTimer.bsml";

        private string _timerText;

        [UIValue("timer-text")]
        internal string TimerText
        {
            get => _timerText;
            set => ChangeProperty(ref _timerText, value);
        }
    }
}
