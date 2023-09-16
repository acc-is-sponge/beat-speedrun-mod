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

        private SongScore.AggregateResult _aggregateResult;

        internal List<SongScore> TopScores => _aggregateResult.TopScores;
        internal float TotalPp => _aggregateResult.TotalPp;

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

            // replay snapshot song scores
            _aggregateResult = new SongScore.AggregateResult(SongScores, 0f);
            foreach (var score in snapshot.SongScores) AddScore(score);
        }

        internal void AddScore(SnapshotSongScore source)
        {
            var songScore = new SongScore(source, MapSet, SongPpCalculator);
            SongScores.Add(songScore);
            Progress.Update(source.CompletedAt, () =>
            {
                var previousTotalPp = _aggregateResult.TotalPp;
                _aggregateResult = SongScore.Aggregate(SongScores, Regulation.Rules.Weight);
                songScore.LatestPpChange = _aggregateResult.TotalPp - previousTotalPp;
                return _aggregateResult.TotalPp;
            });
        }

        internal Snapshot ToSnapshot()
        {
            return new Snapshot()
            {
                Id = Id,
                StartedAt = Progress.StartedAt,
                Regulation = RegulationPath,
                TargetSegment = Progress.TargetSegment?.Segment,
                Checksum = _checksum,
                SongScores = SongScores.Select(score => score.Source).ToList(),
            };
        }
    }
}

