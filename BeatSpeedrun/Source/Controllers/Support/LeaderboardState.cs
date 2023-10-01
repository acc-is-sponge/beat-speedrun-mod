using BeatSpeedrun.Extensions;
using BeatSpeedrun.Models.Leaderboard;
using BeatSpeedrun.Models.Speedrun;
using BeatSpeedrun.Providers;
using BeatSpeedrun.Repositories;
using BeatSpeedrun.Views;
using BeatSpeedrun.Services;
using System.Threading.Tasks;
using System;

namespace BeatSpeedrun.Controllers.Support
{
    /// <summary>
    /// State model for LeaderboardViewController; each `Get...` method returns
    /// the same task for the same argument for TaskWaiter compatibility.
    /// </summary>
    internal class LeaderboardState
    {
        private readonly RegulationProvider _regulationProvider;
        private readonly SpeedrunRepository _speedrunRepository;
        private readonly LocalLeaderboardRepository _localLeaderboardRepository;
        private readonly SpeedrunFacilitator _speedrunFacilitator;
        private readonly TaskCache<string, Speedrun> _speedrunCache;
        private readonly TaskCache<string, LocalLeaderboard> _localLeaderboardCache;

        internal LeaderboardState(
            RegulationProvider regulationProvider,
            SpeedrunRepository speedrunRepository,
            LocalLeaderboardRepository localLeaderboardRepository,
            SpeedrunFacilitator speedrunFacilitator)
        {
            _regulationProvider = regulationProvider;
            _speedrunRepository = speedrunRepository;
            _localLeaderboardRepository = localLeaderboardRepository;
            _speedrunFacilitator = speedrunFacilitator;
            _speedrunCache = new TaskCache<string, Speedrun>(LoadSpeedrunAsync);
            _localLeaderboardCache = new TaskCache<string, LocalLeaderboard>(LoadLocalLeaderboardAsync);
        }

        private async Task<Speedrun> LoadSpeedrunAsync(string id, Task<Speedrun> task)
        {
            if (task != null) await Task.Delay(TimeSpan.FromSeconds(3));
            if (id == null) throw new ArgumentException(nameof(id));
            return await _speedrunRepository.LoadAsync(id);
        }

        private async Task<LocalLeaderboard> LoadLocalLeaderboardAsync(string regulationPath, Task<LocalLeaderboard> task)
        {
            if (task != null) await Task.Delay(TimeSpan.FromSeconds(3));
            var regulation = await _regulationProvider.GetAsync(regulationPath);
            return _localLeaderboardRepository.Get(regulation);
        }

        internal Task<Speedrun> GetSpeedrunAsync(string id)
        {
            if (id == null) return Task.FromResult(_speedrunFacilitator.Current);
            return _speedrunCache.GetAsync(id);
        }

        internal Task<LocalLeaderboard> GetLocalLeaderboardAsync(string regulationPath)
        {
            return _localLeaderboardCache.GetAsync(regulationPath);
        }

        internal void ClearCaches()
        {
            _speedrunCache.Clear();
            _localLeaderboardCache.Clear();
        }

        // The leaderboard state node determines what this view controller renders.
        internal abstract class Node
        {
        }

        internal class ShowSpeedrun : Node
        {
            internal string Speedrun { get; set; } // null for the current speedrun
            internal LeaderboardSideControlView.SpeedrunTab Tab { get; set; }
            internal LeaderboardIndex.Group ProgressIndexGroup { get; set; }
            internal int Page { get; set; }
        }

        internal class ShowLeaderboard : Node
        {
            // We don't have Regulation here, instead we always use SelectionState
            // internal string Regulation { get; set; }
            internal LeaderboardType Type { get; set; }
            internal LeaderboardIndex.Group IndexGroup { get; set; }
            internal string IndexKey { get; set; } // null fallbacks IndexGroup
            internal LeaderboardSort Sort { get; set; }
            internal int Page { get; set; }
        }
    }
}
