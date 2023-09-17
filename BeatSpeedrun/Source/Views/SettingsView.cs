using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSpeedrun.Extensions;
using BeatSpeedrun.Models;
using HMUI;

namespace BeatSpeedrun.Views
{
    internal class SettingsView : BSMLView
    {
        internal const string ResourceName = "BeatSpeedrun.Source.Views.Settings.bsml";

        internal event Action OnStarted;
        internal event Action OnStopped;

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
        internal BSMLDropdownView<string> RegulationDropdown { get; } =
            new BSMLDropdownView<string>(Regulation.ShortenPath);

        [UIValue("segment-dropdown")]
        internal BSMLDropdownView<(Segment?, int)> SegmentDropdown { get; } =
            new BSMLDropdownView<(Segment?, int)>(SegmentSummary);

        private static string SegmentSummary((Segment?, int) info)
        {
            return info.Item1 is Segment s ? $"{s} / {info.Item2}pp" : "endless";
        }

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
            "\nThe speedrun records are stored, but the feature" +
            "\nto view them later has not yet been implemented." +
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
    }
}
