using System;
using System.Threading;
using System.Threading.Tasks;
using BeatSpeedrun.Models;
using BeatSpeedrun.Models.Speedrun;
using BeatSpeedrun.Repositories;
using Zenject;

namespace BeatSpeedrun.Services
{
    /// <summary>
    /// A class that manages the actual speedrun progression in this plugin.
    /// </summary>
    internal class SpeedrunFacilitator : IInitializable, IDisposable
    {
        internal event Action<(Speedrun, Speedrun)> OnCurrentSpeedrunChanged;
        internal event Action OnCurrentSpeedrunUpdated;
        internal event Action OnSpeedrunLoadingStateChanged;

        private readonly SpeedrunRepository _speedrunRepository;
        private readonly LocalLeaderboardRepository _localLeaderboardRepository;
        private readonly CancellationTokenSource _disposeCts = new CancellationTokenSource();
        private bool _isLoading = true;
        private Speedrun _current;

        internal SpeedrunFacilitator(
            SpeedrunRepository speedrunRepository,
            LocalLeaderboardRepository localLeaderboardRepository)
        {
            _speedrunRepository = speedrunRepository;
            _localLeaderboardRepository = localLeaderboardRepository;
        }

        public void Initialize()
        {
            _ = InitializeAsync(_disposeCts.Token);
        }

        private async Task InitializeAsync(CancellationToken ct)
        {
            await WritePastSpeedrunsToLocalLeaderboardsAsync();

            var currentId = PluginConfig.Instance.CurrentSpeedrun;
            if (string.IsNullOrEmpty(currentId))
            {
                IsLoading = false;
                return;
            }

            Speedrun current;
            try
            {
                current = await _speedrunRepository.LoadAsync(currentId, ct);
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
                var prevCurrent = _current;
                _current = value;
                try
                {
                    OnCurrentSpeedrunChanged?.Invoke((prevCurrent, _current));
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
                current = await _speedrunRepository.CreateAsync(regulation, targetSegment, _disposeCts.Token);
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
            if (Current == null) return;

            if (Current.SongScores.Count == 0)
            {
                try
                {
                    _speedrunRepository.Delete(Current.Id);
                }
                catch (Exception ex)
                {
                    Plugin.Log.Warn($"Failed to delete an empty speedrun:\n{ex}");
                }
            }
            else
            {
                Current.Finish(DateTime.UtcNow);
                var leaderboard = _localLeaderboardRepository.Get(Current.Regulation);
                leaderboard.Write(Current);
                _localLeaderboardRepository.Save(leaderboard);
                _speedrunRepository.Save(Current);
            }

            PluginConfig.Instance.CurrentSpeedrun = null;
            PluginConfig.Instance.Changed();

            Current = null;

        }

        internal void NotifyUpdated()
        {
            if (IsLoading || Current == null) return;

            try
            {
                OnCurrentSpeedrunUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Error while invoking OnCurrentSpeedrunUpdated event:\n{ex}");
            }
        }

        internal void Save()
        {
            if (IsLoading || Current == null) return;

            if (PluginConfig.Instance.StopTargetReachedSpeedrunsAutomatically &&
                (Current.Progress.Target?.ReachedAt.HasValue ?? false))
            {
                Stop();
            }
            else
            {
                _speedrunRepository.Save(Current);
            }
        }

        /// <summary>
        /// This method ensures past speedruns to be recorded to local leaderboards.
        /// Whether or not they have been written is recorded in PluginConfig.Instance.LeaderboardWriteVersion.
        /// </summary>
        private async Task WritePastSpeedrunsToLocalLeaderboardsAsync()
        {
            var version = PluginConfig.Instance.LeaderboardWriteVersion;

            if (version < 1) // _ -> 1: added an initial leaderboard feature
            {
                foreach (var id in _speedrunRepository.List())
                {
                    try
                    {
                        var speedrun = await _speedrunRepository.LoadAsync(id);
                        if (speedrun.Progress.FinishedAt.HasValue)
                        {
                            var leaderboard = _localLeaderboardRepository.Get(speedrun.Regulation);
                            leaderboard.Write(speedrun);
                            _localLeaderboardRepository.Save(leaderboard);
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.Warn($"Error while writing speedrun {id} to leaderboard:\n{ex}");
                    }
                }
                version = 1;
            }

            if (version == PluginConfig.Instance.LeaderboardWriteVersion) return;
            PluginConfig.Instance.LeaderboardWriteVersion = version;
            PluginConfig.Instance.Changed();
        }
    }
}
