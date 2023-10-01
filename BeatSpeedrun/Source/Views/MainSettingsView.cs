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

        private bool timerInGameEnable = true;
        [UIValue("timer-in-game-enabled")]
        public bool TimerInGameEnable
        {
            get => timerInGameEnable;
            set => timerInGameEnable = value;
        }

        [UIAction("#post-parse")]
        protected void Parsed()
        {
        }
    }
}
