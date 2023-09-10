using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSpeedrun.Extensions;
using BeatSpeedrun.Models;
using HMUI;

namespace BeatSpeedrun.Views
{
    internal class SettingsView : BSMLView
    {
        internal const string ResourceName = "BeatSpeedrun.Source.Views.Settings.bsml";

        internal event Action<RegulationOption> OnRegulaionSelected;
        internal event Action<SegmentOption> OnSegmentSelected;
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

        [UIComponent("regulation-dropdown")]
        private readonly DropDownListSetting _regulationDropdown;

        private RegulationOption _regulationDropdownValue = RegulationOption.Loading;

        [UIValue("regulation-dropdown-value")]
        internal RegulationOption RegulationDropdownValue
        {
            get => _regulationDropdownValue;
            set => ChangeProperty(ref _regulationDropdownValue, value, _regulationDropdown);
        }

        [UIValue("regulation-dropdown-choices")]
        private readonly List<object> _regulationDropdownChoices =
            new List<object>() { RegulationOption.Loading };

        internal void ReplaceRegulationDropdownChoices(IEnumerable<RegulationOption> options)
        {
            _regulationDropdownChoices.Clear();
            _regulationDropdownChoices.AddRange(options);
            if (_regulationDropdown != null)
            {
                _regulationDropdown.values = _regulationDropdownChoices;
                _regulationDropdown.UpdateChoices();
            }
        }

        [UIAction("regulation-selected")]
        private void RegulationSelected(RegulationOption option)
        {
            try
            {
                OnRegulaionSelected?.Invoke(option);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Error while invoking RegulationSelected event:\n{ex}");
            }
        }

        private bool _regulationDropdownInteractable;

        [UIValue("regulation-dropdown-interactable")]
        internal bool RegulationDropdownInteractable
        {
            get => _regulationDropdownInteractable;
            set => ChangeProperty(ref _regulationDropdownInteractable, value);
        }

        [UIComponent("segment-dropdown")]
        private readonly DropDownListSetting _segmentDropdown;

        private SegmentOption _segmentDropdownValue = SegmentOption.Endless;

        [UIValue("segment-dropdown-value")]
        internal SegmentOption SegmentDropdownValue
        {
            get => _segmentDropdownValue;
            set => ChangeProperty(ref _segmentDropdownValue, value, _segmentDropdown);
        }

        [UIValue("segment-dropdown-choices")]
        private readonly List<object> _segmentDropdownChoices =
            new List<object>() { SegmentOption.Endless };

        internal void ReplaceSegmentDropdownChoices(IEnumerable<SegmentOption> options)
        {
            _segmentDropdownChoices.Clear();
            _segmentDropdownChoices.AddRange(options);
            if (_segmentDropdown != null)
            {
                _segmentDropdown.values = _segmentDropdownChoices;
                _segmentDropdown.UpdateChoices();
            }
        }

        [UIAction("segment-selected")]
        private void SegmentSelected(SegmentOption option)
        {
            try
            {
                OnSegmentSelected?.Invoke(option);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Error while invoking RegulationSelected event:\n{ex}");
            }
        }

        private bool _segmentDropdownInteractable;

        [UIValue("segment-dropdown-interactable")]
        internal bool SegmentDropdownInteractable
        {
            get => _segmentDropdownInteractable;
            set => ChangeProperty(ref _segmentDropdownInteractable, value);
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

        internal class RegulationOption
        {
            internal string Id { get; }
            internal string Label { get; }

            internal RegulationOption(string id)
            {
                Id = id;
                Label = ToLabel(id);
            }

            public override string ToString() => Label;

            private static string ToLabel(string id)
            {
                // NOTE: Perhaps there is a better way
                try
                {
                    var uri = new Uri(id);
                    id = uri.LocalPath;
                }
                catch
                {
                    // ignored
                }
                return id.EndsWith(".json") ? id.Substring(0, id.Length - 5) : id;
            }

            internal static readonly RegulationOption Loading = new RegulationOption("...");
        }

        internal class SegmentOption
        {
            internal Segment? Segment { get; }
            internal float Pp { get; }

            internal SegmentOption(Segment? segment, float pp)
            {
                Segment = segment;
                Pp = pp;
            }

            public override string ToString() =>
                Segment is Segment s ? $"{s} / {Pp}pp" : "endless";

            internal static readonly SegmentOption Endless = new SegmentOption(null, 0);
        }
    }
}
