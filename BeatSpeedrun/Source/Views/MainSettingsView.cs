using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using System.Collections.Generic;
using Zenject;

namespace BeatSpeedrun.Source.Views
{
    [ViewDefinition("BeatSpeedrun.Source.Views.MainSettings.bsml")]
    [HotReload(RelativePathToLayout = @".\MainSettings.bsml")]
    internal class MainSettingsView : BSMLAutomaticViewController
    {   
        private bool modEnable = true;
        [UIValue("ModEnable")]
        public bool ModEnable
        {
            get => modEnable;
            set => modEnable = value;
        }

        [UIAction("#post-parse")]
        protected void Parsed()
        {
        }
    }
}
