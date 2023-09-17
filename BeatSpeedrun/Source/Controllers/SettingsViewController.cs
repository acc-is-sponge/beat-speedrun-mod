using System;
using System.Linq;
using System.Threading.Tasks;
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
            _view = new SettingsView(GetSegmentPp);
        }

        [UIValue("view")]
        private readonly SettingsView _view;

        private string _selectedRegulation;
        private Segment? _selectedSegment;

        private void Render()
        {
            // Speedrunning!
            if (_currentSpeedrunManager.Current is Speedrun speedrun)
            {
                var targetSegment = speedrun.Progress.TargetSegment;
                _selectedRegulation = speedrun.RegulationPath;
                _selectedSegment = targetSegment?.Segment;

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
                _view.SegmentDropdown.Reset(null);
                _view.RunInteractable = false;
                return;
            }

            var segmentOptions = Enum.GetValues(typeof(Segment)).Cast<Segment?>().ToList();
            segmentOptions.Insert(0, null);

            _view.DescriptionText = regulation.Description;
            _view.SegmentDropdown.Reset(segmentOptions, ref _selectedSegment);
            _view.RunInteractable = true;
        }

        private int GetSegmentPp(Segment? segment)
        {
            if (segment == null) return 0;
            var regulation = _regulationManager.GetAsync(_selectedRegulation);
            if (regulation.Status != TaskStatus.RanToCompletion) return 0;
            return regulation.Result.Rules.SegmentRequirements.GetValue(segment.Value);
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

        internal void SelectSegment(Segment? segment)
        {
            _selectedSegment = segment;
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
