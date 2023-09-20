using System;
using BeatSpeedrun.Models;

namespace BeatSpeedrun.Services
{
    internal class SelectionState
    {
        internal event Action OnRegulationSelected;
        internal event Action OnSegmentSelected;

        internal SelectionState()
        {
            _selectedRegulation = PluginConfig.Instance.LatestSelectedRegulation;
            _selectedSegment =
                Enum.TryParse<Segment>(PluginConfig.Instance.LatestSelectedSegment, out var s)
                    ? (Segment?)s
                    : null;
        }

        private string _selectedRegulation;

        internal string SelectedRegulation
        {
            get => _selectedRegulation;
            set
            {
                if (_selectedRegulation == value) return;
                _selectedRegulation = value;
                PluginConfig.Instance.LatestSelectedRegulation = value;
                PluginConfig.Instance.Changed();
                try
                {
                    OnRegulationSelected?.Invoke();
                }
                catch (Exception ex)
                {
                    Plugin.Log.Error($"Error invoking OnRegulationSelected:\n{ex}");
                }
            }
        }

        private Segment? _selectedSegment;

        internal Segment? SelectedSegment
        {
            get => _selectedSegment;
            set
            {
                if (_selectedSegment == value) return;
                _selectedSegment = value;
                PluginConfig.Instance.LatestSelectedSegment = value?.ToString();
                PluginConfig.Instance.Changed();
                try
                {
                    OnSegmentSelected?.Invoke();
                }
                catch (Exception ex)
                {
                    Plugin.Log.Error($"Error invoking OnSegmentSelected:\n{ex}");
                }
            }
        }
    }
}
