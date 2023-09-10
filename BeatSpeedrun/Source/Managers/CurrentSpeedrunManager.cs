using System;
using System.Threading;
using System.Threading.Tasks;
using BeatSpeedrun.Models;
using BeatSpeedrun.Models.Speedrun;
using Zenject;

namespace BeatSpeedrun.Managers
{
    internal class CurrentSpeedrunManager : IInitializable, IDisposable
    {
        internal event Action OnCurrentSpeedrunChanged;
        internal event Action OnCurrentSpeedrunUpdated;
        internal event Action OnSpeedrunLoadingStateChanged;

        private readonly SpeedrunManager _speedrunManager;
        private readonly CancellationTokenSource _disposeCts = new CancellationTokenSource();
        private bool _isLoading = true;
        private Speedrun _current;

        internal CurrentSpeedrunManager(SpeedrunManager speedrunManager)
        {
            _speedrunManager = speedrunManager;
        }

        public void Initialize()
        {
            _ = InitializeAsync(_disposeCts.Token);
        }

        private async Task InitializeAsync(CancellationToken ct)
        {
            var currentId = PluginConfig.Instance.CurrentSpeedrun;
            if (string.IsNullOrEmpty(currentId))
            {
                IsLoading = false;
                return;
            }

            Speedrun current;
            try
            {
                current = await _speedrunManager.LoadAsync(currentId, ct);
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                IsLoading = false;

                Plugin.Log.Error($"Failed to load current speedrun {currentId}:\n{ex}");
                PluginConfig.Instance.CurrentSpeedrun = null;
                PluginConfig.Instance.Changed();

                return;
            }

            Current = current;
            IsLoading = false;
        }

        public void Dispose()
        {
            _disposeCts.Cancel();
        }

        /// <summary>
        /// Whether the current speedrun is loading.
        /// </summary>
        internal bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (_isLoading == value) return;
                _isLoading = value;
                try
                {
                    OnSpeedrunLoadingStateChanged?.Invoke();
                }
                catch (Exception ex)
                {
                    Plugin.Log.Error($"Error while invoking OnSpeedrunLoadingStateChanged event:\n{ex}");
                }
            }
        }

        /// <summary>
        /// The speedrun currently running. null if not running or loading.
        /// </summary>
        internal Speedrun Current
        {
            get => _current;
            private set
            {
                if (_current == value) return;
                _current = value;
                try
                {
                    OnCurrentSpeedrunChanged?.Invoke();
                }
                catch (Exception ex)
                {
                    Plugin.Log.Error($"Error while invoking OnCurrentSpeedrunChanged event:\n{ex}");
                }
            }
        }

        internal async Task StartAsync(string regulation, Segment? targetSegment)
        {
            if (IsLoading) throw new InvalidOperationException("Cannot start a speedrun while loading");
            if (Current != null) throw new InvalidOperationException("Stop the current speedrun before starting");

            IsLoading = true;
            Speedrun current;
            try
            {
                current = await _speedrunManager.CreateAsync(regulation, targetSegment, _disposeCts.Token);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Failed to start speedrun {regulation}:\n{ex}");
                IsLoading = false;
                throw;
            }

            PluginConfig.Instance.CurrentSpeedrun = current.Id;
            PluginConfig.Instance.Changed();

            Current = current;
            IsLoading = false;
        }

        internal void Stop()
        {
            if (IsLoading) throw new InvalidOperationException("Cannot stop the speedrun while loading");
            if (Current != null)
            {
                if (Current.SongScores.Count == 0)
                {
                    try
                    {
                        _speedrunManager.Delete(Current.Id);
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.Warn($"Failed to delete an empty speedrun:\n{ex}");
                    }
                }

                PluginConfig.Instance.CurrentSpeedrun = null;
                PluginConfig.Instance.Changed();

                Current = null;
            }
        }

        internal void NotifyUpdated()
        {
            if (!IsLoading && Current != null)
            {
                try
                {
                    OnCurrentSpeedrunUpdated?.Invoke();
                }
                catch (Exception ex)
                {
                    Plugin.Log.Error($"Error while invoking OnCurrentSpeedrunUpdated event:\n{ex}");
                }
            }
        }

        internal void Save()
        {
            if (!IsLoading && Current != null)
            {
                _speedrunManager.Save(Current);
            }
        }
    }
}
