using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
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
        [UIValue("main-enabled")]
        public bool ModEnable
        {
            get => modEnable;
            set => modEnable = value;
        }

        private bool floatingtimerEnable = true;
        [UIValue("floating-timer-enabled")]
        public bool FloatingTimerEnable
        {
            get => floatingtimerEnable;
            set => floatingtimerEnable = value;
        }

        [UIAction("#post-parse")]
        protected void Parsed()
        {
        }
    }
}
