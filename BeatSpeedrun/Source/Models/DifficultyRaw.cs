using System;

namespace BeatSpeedrun.Models
{
    internal struct DifficultyRaw
    {
        // "_ExpertPlus_SoloStandard"
        // <=> DifficultyRaw { Difficulty = BeatmapDifficulty.ExpertPlus, Characteristic = "SoloStandard" }

        internal BeatmapDifficulty Difficulty { get; set; }
        internal string Characteristic { get; set; }

        internal DifficultyRaw(BeatmapDifficulty difficulty, string characteristic = "SoloStandard")
        {
            Difficulty = difficulty;
            Characteristic = characteristic;
        }

        public override string ToString() => $"_{Difficulty}_{Characteristic}";

        internal static DifficultyRaw Parse(string s)
        {
            if (!TryParse(s, out var dest))
            {
                throw new FormatException($"Cannot parse \"{s}\" as DifficultyRaw");
            }
            return dest;
        }

        internal static bool TryParse(string s, out DifficultyRaw dest)
        {
            dest = default;
            var parts = s.Split('_');
            if (parts.Length != 3) return false;

            // parts[0] must be empty
            if (!string.IsNullOrEmpty(parts[0])) return false;

            // parts[1] must be difficulty
            if (!Enum.TryParse<BeatmapDifficulty>(parts[1], out var difficulty)) return false;
            dest.Difficulty = difficulty;

            // parts[2] must be characteristic
            dest.Characteristic = parts[2];

            return true;
        }
    }
}
