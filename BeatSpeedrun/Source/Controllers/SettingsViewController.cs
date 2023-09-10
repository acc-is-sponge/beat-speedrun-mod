using BeatSaberMarkupLanguage.Attributes;
using BeatSpeedrun.Extensions;
using BeatSpeedrun.Views;

namespace BeatSpeedrun.Controllers
{
    internal class SettingsViewController : BSMLTabViewController
    {
        protected override string TabName => "Beat Speedrun";
        protected override string TabResource => SettingsView.ResourceName;

        [UIValue("view")]
        private readonly SettingsView _view = new SettingsView();

        public override void Initialize()
        {
            MockUp();
            base.Initialize();
        }

        private void MockUp()
        {
            var regulationOptions = new[]
            {
                new SettingsView.RegulationOption("scoresaber/092023.json"),
            };
            var segmentOptions = new[]
            {
                SettingsView.SegmentOption.NoSegment,
            };

            _view.ReplaceRegulationDropdownChoices(regulationOptions);
            _view.RegulationDropdownValue = regulationOptions[0];
            _view.RegulationDropdownInteractable = true;
            _view.IsRunning = true;
            _view.DescriptionText = "Description of the regulation";
            _view.ReplaceSegmentDropdownChoices(segmentOptions);
            _view.SegmentDropdownValue = segmentOptions[0];
            _view.SegmentDropdownInteractable = true;
            _view.RunInteractable = true;
        }
    }
}
