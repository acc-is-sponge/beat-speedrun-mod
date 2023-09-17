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
        private (Segment?, int) _selectedSegment;

        private void Render()
        {
            // Speedrunning!
            if (_currentSpeedrunManager.Current is Speedrun speedrun)
            {
                var targetSegment = speedrun.Progress.TargetSegment;
                _selectedRegulation = speedrun.RegulationPath;
                _selectedSegment = (targetSegment?.Segment, targetSegment?.RequiredPp ?? 0);

                _view.DescriptionText = speedrun.Regulation.Description;
                _view.RegulationDropdown.Reset(_selectedRegulation);
                _view.SegmentDropdown.Reset(_selectedSegment);
                _view.IsRunning = true;
                _view.RunInteractable = !_currentSpeedrunManager.IsLoading;
                return;
            }

            var regulationOptions = _taskWaiter
                .Wait(_regulationManager.GetOptionsAsync())?
                .ToList();

            if (_currentSpeedrunManager.IsLoading || regulationOptions == null)
            {
                _view.DescriptionText = "loading...";
                _view.RegulationDropdown.Interactable = false;
                _view.SegmentDropdown.Interactable = false;
                _view.IsRunning = false;
                _view.RunInteractable = false;
                return;
            }

            _view.RegulationDropdown.Reset(regulationOptions, ref _selectedRegulation);
            _view.IsRunning = false;

            var regulation = _taskWaiter.Wait(_regulationManager.GetAsync(_selectedRegulation));
            if (regulation == null)
            {
                _view.DescriptionText = "loading...";
                _view.SegmentDropdown.Reset((null, 0));
                _view.RunInteractable = false;
                return;
            }

            var segmentOptions = Enum.GetValues(typeof(Segment))
                .Cast<Segment>()
                .Select(s => ((Segment?)s, regulation.Rules.SegmentRequirements.GetValue(s)))
                .ToList();
            segmentOptions.Sort((a, b) => a.Item2.CompareTo(b.Item2));
            segmentOptions.Insert(0, (null, 0));

            _view.DescriptionText = regulation.Description;
            _view.SegmentDropdown.Reset(segmentOptions, ref _selectedSegment);
            _view.RunInteractable = true;
        }

        public override void Initialize()
        {
            Render();
            base.Initialize();
            _currentSpeedrunManager.OnCurrentSpeedrunChanged += Render;
            _currentSpeedrunManager.OnSpeedrunLoadingStateChanged += Render;
            _view.RegulationDropdown.OnSelected += SelectRegulation;
            _view.SegmentDropdown.OnSelected += SelectSegment;
            _view.OnStarted += StartAsync;
            _view.OnStopped += Stop;
        }

        public override void Dispose()
        {
            _taskWaiter.Dispose();
            _view.OnStopped -= Stop;
            _view.OnStarted -= StartAsync;
            _view.SegmentDropdown.OnSelected -= SelectSegment;
            _view.RegulationDropdown.OnSelected -= SelectRegulation;
            _currentSpeedrunManager.OnSpeedrunLoadingStateChanged -= Render;
            _currentSpeedrunManager.OnCurrentSpeedrunChanged -= Render;
            base.Dispose();
        }

        internal void SelectRegulation(string regulation)
        {
            _selectedRegulation = regulation;
            Render();
        }

        internal void SelectSegment((Segment?, int) segment)
        {
            _selectedSegment = segment;
            Render();
        }

        internal async void StartAsync()
        {
            try
            {
                await _currentSpeedrunManager.StartAsync(
                    _selectedRegulation,
                    _selectedSegment.Item1);
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
