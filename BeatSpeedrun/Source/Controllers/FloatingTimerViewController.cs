using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSpeedrun.Extensions;
using BeatSpeedrun.Models.Speedrun;
using BeatSpeedrun.Services;
using BeatSpeedrun.Views;
using System;
using Zenject;

namespace BeatSpeedrun.Controllers
{
    [HotReload(RelativePathToLayout = @"..\Views\FloatingTimer.bsml")]
    [ViewDefinition(FloatingTimerView.ResourceName)]
    internal class FloatingTimerViewController : BSMLAutomaticViewController, ITickable
    {
        private SpeedrunFacilitator _speedrunFacilitator;

        [Inject]
        internal void Construct(SpeedrunFacilitator speedrunFacilitator)
        {
            _speedrunFacilitator = speedrunFacilitator;
        }

        #region render

        [UIValue("view")]
        private readonly FloatingTimerView _view = new FloatingTimerView();

        private void Render()
        {
            if (!PluginConfig.Instance.FloatingTimerEnabled)
            {
                _view.TimerText = "";
                return;
            }

            var speedrun = _speedrunFacilitator.Current;
            var now = DateTime.UtcNow;
            switch (speedrun?.Progress.ComputeState(now) ?? Progress.State.Finished)
            {
                case Progress.State.Running:
                case Progress.State.TimeIsUp:
                    var theme = LeaderboardTheme.FromSpeedrun(speedrun);
                    var time = speedrun.Progress.ElapsedTime(now);
                    var text =
                        $"<line-height=60%><$icon>{time.ToTimerString()}" +
                        (speedrun.Progress.Current is Progress.SegmentProgress p ? $"\n{p.Segment}" : "") +
                        $"\n{speedrun.TotalPp:0.#}pp";
                    _view.TimerText = theme.ReplaceRichText(text);
                    break;
                default:
                    _view.TimerText = "";
                    break;
            }
        }

        #endregion

        #region callbacks

        void ITickable.Tick() => Render();

        #endregion
    }
}
