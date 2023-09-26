using System;
using System.Collections.Generic;
using System.Linq;
using BeatSpeedrun.Extensions;
using BeatSpeedrun.Models.Leaderboard;
using BeatSpeedrun.Models.Speedrun;
using BeatSpeedrun.Views;

namespace BeatSpeedrun.Controllers.Support
{
    internal static class LeaderboardViewHelper
    {
        internal static LeaderboardCardView.Entry ToProgressCardEntry(Speedrun speedrun, LeaderboardIndex index)
        {
            if (speedrun == null) return null;

            bool isActive;
            string text;

            if (index is LeaderboardIndex.SegmentTime segmentTimeIndex)
            {
                var p = speedrun.Progress.Segments.FirstOrDefault(a => a.Segment == segmentTimeIndex.Segment);
                if (!p.Segment.HasValue) return null;

                isActive = p.ReachedAt.HasValue;
                text = p.ReachedAt is TimeSpan at
                    ? $"<line-height=55%><$main>{p.Segment}<size=80%><$accent> / <$main>{p.RequiredPp}pp"
                    + $"\n<size=80%><$accent>reached at <$main>{at.ToTimerString()}"
                    : $"<#aaaaaa>{p.Segment}<size=80%><$icon> / <#888888>{p.RequiredPp}pp";

            }
            else
            {
                var value = index.ComputeValue(speedrun);

                isActive = value.HasValue;
                text = value.HasValue
                    ? $"<line-height=55%><$main>{index.Key}\n<size=80%><$accent>⇒ <$main>{index.FormatValue(value.Value)}"
                    : $"<#aaaaaa>{index.Key}";
            }

            var theme = LeaderboardTheme.FromIndex(index);
            var activeTheme = isActive ? theme : LeaderboardTheme.Inactive;
            return new LeaderboardCardView.Entry(
                index.Key,
                activeTheme.Gradient,
                theme.IconSource,
                activeTheme.IconColor,
                ("#000000aa", "#000000dd"),
                theme.ReplaceRichText(text));
        }

        internal static LeaderboardScoresView.Entry ToScoreEntry(SongScore score)
        {
            var rectGradient = score.LatestPpChange.HasValue ? ("#ffff6622", "#ffff664f") : ("#00000000", "#00000000");
            var rank = score.Rank?.ToString("00") ?? "<#777777>--";
            var cover = score.GetCoverImageAsync(default);
            var title = $"<line-height=45%><noparse>{score.SongName}</noparse> <size=80%><#cccccc><noparse>{score.SongSubName}</noparse>";
            var subTitle = $"<#bbbbbb><noparse>{score.SongAuthorName}</noparse> [<#aaeeaa><noparse>{score.LevelAuthorName}</noparse><#bbbbbb>]";
            var difficulty =
                $"<{score.DifficultyRaw.Difficulty.ToTextColor()}>" +
                (score.Star != 0 ? $"★{score.Star:0.##}" : score.DifficultyRaw.Difficulty.ToShortLabel());
            var result =
                $"<line-height=45%>{score.Source.BaseAccuracy * 100:0.##}<size=70%>%" +
                "\n<size=75%>";
            result += score.Source.FullCombo
                ? "<#33ff33>FC"
                : score.Source.MissOrBadCutNoteCount == 0
                    ? "<#ff3333>×FC"
                    : $"<#ff3333>×{score.Source.MissOrBadCutNoteCount}";
            var modifiers = score.Source.Modifiers.ToString();
            if (!string.IsNullOrEmpty(modifiers)) result += $"<#bbbbbb>,{modifiers}";
            var meta = score.Pp != 0
                ? $"<{(score.Rank.HasValue ? "#ffff99" : "#777777")}>{score.Pp:0.#}<size=80%>pp"
                : "<#777777>---";
            if (score.LatestPpChange is float diff)
            {
                meta = $"<line-height=45%>{meta}\n<size=60%><#33ff33>+{diff:0.#}<size=50%>pp";
            }

            return new LeaderboardScoresView.Entry(
                rectGradient, rank, cover, title, subTitle, difficulty, result, meta);
        }
    }
}
