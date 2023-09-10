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
        internal string RegulationId { get; }
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
            RegulationId = snapshot.Regulation;
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

            // replay snapshot completed scores
            _aggregateResult = new SongScore.AggregateResult(SongScores, 0f);
            foreach (var score in snapshot.CompletedScores) AddScore(score);
        }

        internal void AddScore(SnapshotCompletedScore source)
        {
            var songScore = new SongScore(source, MapSet, SongPpCalculator);
            SongScores.Add(songScore);
            var previousTotalPp = _aggregateResult.TotalPp;
            _aggregateResult = SongScore.Aggregate(SongScores, Regulation.Rules.Weight);
            Progress.Update(_aggregateResult.TotalPp, source.CompletedAt);
            songScore.LatestPpChange = _aggregateResult.TotalPp - previousTotalPp;
        }

        internal Snapshot ToSnapshot()
        {
            return new Snapshot()
            {
                Id = Id,
                StartedAt = Progress.StartedAt,
                Regulation = RegulationId,
                TargetSegment = Progress.TargetSegment?.Segment,
                Checksum = _checksum,
                CompletedScores = SongScores.Select(score => score.Source).ToList(),
            };
        }
    }
}

