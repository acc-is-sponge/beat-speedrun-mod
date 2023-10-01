using System;

namespace BeatSpeedrun.Models.Leaderboard
{
    internal class LeaderboardRecord
    {
        internal string Speedrun { get; set; }

        internal int Rank { get; set; }

        internal string UserName { get; set; }

        internal DateTime StartedAt { get; set; }

        internal float Value { get; set; }

        internal LeaderboardRecord(
            string speedrun,
            int rank,
            string userName,
            DateTime startedAt,
            float value)
        {
            Speedrun = speedrun;
            Rank = rank;
            UserName = userName;
            StartedAt = startedAt;
            Value = value;
        }
    }
}
