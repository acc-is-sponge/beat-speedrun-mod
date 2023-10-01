using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatSpeedrun.Models.Speedrun
{
    /// <summary>
    /// A root state of the speedrun.
    /// </summary>
    internal class Speedrun
    {
        internal string Id { get; }
        internal string RegulationPath { get; }
        internal Regulation Regulation { get; }
        internal MapSet MapSet { get; }
        internal Progress Progress { get; }
        internal SongPpCalculator SongPpCalculator { get; }
        internal List<SongScore> SongScores { get; }
        internal List<SongScore> TopScores { get; private set; }
        internal float TotalPp { get; private set; }

        private readonly SnapshotChecksum _checksum;

        internal Speedrun(Snapshot snapshot, Regulation regulation, MapSet mapSet)
        {
            snapshot.Validate(regulation, mapSet);

            _checksum = snapshot.Checksum;

            Id = snapshot.Id;
            RegulationPath = snapshot.Regulation;
            Regulation = regulation;
            MapSet = mapSet;
            Progress = new Progress(
                snapshot.StartedAt,
                TimeSpan.FromSeconds(regulation.Rules.TimeLimit),
                snapshot.TargetSegment,
                regulation.Rules.SegmentRequirements);
            SongPpCalculator = new SongPpCalculator(
                regulation.Rules.Base,
                regulation.Rules.Curve,
                regulation.Rules.ModifiersOverride);
            SongScores = new List<SongScore>();
            TopScores = new List<SongScore>();

            // replay snapshot song scores
            foreach (var score in snapshot.SongScores) AddScore(score);
            if (snapshot.FinishedAt is DateTime t) Finish(t);
        }

        internal void AddScore(SnapshotSongScore source)
        {
            if (Progress.FinishedAt.HasValue) return;

            var songScore = new SongScore(source, MapSet, SongPpCalculator);
            SongScores.Add(songScore);
            Progress.Update(source.CompletedAt, () =>
            {
                var previousTotalPp = TotalPp;
                TopScores = SongScore.ComputeTopScores(SongScores);
                TotalPp = SongScore.ComputeTotalPp(TopScores, Regulation.Rules.Weight);

                foreach (var score in SongScores)
                {
                    score.Rank = null;
                    score.LatestPpChange = null;
                }
                var rank = 1;
                foreach (var topScore in TopScores) topScore.Rank = rank++;
                songScore.LatestPpChange = TotalPp - previousTotalPp;

                return TotalPp;
            });
        }

        internal float SongsPp(int count)
        {
            var songs = TopScores.Take(count);
            return SongScore.ComputeTotalPp(songs, Regulation.Rules.Weight);
        }

        internal float TimePp(TimeSpan time)
        {
            var songs = TopScores.Where(s => s.Source.CompletedAt - Progress.StartedAt < time);
            return SongScore.ComputeTotalPp(songs, Regulation.Rules.Weight);
        }

        internal void Finish(DateTime finieshedAt)
        {
            if (Progress.FinishedAt.HasValue) return;

            Progress.Finish(finieshedAt);
        }

        internal Snapshot ToSnapshot()
        {
            return new Snapshot()
            {
                Id = Id,
                StartedAt = Progress.StartedAt,
                FinishedAt = Progress.FinishedAt,
                Regulation = RegulationPath,
                TargetSegment = Progress.Target?.Segment,
                Checksum = _checksum,
                SongScores = SongScores.Select(score => score.Source).ToList(),
            };
        }
    }
}
