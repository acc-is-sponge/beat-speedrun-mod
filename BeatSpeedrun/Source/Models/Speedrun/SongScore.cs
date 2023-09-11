using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BeatSpeedrun.Models.Speedrun
{
    internal class SongScore
    {
        internal string Id { get; }
        internal SnapshotSongScore Source { get; }
        internal DifficultyRaw DifficultyRaw { get; }
        internal float Star { get; }
        internal float Pp { get; }
        internal int? Rank { get; set; } // set by Aggregate
        internal float? LatestPpChange { get; set; } // always cleared by Aggregate

        internal SongScore(SnapshotSongScore source, MapSet mapSet, SongPpCalculator songPpCalculator)
        {
            // Scores are identified by song hash and difficulty
            Id = $"{source.SongHash}{source.DifficultyRaw}";
            Source = source;
            DifficultyRaw = DifficultyRaw.Parse(source.DifficultyRaw);
            Star = mapSet.GetStar(source.SongHash, DifficultyRaw) ?? 0f;
            Pp = songPpCalculator.Calculate(Star, source.BaseAccuracy, source.Modifiers);
        }

        // It's a little odd that it's implemented here, but we cache it here because it may be referenced so often.
        #region IPreviewBeatmapLevel

        private IPreviewBeatmapLevel _level;

        internal IPreviewBeatmapLevel Level =>
            _level ?? (_level = LoadLevel()) ?? new DummyPreviewBeatmapLevel(Source.SongHash);

        private IPreviewBeatmapLevel LoadLevel()
        {
            var id = CustomLevelLoader.kCustomLevelPrefixId + Source.SongHash.ToUpper();
            return SongCore.Loader.GetLevelById(id);
        }

        internal string SongName => Level.songName;

        internal string SongSubName => Level.songSubName;

        internal string SongAuthorName => Level.songAuthorName;

        internal string LevelAuthorName => Level.levelAuthorName;

        internal Task<UnityEngine.Sprite> GetCoverImageAsync(CancellationToken ct) => Level.GetCoverImageAsync(ct);

        // TODO: How to handle this? Use SongDetailsCache?
        internal class DummyPreviewBeatmapLevel : IPreviewBeatmapLevel
        {
            private readonly string _songHash;

            internal DummyPreviewBeatmapLevel(string songHash)
            {
                _songHash = songHash;
            }

            public string levelID => CustomLevelLoader.kCustomLevelPrefixId + _songHash.ToUpper();
            public string songName => _songHash;
            public string songSubName => "";
            public string songAuthorName => "---";
            public string levelAuthorName => "---";
            public float beatsPerMinute => 100f;
            public float songTimeOffset => 0f;
            public float shuffle => 0f;
            public float shufflePeriod => 0f;
            public float previewStartTime => 0f;
            public float previewDuration => 0f;
            public float songDuration => 60f;
            public EnvironmentInfoSO environmentInfo => null;
            public EnvironmentInfoSO allDirectionsEnvironmentInfo => null;
            public IReadOnlyList<PreviewDifficultyBeatmapSet> previewDifficultyBeatmapSets => null;
            public Task<UnityEngine.Sprite> GetCoverImageAsync(CancellationToken _) => Task.FromResult<UnityEngine.Sprite>(null);
        }

        #endregion

        internal static AggregateResult Aggregate(IEnumerable<SongScore> scores, float weight)
        {
            var topScoresById = new Dictionary<string, SongScore>();

            foreach (var score in scores)
            {
                score.Rank = null; // reset
                score.LatestPpChange = null; // clear

                // Collect top scores for each id
                if (!topScoresById.TryGetValue(score.Id, out var topScore) || topScore.Pp < score.Pp)
                {
                    topScoresById[score.Id] = score;
                }
            }

            var topScores = topScoresById
                .Select(pair => pair.Value)
                .Where(score => score.Pp > 0f)
                .ToList();
            topScores.Sort((a, b) => b.Pp.CompareTo(a.Pp));

            var rank = 1;
            var totalPp = 0f;

            foreach (var topScore in topScores)
            {
                totalPp += topScore.Pp * UnityEngine.Mathf.Pow(weight, rank - 1);

                topScore.Rank = rank++;
            }

            return new AggregateResult(topScores, totalPp);
        }

        internal readonly struct AggregateResult
        {
            internal List<SongScore> TopScores { get; }
            internal float TotalPp { get; }

            internal AggregateResult(List<SongScore> topScores, float totalPp)
            {
                TopScores = topScores;
                TotalPp = totalPp;
            }
        }
    }
}
