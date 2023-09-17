using System;
using System.Collections.Generic;
using System.Linq;
using BeatSpeedrun.Extensions;

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
        /// When is the speedrun started?
        /// </summary>
        internal DateTime StartedAt { get; }

        /// <summary>
        /// When is the speedrun finished?
        /// </summary>
        internal DateTime? FinishedAt { get; private set; }

        internal TimeSpan TimeLimit { get; }

        internal enum State
        {
            Running,
            TargetReached,
            TimeIsUp,
            Finished,
        }

        internal State ComputeState(DateTime time, bool ignoresTarget = false)
        {
            // We can't update progress after TIME IS UP or FINISHED, so it can be checked first
            if (!ignoresTarget && IsTargetReached) return State.TargetReached;

            var upTime = StartedAt + TimeLimit;
            var finishedTime = FinishedAt ?? DateTime.MaxValue;

            // TIME IS UP before FINISHED
            if (upTime < time && upTime < finishedTime) return State.TimeIsUp;

            // FINISHED
            if (finishedTime < time) return State.Finished;

            return State.Running;
        }

        /// <summary>
        /// Speedrun time elapsed to the argument time.
        /// </summary>
        internal TimeSpan ElapsedTime(DateTime time, bool ignoresTarget = false)
        {
            switch (ComputeState(time, ignoresTarget))
            {
                case State.TargetReached:
                    return TargetSegment.Value.ReachedAt.Value;
                case State.TimeIsUp:
                    return TimeLimit;
                case State.Finished:
                    return FinishedAt.Value - StartedAt;
                default:
                    return time - StartedAt;
            }
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
            if (ComputeState(ppChangedAt, ignoresTarget) != State.Running) return;

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

        internal void Finish(DateTime finishedAt)
        {
            if (FinishedAt.HasValue) return;

            FinishedAt = finishedAt;
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
