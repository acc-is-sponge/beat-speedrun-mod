using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSpeedrun.Extensions;
using BeatSpeedrun.Models.Speedrun;
using BeatSpeedrun.Services;
using BeatSpeedrun.Views;
using BS_Utils.Utilities;
using IPA.Utilities;
using System;
using UnityEngine;
using VRUIControls;
using Zenject;

namespace BeatSpeedrun.Source.Views
{
    internal class FloatingTimerViewController : IInitializable, ITickable, IDisposable
    {
        private FloatingScreen _floatingScreen;
        private readonly FloatingTimerView _floatingTimerView;
        private readonly SpeedrunFacilitator _speedrunFacilitator;
        private readonly PhysicsRaycasterWithCache _physicsRaycasterWithCache;
        private readonly MainSettingsView _mainSettingsView;
        private LeaderboardTheme CurrentTheme =>
            _speedrunFacilitator.Current is Speedrun speedrun
                ? LeaderboardTheme.FromSegment(speedrun.Progress.GetCurrentSegment().Segment)
                : LeaderboardTheme.NotRunning;

        public FloatingTimerViewController(PhysicsRaycasterWithCache physicsRaycasterWithCache, SpeedrunFacilitator speedrunFacilitator, FloatingTimerView floatingTimerView, MainSettingsView mainSettingsView)
        {
            _physicsRaycasterWithCache = physicsRaycasterWithCache;
            _speedrunFacilitator = speedrunFacilitator;
            _floatingTimerView = floatingTimerView;
            _mainSettingsView = mainSettingsView;
        }

        private bool activeFloor = false;
        public void Display()
        {
            activeFloor = true;
        }

        public void Hide()
        {
            activeFloor = false;
        }

        public void Initialize()
        {
            PrepareFloatingScreen();
            BSEvents.gameSceneLoaded += Display;
            BSEvents.menuSceneLoaded += Hide;
        }

        private void PrepareFloatingScreen()
        {
            _floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(150f, 50f), false, new Vector3(0f, 3f, 3.9f), Quaternion.Euler(new Vector3(325f, 0f, 0f)));
            _floatingScreen.GetComponent<VRGraphicRaycaster>().SetField("_physicsRaycaster", _physicsRaycasterWithCache);
            _floatingScreen.SetRootViewController(_floatingTimerView, HMUI.ViewController.AnimationType.Out);
        }

        public void Tick()
        {
            RenderFloatingTimer();
        }

        private void RenderFloatingTimer()
        {
            if (!_mainSettingsView.FloatingTimerEnable || !activeFloor)
            {
                if (_floatingScreen.gameObject.activeSelf)
                    _floatingScreen.gameObject.SetActive(false);
                return;
            }

            if (!_floatingScreen.gameObject.activeSelf)
                PrepareFloatingScreen();

            Speedrun speedrun = _speedrunFacilitator.Current;
            LeaderboardTheme theme = CurrentTheme;

            if (speedrun == null) return;

            DateTime now = DateTime.UtcNow;
            TimeSpan time = speedrun.Progress.ElapsedTime(now);
            string text;
            switch (speedrun.Progress.ComputeState(now))
            {
                case Progress.State.TimeIsUp:
                    text = $"<line-height=40%><size=80%><$main>TIME IS UP!\n<size=60%><$sub>{time.ToTimerString()}";
                    break;
                default:
                    text = time.ToTimerString();
                    break;
            }
            _floatingTimerView.TimerText = theme.ReplaceRichText(text);
            string segment = _speedrunFacilitator.Current.Progress.GetCurrentSegment().Segment != null ?
                _speedrunFacilitator.Current.Progress.GetCurrentSegment().Segment.ToString() : "-";
            _floatingTimerView.TimerText += "\n" + segment;
            _floatingTimerView.TimerText += "\n" + _speedrunFacilitator.Current.TotalPp.ToString("F2");
            _floatingTimerView.TimerColor = new Color(
                System.Convert.ToInt32(theme.IconColor.Substring(1, 2), 16) / 255f,
                System.Convert.ToInt32(theme.IconColor.Substring(3, 2), 16) / 255f,
                System.Convert.ToInt32(theme.IconColor.Substring(5, 2), 16) / 255f
            ).ColorWithAlpha(1f);
        }

        public void Dispose()
        {
            BSEvents.gameSceneLoaded -= Display;
            BSEvents.menuSceneLoaded -= Hide;
        }
    }
}
