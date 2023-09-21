using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSpeedrun.Extensions;
using BeatSpeedrun.Providers;
using BeatSpeedrun.Models;
using BeatSpeedrun.Models.Speedrun;
using BeatSpeedrun.Services;
using BeatSpeedrun.Views;
using BeatSpeedrun.Controllers.Support;
using Zenject;

namespace BeatSpeedrun.Controllers
{
    internal class SettingsViewController : IInitializable, IDisposable
    {
        // Since this class cannot be directly derived from BSMLViewController,
        // it is redefined for SettingsRegisterer
        internal const string TabResource = SettingsView.ResourceName;

        private readonly RegulationProvider _regulationProvider;
        private readonly SpeedrunFacilitator _speedrunFacilitator;
        private readonly SelectionState _selectionState;
        private readonly TaskWaiter _taskWaiter;

        public SettingsViewController(
            RegulationProvider regulationProvider,
            SpeedrunFacilitator speedrunFacilitator,
            SelectionState selectionState)
        {
            _regulationProvider = regulationProvider;
            _speedrunFacilitator = speedrunFacilitator;
            _selectionState = selectionState;
            _taskWaiter = new TaskWaiter(Render);
            _view = new SettingsView(GetSegmentPp);
        }

        private int GetSegmentPp(Segment? segment)
        {
            if (segment == null) return 0;
            var regulation = _regulationProvider.GetAsync(_selectionState.SelectedRegulation);
            if (regulation.Status != TaskStatus.RanToCompletion) return 0;
            return regulation.Result.Rules.SegmentRequirements.GetValue(segment.Value);
        }

        #region hooks

        private List<string> UseRegulationOptions() =>
            _taskWaiter.Wait(_regulationProvider.GetOptionsAsync());

        private Regulation UseRegulation(string regulation) =>
            _taskWaiter.Wait(_regulationProvider.GetAsync(regulation));

        #endregion

        #region render

        [UIValue("view")]
        private readonly SettingsView _view;

        private void Render()
        {
            // Speedrunning!
            if (_speedrunFacilitator.Current is Speedrun speedrun)
            {
                var targetSegment = speedrun.Progress.TargetSegment;
                _selectionState.SelectedRegulation = speedrun.RegulationPath;
                _selectionState.SelectedSegment = targetSegment?.Segment;

                _view.DescriptionText = speedrun.Regulation.Description;
                _view.RegulationDropdown.Reset(_selectionState.SelectedRegulation);
                _view.SegmentDropdown.Reset(_selectionState.SelectedSegment);
                _view.IsRunning = true;
                _view.RunInteractable = !_speedrunFacilitator.IsLoading;
                return;
            }

            var regulationOptions = UseRegulationOptions();
            if (_speedrunFacilitator.IsLoading || regulationOptions == null)
            {
                _view.DescriptionText = "loading...";
                _view.RegulationDropdown.Interactable = false;
                _view.SegmentDropdown.Interactable = false;
                _view.IsRunning = false;
                _view.RunInteractable = false;
                return;
            }

            _selectionState.SelectedRegulation =
                _view.RegulationDropdown.Reset(regulationOptions, _selectionState.SelectedRegulation);
            _view.IsRunning = false;

            var regulation = UseRegulation(_selectionState.SelectedRegulation);
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
            _selectionState.SelectedSegment =
                _view.SegmentDropdown.Reset(segmentOptions, _selectionState.SelectedSegment);
            _view.RunInteractable = true;
        }

        #endregion

        #region callbacks

        void IInitializable.Initialize()
        {
            Render();
            _speedrunFacilitator.OnCurrentSpeedrunChanged += Render;
            _speedrunFacilitator.OnSpeedrunLoadingStateChanged += Render;
            _selectionState.OnRegulationSelected += Render;
            _selectionState.OnSegmentSelected += Render;
            _view.RegulationDropdown.OnSelected += OnSelectRegulation;
            _view.SegmentDropdown.OnSelected += OnSelectSegment;
            _view.OnStarted += OnStartAsync;
            _view.OnStopped += OnStop;
        }

        void IDisposable.Dispose()
        {
            _taskWaiter.Dispose();
            _view.OnStopped -= OnStop;
            _view.OnStarted -= OnStartAsync;
            _view.SegmentDropdown.OnSelected -= OnSelectSegment;
            _view.RegulationDropdown.OnSelected -= OnSelectRegulation;
            _selectionState.OnSegmentSelected -= Render;
            _selectionState.OnRegulationSelected -= Render;
            _speedrunFacilitator.OnSpeedrunLoadingStateChanged -= Render;
            _speedrunFacilitator.OnCurrentSpeedrunChanged -= Render;
        }

        private void OnSelectRegulation(string regulation)
        {
            _selectionState.SelectedRegulation = regulation;
        }

        private void OnSelectSegment(Segment? segment)
        {
            _selectionState.SelectedSegment = segment;
        }

        private async void OnStartAsync()
        {
            try
            {
                await _speedrunFacilitator.StartAsync(
                    _selectionState.SelectedRegulation,
                    _selectionState.SelectedSegment);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Failed to start speedrun:\n{ex}");
            }
        }

        private void OnStop()
        {
            _speedrunFacilitator.Stop();
        }

        #endregion
    }
}
