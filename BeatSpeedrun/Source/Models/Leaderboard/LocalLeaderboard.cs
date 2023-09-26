using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using BeatSpeedrun.Extensions;

namespace BeatSpeedrun.Models.Leaderboard
{
    /// <summary>
    /// A model that accumulates records of speedruns of several compatible regulations.
    /// </summary>
    internal class LocalLeaderboard
    {
        /// <summary>
        /// Key to distinguish between incompatible regulations.
        /// </summary>
        [JsonProperty("key")]
        internal string Key { get; set; }

        /// <summary>
        /// List of speedrun records.
        /// </summary>
        [JsonProperty("records")]
        internal Dictionary<string, LocalLeaderboardRecord> Records { get; set; }

        internal static LocalLeaderboard FromJson(string json)
        {
            return JsonConvert.DeserializeObject<LocalLeaderboard>(json);
        }

        internal string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }

        /// <summary>
        ///  Write finished speedruns on this leaderboard.
        /// </summary>
        internal void Write(Speedrun.Speedrun speedrun)
        {
            if (!speedrun.Progress.FinishedAt.HasValue)
            {
                throw new ArgumentException("Speedrun is not finished yet");
            }
            if (GetKey(speedrun.Regulation) != Key)
            {
                throw new ArgumentException("Provided speedrun is not compatible with this leaderboard");
            }

            if (!Records.ContainsKey(speedrun.Id))
            {
                Records[speedrun.Id] = new LocalLeaderboardRecord
                {
                    Speedrun = speedrun.Id,
                    StartedAt = speedrun.Progress.StartedAt,
                    Indices = new Dictionary<string, float>(),
                };
            }
            var record = Records[speedrun.Id];

            foreach (var index in LeaderboardIndex.Map.Values)
            {
                var value = index.ComputeValue(speedrun);
                if (!value.HasValue) continue;
                record.Indices[index.Key] = value.Value;
            }
        }

        internal static string GetKey(Regulation regulation)
        {
            // For now, we use regulation.Title to distinguish between incompatible regulations.
            // It is difficult (or impposible) to calculate compatibility between regulations, so
            // somemthing like it is unavoidable, but we should prepare more meaningful field on regulations.
            return regulation.Title.ComputeChecksum();
        }

        internal IEnumerable<(LeaderboardIndex, LeaderboardRecord)> QueryRecords(
            LeaderboardIndex.Group indexGroup, LeaderboardSort sort)
        {
            var orderFunc = LocalLeaderboardRecord.OrderFunc(sort);
            return LeaderboardIndex.List(indexGroup).Select(index =>
            {
                var record = Records.Values
                    .Where(r => r.CanFocus(index))
                    .OrderBy(r => orderFunc(r, index))
                    .FirstOrDefault();
                return (index, record?.Focus(index));
            });
        }

        internal IEnumerable<LeaderboardRecord> QueryRecords(
            LeaderboardIndex index, LeaderboardSort sort)
        {
            var records = Records.Values.Where(r => r.CanFocus(index));

            var rankOrderFunc = LocalLeaderboardRecord.OrderFunc(LeaderboardSort.Best);
            var recordRanks = new Dictionary<LocalLeaderboardRecord, int>();
            var rank = 0;
            foreach (var record in records.OrderBy(r => rankOrderFunc(r, index)))
            {
                recordRanks[record] = rank++;
            }

            var orderFunc = LocalLeaderboardRecord.OrderFunc(sort);
            return records
                .OrderBy(r => orderFunc(r, index))
                .Select(r =>
                {
                    var record = r.Focus(index);
                    record.Rank = recordRanks[r];
                    return record;
                });
        }
    }

    internal class LocalLeaderboardRecord
    {
        [JsonProperty("speedrun")]
        internal string Speedrun { get; set; }

        [JsonProperty("startedAt")]
        internal DateTime StartedAt { get; set; }

        [JsonProperty("indices")]
        internal Dictionary<string, float> Indices { get; set; }

        internal bool CanFocus(LeaderboardIndex index) => Indices.ContainsKey(index.Key);

        internal LeaderboardRecord Focus(LeaderboardIndex index)
        {
            if (!Indices.TryGetValue(index.Key, out var value)) return null;

            return new LeaderboardRecord(
                Speedrun,
                0, // filled later
                null, // TODO
                StartedAt,
                value);
        }

        internal static Func<LocalLeaderboardRecord, LeaderboardIndex, float> OrderFunc(LeaderboardSort sort)
        {
            switch (sort)
            {
                case LeaderboardSort.Recent:
                    var now = DateTime.UtcNow;
                    return (r, _) => (float)(now - r.StartedAt).TotalMilliseconds;
                case LeaderboardSort.Best:
                    return (r, i) => r.Indices[i.Key] * i.SortOrder;
                default:
                    throw new ArgumentException(nameof(sort));
            }
        }
    }
}
