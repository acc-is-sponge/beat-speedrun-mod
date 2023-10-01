using System;
using System.Collections.Generic;
using System.Linq;
using BeatSpeedrun.Extensions;

namespace BeatSpeedrun.Models.Leaderboard
{
    internal abstract class LeaderboardIndex
    {
        internal enum Group
        {
            Segments,
            Stats,
        }

        internal abstract string Key { get; }
        internal abstract float? ComputeValue(Speedrun.Speedrun speedrun);
        internal abstract string FormatValue(float value);
        internal virtual int SortOrder => -1; // positive for ascending, negative for descending

        private static readonly Dictionary<string, LeaderboardIndex> _map;
        internal static IReadOnlyDictionary<string, LeaderboardIndex> Map => _map;

        private static readonly List<LeaderboardIndex> _segmentsIndices;
        private static readonly List<LeaderboardIndex> _statsIndices;

        internal static IReadOnlyList<LeaderboardIndex> List(Group group)
        {
            switch (group)
            {
                case Group.Segments:
                    return _segmentsIndices;
                case Group.Stats:
                    return _statsIndices;
                default:
                    throw new ArgumentException(nameof(group));
            }
        }

        static LeaderboardIndex()
        {
            _map = new Dictionary<string, LeaderboardIndex>();
            _segmentsIndices = new List<LeaderboardIndex>();
            _statsIndices = new List<LeaderboardIndex>();

            foreach (var segment in Enum.GetValues(typeof(Segment)).Cast<Segment>())
            {
                Register(new SegmentTime(segment), _segmentsIndices);
            }

            Register(new TotalPp(), _statsIndices);
            Register(new SongCount(), _statsIndices);
            Register(new TotalTime(), _statsIndices);

            foreach (var minutes in new[] { 15, 30, 60, 90, 120 })
            {
                Register(new MinutesPp(minutes), _statsIndices);
            }

            foreach (var count in new[] { 3, 6 })
            {
                Register(new SongsPp(count), _statsIndices);
            }
        }

        private static void Register(LeaderboardIndex index, List<LeaderboardIndex> group)
        {
            _map[index.Key] = index;
            group.Add(index);
        }

        internal sealed class SegmentTime : LeaderboardIndex
        {
            internal Segment Segment { get; }

            internal SegmentTime(Segment segment) => Segment = segment;

            internal override string Key => $"{Segment} time";

            internal override int SortOrder => 1;

            internal override float? ComputeValue(Speedrun.Speedrun speedrun)
            {
                var progress = speedrun.Progress.Segments.FirstOrDefault(s => s.Segment == Segment);
                if (!progress.Segment.HasValue) return null;
                if (progress.ReachedAt is TimeSpan time) return (float)time.TotalMilliseconds;
                return null;
            }

            internal override string FormatValue(float value) =>
                TimeSpan.FromMilliseconds(value).ToTimerString();
        }

        internal sealed class TotalPp : LeaderboardIndex
        {
            internal override string Key => "Total pp";

            internal override float? ComputeValue(Speedrun.Speedrun s) => s.TotalPp;

            internal override string FormatValue(float value) => $"{value:0.#}pp";
        }

        internal sealed class TotalTime : LeaderboardIndex
        {
            internal override string Key => "Total time";

            internal override float? ComputeValue(Speedrun.Speedrun s) =>
                (float)s.Progress.ElapsedTime(DateTime.UtcNow).TotalMilliseconds;

            internal override string FormatValue(float value) =>
                TimeSpan.FromMilliseconds(value).ToTimerString();
        }

        internal sealed class SongCount : LeaderboardIndex
        {
            internal override string Key => "Song count";

            internal override float? ComputeValue(Speedrun.Speedrun s) => s.TopScores.Count;

            internal override string FormatValue(float value) => $"{value} songs";
        }

        internal sealed class SongsPp : LeaderboardIndex
        {
            internal int Count { get; }

            internal SongsPp(int count) => Count = count;

            internal override string Key => $"{Count} songs pp";

            internal override float? ComputeValue(Speedrun.Speedrun s)
            {
                if (s.TopScores.Count < Count) return null;
                return s.SongsPp(Count);
            }

            internal override string FormatValue(float value) => $"{value:0.#}pp";
        }

        internal sealed class MinutesPp : LeaderboardIndex
        {
            internal int Minutes { get; }

            internal MinutesPp(int minutes) => Minutes = minutes;

            internal override string Key => $"{Minutes} minutes pp";

            internal override float? ComputeValue(Speedrun.Speedrun s)
            {
                var span = TimeSpan.FromMinutes(Minutes);
                if (s.Progress.ElapsedTime(DateTime.UtcNow) < span) return null;
                return s.TimePp(span);
            }

            internal override string FormatValue(float value) => $"{value:0.#}pp";
        }
    }
}
