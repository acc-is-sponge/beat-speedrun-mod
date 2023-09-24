using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using System.Collections.Generic;


namespace BeatSpeedrun.Source.Views
{
    [HotReload(RelativePathToLayout = @"MainSettings.bsml")]
    [ViewDefinition("BeatSpeedrun.Source.Views.MainSettings.bsml")]
    internal class MainSettingsView : BSMLAutomaticViewController
    {
        private bool modEnable = true;
        [UIValue("ModEnable")]
        public bool ModEnable
        {
            get => modEnable;
            set => modEnable = value;
        }
    }
}
