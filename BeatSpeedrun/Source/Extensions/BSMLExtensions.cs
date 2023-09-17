using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
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
        internal static void Fill(this Backgroundable rect, string from, string to)
        {
            var view = rect?.background as ImageView;
            if (view != null)
            {
                ColorUtility.TryParseHtmlString(from, out var a);
                ColorUtility.TryParseHtmlString(to, out var b);
                view.SetGradient(a, b);
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

    // USAGE:
    // <macro.as-host host="dropdown-view">
    //   <dropdown-list-setting id="id" text="" value="value" choices="choices" interactable="~interactable" on-change="on-change" />
    // </macro.as-host>
    internal class BSMLDropdownView<T> : BSMLView
    {
        internal event Action<T> OnSelected;

        private readonly Func<T, string> _label;
        private readonly IEqualityComparer<T> _comparer;

        internal BSMLDropdownView(Func<T, string> label, IEqualityComparer<T> comparer = null)
        {
            _label = label;
            _value = new Choice(default, _label(default));
            _choices.Add(_value);
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        [UIComponent("id")]
        private readonly DropDownListSetting _id;

        private Choice _value;

        [UIValue("value")]
        private Choice Value
        {
            get => _value;
            set => ChangeProperty(ref _value, value, _id);
        }

        [UIValue("choices")]
        private readonly List<object> _choices = new List<object>();

        private void ReplaceChoices(IEnumerable<Choice> choices)
        {
            _choices.Clear();
            _choices.AddRange(choices);
            if (_id != null)
            {
                _id.values = _choices;
                _id.UpdateChoices();
            }
        }

        private bool _interactable = true;

        [UIValue("interactable")]
        internal bool Interactable
        {
            get => _interactable;
            set => ChangeProperty(ref _interactable, value);
        }

        [UIAction("on-change")]
        private void OnChange(Choice choice)
        {
            try
            {
                OnSelected?.Invoke(choice.Value);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Error while invoking Selected event:\n{ex}");
            }
        }

        internal IEnumerable<T> Items
        {
            get => _choices.Select(c => ((Choice)c).Value);
            set => ReplaceChoices(value.Select(v => new Choice(v, _label(v))));
        }

        internal T Current
        {
            get => Value.Value;
            set => Value = _choices.Select(c => (Choice)c).First(c => _comparer.Equals(c.Value, value));
        }

        internal void Reset(T value)
        {
            Items = new[] { value };
            Current = value;
            Interactable = false;
        }

        internal void Reset(List<T> items, ref T current)
        {
            if (!items.Contains(current, _comparer))
            {
                current = items[0];
            }
            Items = items;
            Current = current;
            Interactable = true;
        }

        private class Choice
        {
            internal T Value { get; }
            internal string Label { get; }

            internal Choice(T value, string label)
            {
                Value = value;
                Label = label;
            }

            public override string ToString() => Label;
        }
    }
}
