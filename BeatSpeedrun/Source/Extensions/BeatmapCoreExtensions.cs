namespace BeatSpeedrun.Extensions
{
    internal static class BeatmapDifficultyExtensions
    {
        internal static string ToTextColor(this BeatmapDifficulty diff)
        {
            // TODO: Use more emphasized colors
            switch (diff)
            {
                case BeatmapDifficulty.ExpertPlus:
                    return "#ddaaff";
                case BeatmapDifficulty.Expert:
                    return "#ff7777";
                case BeatmapDifficulty.Hard:
                    return "#ffbb77";
                case BeatmapDifficulty.Normal:
                    return "#77dddd";
                case BeatmapDifficulty.Easy:
                    return "#77ff77";
                default:
                    return "#888888";
            }
        }

        internal static string ToShortLabel(this BeatmapDifficulty diff)
        {
            switch (diff)
            {
                case BeatmapDifficulty.ExpertPlus:
                    return "EX+";
                case BeatmapDifficulty.Expert:
                    return "EX";
                case BeatmapDifficulty.Hard:
                    return "H";
                case BeatmapDifficulty.Normal:
                    return "N";
                case BeatmapDifficulty.Easy:
                    return "E";
                default:
                    return "-";
            }
        }
    }
}
