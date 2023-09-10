using System;
using System.Linq;

namespace BeatSpeedrun.Models.Speedrun
{
    internal class SongPpCalculator
    {
        private readonly float _base;
        private readonly LinearInterpolation _curve;
        private readonly ModifiersFactor _modifiersFactor;

        internal SongPpCalculator(
            float @base,
            float[][] curve,
            ModifiersOverride modifiersOverride)
        {
            _base = @base;
            _curve = new LinearInterpolation(curve);
            _modifiersFactor = new ModifiersFactor(modifiersOverride);
        }

        internal float Calculate(
            float star,
            float accuracy,
            SnapshotModifiers modifiers)
        {
            var weight = _curve.ValueAt(accuracy);
            var modifiersFactor = _modifiersFactor.Calculate(modifiers);
            return _base * star * weight * modifiersFactor;
        }
    }

    internal readonly struct ModifiersFactor
    {
        private readonly ModifiersOverride _modifiersOverride;

        internal ModifiersFactor(ModifiersOverride modifiersOverride)
        {
            _modifiersOverride = modifiersOverride;
        }

        internal float Calculate(SnapshotModifiers modifiers)
        {
            var factor = 1.0f;
            if (modifiers.UsePause) factor *= _modifiersOverride.UsePause;
            if (modifiers.BatteryEnergy) factor *= _modifiersOverride.BatteryEnergy;
            if (modifiers.NoFail) factor *= _modifiersOverride.NoFail;
            if (modifiers.InstaFail) factor *= _modifiersOverride.InstaFail;
            if (modifiers.NoObstacles) factor *= _modifiersOverride.NoObstacles;
            if (modifiers.NoBombs) factor *= _modifiersOverride.NoBombs;
            if (modifiers.StrictAngles) factor *= _modifiersOverride.StrictAngles;
            if (modifiers.DisappearingArrows) factor *= _modifiersOverride.DisappearingArrows;
            if (modifiers.FasterSong) factor *= _modifiersOverride.FasterSong;
            if (modifiers.SlowerSong) factor *= _modifiersOverride.SlowerSong;
            if (modifiers.NoArrows) factor *= _modifiersOverride.NoArrows;
            if (modifiers.GhostNotes) factor *= _modifiersOverride.GhostNotes;
            if (modifiers.SuperFastSong) factor *= _modifiersOverride.SuperFastSong;
            if (modifiers.ProMode) factor *= _modifiersOverride.ProMode;
            if (modifiers.SmallCubes) factor *= _modifiersOverride.SmallCubes;
            return factor;
        }
    }

    internal readonly struct LinearInterpolation
    {
        private readonly float[][] _points;

        internal LinearInterpolation(float[][] points)
        {
            var last = -UnityEngine.Mathf.Infinity;
            if (points.Length == 0 || points.Any(p => p.Length != 2))
            {
                throw new ArgumentException(nameof(points));
            }
            foreach (var point in points)
            {
                if (last >= point[0])
                {
                    throw new ArgumentException(nameof(points));
                }
                last = point[0];
            }
            _points = points;
        }

        internal float ValueAt(float x)
        {
            // lower than [0]
            if (x <= _points[0][0]) return _points[0][1];
            // higher than [$-1]
            if (x >= _points[_points.Length - 1][0]) return _points[_points.Length - 1][1];

            var i = 1;
            while (x > _points[i][0]) i++;

            var a = _points[i - 1];
            var b = _points[i];
            var t = (x - a[0]) / (b[0] - a[0]);
            return (a[1] * (1 - t)) + (b[1] * t);
        }
    }
}
