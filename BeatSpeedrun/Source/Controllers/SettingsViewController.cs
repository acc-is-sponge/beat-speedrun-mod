using System;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSpeedrun.Extensions;
using BeatSpeedrun.Managers;
using BeatSpeedrun.Models;
using BeatSpeedrun.Models.Speedrun;
using BeatSpeedrun.Views;

namespace BeatSpeedrun.Controllers
{
    internal class SettingsViewController : BSMLTabViewController
    {
        protected override string TabName => "Beat Speedrun";
        protected override string TabResource => SettingsView.ResourceName;

        private readonly RegulationManager _regulationManager;
        private readonly CurrentSpeedrunManager _currentSpeedrunManager;
        private readonly TaskWaiter _taskWaiter;

        public SettingsViewController(
            RegulationManager regulationManager,
            CurrentSpeedrunManager currentSpeedrunManager)
        {
            _regulationManager = regulationManager;
            _currentSpeedrunManager = currentSpeedrunManager;
            _taskWaiter = new TaskWaiter(Render);
        }

        [UIValue("view")]
        private readonly SettingsView _view = new SettingsView();

        private string _selectedRegulation;
        private Segment? _selectedSegment;

        private void Render()
        {
            SettingsView.RegulationOption selectedRegulationOption;
            SettingsView.SegmentOption selectedSegmentOption;

            // Speedrunning!
            if (_currentSpeedrunManager.Current is Speedrun speedrun)
            {
                selectedRegulationOption =
                    new SettingsView.RegulationOption(speedrun.RegulationId);
                selectedSegmentOption =
                    speedrun.Progress.TargetSegment is Progress.SegmentProgress s
                        ? new SettingsView.SegmentOption(s.Segment, s.RequiredPp)
                        : SettingsView.SegmentOption.NoSegment;
                _selectedRegulation = selectedRegulationOption.Id;
                _selectedSegment = selectedSegmentOption?.Segment;

                _view.DescriptionText = speedrun.Regulation.Description;
                _view.ReplaceRegulationDropdownChoices(new[] { selectedRegulationOption });
                _view.RegulationDropdownValue = selectedRegulationOption;
                _view.RegulationDropdownInteractable = false;
                _view.ReplaceSegmentDropdownChoices(new[] { selectedSegmentOption });
                _view.SegmentDropdownValue = selectedSegmentOption;
                _view.SegmentDropdownInteractable = false;
                _view.IsRunning = true;
                _view.RunInteractable = !_currentSpeedrunManager.IsLoading;
                return;
            }

            var regulationOptions = _taskWaiter
                .Wait(_regulationManager.GetOptionsAsync())?
                .Select(id => new SettingsView.RegulationOption(id))
                .ToList();

            if (_currentSpeedrunManager.IsLoading || regulationOptions == null)
            {
                _view.DescriptionText = "loading...";
                _view.RegulationDropdownInteractable = false;
                _view.SegmentDropdownInteractable = false;
                _view.IsRunning = false;
                _view.RunInteractable = false;
                return;
            }

            selectedRegulationOption = regulationOptions.FirstOrDefault(r => r.Id == _selectedRegulation);
            if (selectedRegulationOption == null)
            {
                selectedRegulationOption = regulationOptions[0];
                _selectedRegulation = selectedRegulationOption.Id;
            }

            _view.ReplaceRegulationDropdownChoices(regulationOptions);
            _view.RegulationDropdownValue = selectedRegulationOption;
            _view.RegulationDropdownInteractable = true;
            _view.IsRunning = false;

            var regulation = _taskWaiter.Wait(_regulationManager.GetAsync(_selectedRegulation));
            if (regulation == null)
            {
                _view.DescriptionText = "loading...";
                _view.ReplaceSegmentDropdownChoices(new[] { SettingsView.SegmentOption.NoSegment });
                _view.SegmentDropdownValue = SettingsView.SegmentOption.NoSegment;
                _view.SegmentDropdownInteractable = false;
                _view.RunInteractable = false;
                return;
            }

            var segmentOptions = Enum.GetValues(typeof(Segment))
                .Cast<Segment>()
                .Select(s => new SettingsView.SegmentOption(s, regulation.Rules.SegmentRequirements.GetValue(s)))
                .ToList();
            segmentOptions.Sort((a, b) => a.Pp.CompareTo(b.Pp));
            segmentOptions.Insert(0, SettingsView.SegmentOption.NoSegment);

            selectedSegmentOption = segmentOptions.FirstOrDefault(s => s.Segment == _selectedSegment);
            if (selectedSegmentOption == null)
            {
                selectedSegmentOption = segmentOptions[0];
                _selectedSegment = selectedSegmentOption.Segment;
            }

            _view.DescriptionText = regulation.Description;
            _view.ReplaceSegmentDropdownChoices(segmentOptions);
            _view.SegmentDropdownValue = selectedSegmentOption;
            _view.SegmentDropdownInteractable = true;
            _view.RunInteractable = true;
        }

        public override void Initialize()
        {
            Render();
            base.Initialize();
            _currentSpeedrunManager.OnCurrentSpeedrunChanged += Render;
            _currentSpeedrunManager.OnSpeedrunLoadingStateChanged += Render;
            _view.OnRegulaionSelected += SelectRegulation;
            _view.OnSegmentSelected += SelectSegment;
            _view.OnStarted += StartAsync;
            _view.OnStopped += Stop;
        }

        public override void Dispose()
        {
            _taskWaiter.Dispose();
            _view.OnStopped -= Stop;
            _view.OnStarted -= StartAsync;
            _view.OnSegmentSelected -= SelectSegment;
            _view.OnRegulaionSelected -= SelectRegulation;
            _currentSpeedrunManager.OnSpeedrunLoadingStateChanged -= Render;
            _currentSpeedrunManager.OnCurrentSpeedrunChanged -= Render;
            base.Dispose();
        }

        internal void SelectRegulation(SettingsView.RegulationOption option)
        {
            _selectedRegulation = option.Id;
            Render();
        }

        internal void SelectSegment(SettingsView.SegmentOption option)
        {
            _selectedSegment = option.Segment;
            Render();
        }

        internal async void StartAsync()
        {
            try
            {
                await _currentSpeedrunManager.StartAsync(_selectedRegulation, _selectedSegment);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Failed to start speedrun:\n{ex}");
            }
        }

        internal void Stop()
        {
            _currentSpeedrunManager.Stop();
        }
    }
}
