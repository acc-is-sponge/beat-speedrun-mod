
using System;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSpeedrun.Controllers;
using BS_Utils.Utilities;
using IPA.Utilities;
using UnityEngine;
using VRUIControls;
using Zenject;

namespace BeatSpeedrun.Registerers
{
    internal class FloatingTimerRegisterer : IInitializable, IDisposable
    {
        private readonly PhysicsRaycasterWithCache _physicsRaycasterWithCache;
        private readonly FloatingTimerViewController _floatingTimerViewController;

        internal FloatingTimerRegisterer(
            PhysicsRaycasterWithCache physicsRaycasterWithCache,
            FloatingTimerViewController floatingTimerViewController)
        {
            _physicsRaycasterWithCache = physicsRaycasterWithCache;
            _floatingTimerViewController = floatingTimerViewController;
        }

        private FloatingScreen _floatingScreen;

        void IInitializable.Initialize()
        {
            _floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(150f, 50f), false, new Vector3(0f, 3f, 3.9f), Quaternion.Euler(new Vector3(325f, 0f, 0f)));
            // NOTE: Is this necessary?
            _floatingScreen.GetComponent<VRGraphicRaycaster>().SetField("_physicsRaycaster", _physicsRaycasterWithCache);
            _floatingScreen.SetRootViewController(_floatingTimerViewController, HMUI.ViewController.AnimationType.Out);
        }

        void IDisposable.Dispose()
        {
            GameObject.Destroy(_floatingScreen);
        }
    }
}
