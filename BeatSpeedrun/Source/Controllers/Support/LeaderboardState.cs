using BeatSpeedrun.Extensions;
using BeatSpeedrun.Models.Leaderboard;
using BeatSpeedrun.Models.Speedrun;
using BeatSpeedrun.Repositories;
using BeatSpeedrun.Services;
using BeatSpeedrun.Views;
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
        private readonly SpeedrunRepository _speedrunRepository;
        private readonly SpeedrunFacilitator _speedrunFacilitator;
        private readonly TaskCache<string, Speedrun> _speedrunCache;

        internal LeaderboardState(
            SpeedrunRepository speedrunRepository,
            SpeedrunFacilitator speedrunFacilitator)
        {
            _speedrunRepository = speedrunRepository;
            _speedrunFacilitator = speedrunFacilitator;
            _speedrunCache = new TaskCache<string, Speedrun>(LoadSpeedrunAsync);
        }

        private async Task<Speedrun> LoadSpeedrunAsync(string id, Task<Speedrun> task)
        {
            if (task != null) await Task.Delay(TimeSpan.FromSeconds(3));
            if (id == null) throw new ArgumentException(nameof(id));
            return await _speedrunRepository.LoadAsync(id);
        }

        internal Task<Speedrun> GetSpeedrunAsync(string id)
        {
            if (id == null) return Task.FromResult(_speedrunFacilitator.Current);
            return _speedrunCache.GetAsync(id);
        }

        internal void ClearCaches()
        {
            _speedrunCache.Clear();
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
    }
}
