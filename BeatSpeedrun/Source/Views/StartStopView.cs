using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSpeedrun.Extensions;
using BeatSpeedrun.Models;
using HMUI;

namespace BeatSpeedrun.Views
{
    internal class StartStopView : BSMLView
    {
        internal const string ResourceName = "BeatSpeedrun.Source.Views.StartStop.bsml";

        internal event Action OnStarted;
        internal event Action OnStopped;

        public StartStopView(Func<Segment?, int> getSegmentPp)
        {
            RegulationDropdown = new BSMLDropdownView<string>(Regulation.ShortenPath);
            SegmentDropdown = new BSMLDropdownView<Segment?>(SegmentSummary(getSegmentPp));
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            UpdateRunText();
        }

        private string _descriptionText;

        [UIValue("description-text")]
        internal string DescriptionText
        {
            get => _descriptionText;
            set => ChangeProperty(ref _descriptionText, value);
        }

        [UIValue("regulation-dropdown")]
        internal BSMLDropdownView<string> RegulationDropdown { get; }

        [UIValue("segment-dropdown")]
        internal BSMLDropdownView<Segment?> SegmentDropdown { get; }

        private bool _isRunning;

        internal bool IsRunning
        {
            get => _isRunning;
            set => ChangeProperty(ref _isRunning, value, UpdateRunText);
        }

        private string _runText;

        [UIValue("run-text")]
        private string RunText
        {
            get => _runText;
            set => ChangeProperty(ref _runText, value);
        }

        private void UpdateRunText()
        {
            RunText = _isRunning ? "Stop" : "Start!";
        }

        [UIAction("run-clicked")]
        private void RunClicked()
        {
            if (_isRunning)
            {
                _confirmStopModal.Show(true, true);
            }
            else
            {
                try
                {
                    OnStarted?.Invoke();
                }
                catch (Exception ex)
                {
                    Plugin.Log.Error($"Error while invoking RegulationSelected event:\n{ex}");
                }
            }
        }

        private bool _runInteractable;

        [UIValue("run-interactable")]
        internal bool RunInteractable
        {
            get => _runInteractable;
            set => ChangeProperty(ref _runInteractable, value);
        }

        [UIComponent("confirm-stop-modal")]
        private readonly ModalView _confirmStopModal;

        [UIValue("confirm-stop-text")]
        private readonly string _confirmStopText =
            "Are you sure you want to stop the speedrun?" +
            "\n\n<#ff7777>You cannot continue on this speedrun!";

        [UIAction("confirm-stop-clicked")]
        private void ConfirmStopClicked()
        {
            try
            {
                OnStopped?.Invoke();
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Error while invoking RegulationSelected event:\n{ex}");
            }
            _confirmStopModal.Hide(true);
        }

        [UIAction("cancel-stop-clicked")]
        private void CancelStopClicked()
        {
            _confirmStopModal.Hide(true);
        }

        private Func<Segment?, string> SegmentSummary(Func<Segment?, int> getPp)
        {
            return segment => segment is Segment s ? $"{s} / {getPp(s)}pp" : "endless";
        }
    }
}
