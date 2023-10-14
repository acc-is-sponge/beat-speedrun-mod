using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSpeedrun.Views;
using System;
using System.ComponentModel;
using Zenject;

namespace BeatSpeedrun.Controllers
{
    [HotReload(RelativePathToLayout = @"..\Views\ModSettings.bsml")]
    [ViewDefinition(ModSettingsView.ResourceName)]
    internal class ModSettingsViewController : BSMLAutomaticViewController, IInitializable, IDisposable
    {
        #region render

        [UIValue("view")]
        private readonly ModSettingsView _view = new ModSettingsView();

        private void Render()
        {
            _view.ShowFloatingTimer = PluginConfig.Instance.FloatingTimerEnabled;
        }

        #endregion

        #region callbacks

        void IInitializable.Initialize()
        {
            Render();
            _view.PropertyChanged += HandleChange;
        }

        void IDisposable.Dispose()
        {
            _view.PropertyChanged -= HandleChange;
        }

        private void HandleChange(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ModSettingsView.ShowFloatingTimer):
                    PluginConfig.Instance.FloatingTimerEnabled = _view.ShowFloatingTimer;
                    PluginConfig.Instance.Changed();
                    break;
            }
        }

        #endregion
    }
}
