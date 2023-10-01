using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using TMPro;
using UnityEngine;

namespace BeatSpeedrun.Views
{
    [ViewDefinition("BeatSpeedrun.Source.Views.FloatingTimer.bsml")]
    [HotReload(RelativePathToLayout = @".\FloatingTimer.bsml")]
    internal class FloatingTimerView : BSMLAutomaticViewController
    {
        [UIComponent("timer-text")]
        protected FormattableText _timerTextObject;

        private string _timerText;
        [UIValue("timer-text")]
        public string TimerText
        {
            get => _timerText;
            set
            {
                _timerText = value;
                NotifyPropertyChanged();
            }
        }

        private float _timerSize = 10f;
        [UIValue("timer-size")]
        public float TimerSize
        {
            get => _timerSize;
            set
            {
                _timerSize = value;
                NotifyPropertyChanged();
            }
        }

        public Color TimerColor
        {
            set
            {
                _timerTextObject.color = value;
            }
        }

        public TMP_FontAsset Font
        {
            set => _timerTextObject.font = value;
        }

        [UIAction("#post-parse")]
        protected void Parsed()
        {
            Material mat = new Material(_timerTextObject.material) { shader = Shader.Find("UI/Default") };
            _timerTextObject.material = mat;
        }
    }
}
