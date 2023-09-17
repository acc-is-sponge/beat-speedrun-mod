using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatSpeedrun.Models.Speedrun
{
    /// <summary>
    /// Tracks the progress of the speedrun.
    /// </summary>
    internal class Progress
    {
        /// <summary>
        /// List of segments, sorted by pp (ascending).
        /// </summary>
        internal List<SegmentProgress> Segments { get; }

        private readonly int? _targetSegmentIndex;
        private int _currentSegmentIndex;

        /// <summary>
        /// Get the target segment. On the surface, speedruns stop when they
        /// reach the target segment. Returns null if target is unspecified.
        /// </summary>
        internal SegmentProgress? TargetSegment =>
            _targetSegmentIndex is int i ? (SegmentProgress?)Segments[i] : null;

        internal bool IsTargetReached => TargetSegment?.ReachedAt != null;

        /// <summary>
        /// Get the current segment.
        /// </summary>
        internal SegmentProgress GetCurrentSegment(bool ignoresTarget = false)
        {
            if (!ignoresTarget && IsTargetReached) return TargetSegment.Value;
            return Segments[_currentSegmentIndex];
        }

        internal SegmentProgress? GetNextSegment(bool ignoresTarget = false)
        {
            if (!ignoresTarget && IsTargetReached) return null;
            return _currentSegmentIndex + 1 < Segments.Count
                ? (SegmentProgress?)Segments[_currentSegmentIndex + 1]
                : null;
        }

        /// <summary>
        /// When the speedrun start?
        /// </summary>
        internal DateTime StartedAt { get; }

        internal TimeSpan TimeLimit { get; }

        internal DateTime TimeIsUpAt => StartedAt + TimeLimit;

        /// <summary>
        /// How much the speedrun has elapsed since the time.
        /// </summary>
        internal TimeSpan ElapsedTime(DateTime time, bool ignoresTarget = false)
        {
            if (!ignoresTarget && IsTargetReached) return TargetSegment.Value.ReachedAt.Value;
            return TimeIsUpAt < time ? TimeLimit : time - StartedAt;
        }

        internal Progress(
            DateTime startedAt,
            TimeSpan timeLimit,
            Segment? targetSegment,
            SegmentRequirements segmentRequirements)
        {
            StartedAt = startedAt;
            TimeLimit = timeLimit;
            Segments = Enum.GetValues(typeof(Segment))
                .Cast<Segment>()
                .Select(s => new SegmentProgress { Segment = s, RequiredPp = segmentRequirements.GetValue(s) })
                .ToList();
            Segments.Add(SegmentProgress.Start);
            Segments.Sort((a, b) => a.RequiredPp.CompareTo(b.RequiredPp));

            _targetSegmentIndex = targetSegment == null
                ? null
                : (int?)Segments.FindIndex(segment => segment.Segment == targetSegment);
            _currentSegmentIndex = 0;
        }

        internal void Update(DateTime ppChangedAt, Func<float> applyPpChange, bool ignoresTarget = false)
        {
            if (!ignoresTarget && IsTargetReached) return;
            if (TimeIsUpAt < ppChangedAt) return;

            var pp = applyPpChange();
            for (var i = _currentSegmentIndex + 1; i < Segments.Count; ++i)
            {
                var nextSegment = Segments[i];
                if (nextSegment.RequiredPp > pp) break;
                nextSegment.ReachedAt = ppChangedAt - StartedAt;
                Segments[i] = nextSegment;
                _currentSegmentIndex = i;
            }
        }

        internal struct SegmentProgress
        {
            internal Segment? Segment { get; set; }

            internal int RequiredPp { get; set; }

            /// <summary>
            /// When the speedrun reached the segment?
            /// </summary>
            internal TimeSpan? ReachedAt { get; set; }

            internal static readonly SegmentProgress Start =
                new SegmentProgress { ReachedAt = TimeSpan.Zero };
        }
    }
}
