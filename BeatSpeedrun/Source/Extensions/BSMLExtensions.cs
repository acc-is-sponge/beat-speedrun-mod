using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.GameplaySetup;
using HMUI;
using UnityEngine;
using Zenject;

namespace BeatSpeedrun.Extensions
{
    internal class BSMLView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void ChangeProperty<T>(ref T var, T val, Action render)
        {
            if (EqualityComparer<T>.Default.Equals(var, val)) return;
            var = val;
            render();
        }

        protected void ChangeProperty<T>(ref T var, T val, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(var, val)) return;
            var = val;
            NotifyPropertyChanged(propertyName);
        }

        protected void ChangeProperty<T>(ref T var, T val, DropDownListSetting dropdown)
        {
            if (EqualityComparer<T>.Default.Equals(var, val)) return;
            var = val;
            if (dropdown != null) dropdown.Value = val;
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Error invoking PropertyChanged for property '{propertyName}'\n{ex}");
            }
        }
    }

    internal static class BackgroundableExtensions
    {
        internal static void Fill(this Backgroundable rect, string from, string to, float skew)
        {
            var view = rect?.background as ImageView;
            if (view != null)
            {
                ColorUtility.TryParseHtmlString(from, out var a);
                ColorUtility.TryParseHtmlString(to, out var b);
                view.SetGradient(a, b);
                view.SetSkew(skew);
            }
        }
    }

    // FIXME: Can we replace this with `BSMLAutomaticViewController` and introduce `SettingsManager`
    // to add/remove tab? It seems that it is incompatible with `GameplaySetup.instance.AddTab` interface.
    internal abstract class BSMLTabViewController : IInitializable, IDisposable
    {
        abstract protected string TabName { get; }

        abstract protected string TabResource { get; }

        public virtual void Initialize()
        {
            GameplaySetup.instance.AddTab(TabName, TabResource, this);
        }

        public virtual void Dispose()
        {
            if (GameplaySetup.IsSingletonAvailable && BSMLParser.IsSingletonAvailable)
                GameplaySetup.instance.RemoveTab(TabName);
        }
    }
}
