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
        private readonly SelectionStateManager _selectionStateManager;
        private readonly TaskWaiter _taskWaiter;

        public SettingsViewController(
            RegulationManager regulationManager,
            CurrentSpeedrunManager currentSpeedrunManager,
            SelectionStateManager selectionStateManager)
        {
            _regulationManager = regulationManager;
            _currentSpeedrunManager = currentSpeedrunManager;
            _selectionStateManager = selectionStateManager;
            _taskWaiter = new TaskWaiter(Render);
            _view = new SettingsView(GetSegmentPp);
        }

        [UIValue("view")]
        private readonly SettingsView _view;

        private void Render()
        {
            // Speedrunning!
            if (_currentSpeedrunManager.Current is Speedrun speedrun)
            {
                var targetSegment = speedrun.Progress.TargetSegment;
                _selectionStateManager.SelectedRegulation = speedrun.RegulationPath;
                _selectionStateManager.SelectedSegment = targetSegment?.Segment;

                _view.DescriptionText = speedrun.Regulation.Description;
                _view.RegulationDropdown.Reset(_selectionStateManager.SelectedRegulation);
                _view.SegmentDropdown.Reset(_selectionStateManager.SelectedSegment);
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

            _selectionStateManager.SelectedRegulation =
                _view.RegulationDropdown.Reset(regulationOptions, _selectionStateManager.SelectedRegulation);
            _view.IsRunning = false;

            var regulation = _taskWaiter.Wait(
                _regulationManager.GetAsync(_selectionStateManager.SelectedRegulation));
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
            _selectionStateManager.SelectedSegment =
                _view.SegmentDropdown.Reset(segmentOptions, _selectionStateManager.SelectedSegment);
            _view.RunInteractable = true;
        }

        private int GetSegmentPp(Segment? segment)
        {
            if (segment == null) return 0;
            var regulation = _regulationManager.GetAsync(_selectionStateManager.SelectedRegulation);
            if (regulation.Status != TaskStatus.RanToCompletion) return 0;
            return regulation.Result.Rules.SegmentRequirements.GetValue(segment.Value);
        }

        public override void Initialize()
        {
            base.Initialize();
            _currentSpeedrunManager.OnCurrentSpeedrunChanged += Render;
            _currentSpeedrunManager.OnSpeedrunLoadingStateChanged += Render;
            _selectionStateManager.OnRegulationSelected += Render;
            _selectionStateManager.OnSegmentSelected += Render;
            _view.RegulationDropdown.OnSelected += OnSelectRegulation;
            _view.SegmentDropdown.OnSelected += OnSelectSegment;
            _view.OnStarted += OnStartAsync;
            _view.OnStopped += OnStop;
            Render();
        }

        public override void Dispose()
        {
            _taskWaiter.Dispose();
            _view.OnStopped -= OnStop;
            _view.OnStarted -= OnStartAsync;
            _view.SegmentDropdown.OnSelected -= OnSelectSegment;
            _view.RegulationDropdown.OnSelected -= OnSelectRegulation;
            _selectionStateManager.OnSegmentSelected -= Render;
            _selectionStateManager.OnRegulationSelected -= Render;
            _currentSpeedrunManager.OnSpeedrunLoadingStateChanged -= Render;
            _currentSpeedrunManager.OnCurrentSpeedrunChanged -= Render;
            base.Dispose();
        }

        private void OnSelectRegulation(string regulation)
        {
            _selectionStateManager.SelectedRegulation = regulation;
        }

        private void OnSelectSegment(Segment? segment)
        {
            _selectionStateManager.SelectedSegment = segment;
        }

        private async void OnStartAsync()
        {
            try
            {
                await _currentSpeedrunManager.StartAsync(
                    _selectionStateManager.SelectedRegulation,
                    _selectionStateManager.SelectedSegment);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Failed to start speedrun:\n{ex}");
            }
        }

        private void OnStop()
        {
            _currentSpeedrunManager.Stop();
        }
    }
}
