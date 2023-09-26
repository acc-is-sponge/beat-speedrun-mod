using BeatSpeedrun.Models;
using BeatSpeedrun.Models.Leaderboard;
using BeatSpeedrun.Models.Speedrun;

namespace BeatSpeedrun.Views
{
    internal class LeaderboardTheme
    {
        public string IconSource { get; set; }
        public string IconColor { get; set; } = "#ffffff";
        public (string, string) Gradient { get; set; } = ("#203057", "#333384");
        public string GradientMainTextColor { get; set; } = "#ffffff";
        public string GradientSubTextColor { get; set; } = "#aaaaaa";
        public string AccentColor { get; set; } = "#ef96fd";

        internal string ReplaceRichText(string text)
        {
            return text
                .Replace("<$icon>", $"<{IconColor}>")
                .Replace("<$main>", $"<{GradientMainTextColor}>")
                .Replace("<$sub>", $"<{GradientSubTextColor}>")
                .Replace("<$accent>", $"<{AccentColor}>");
        }

        internal static LeaderboardTheme FromSpeedrun(Speedrun speedrun)
        {
            return speedrun != null
                ? FromSegment(speedrun.Progress.Current.Segment)
                : Inactive;
        }

        internal static LeaderboardTheme FromSegment(Segment? segment)
        {
            switch (segment)
            {
                case Segment.Bronze:
                    return Bronze;
                case Segment.Silver:
                    return Silver;
                case Segment.Gold:
                    return Gold;
                case Segment.Platinum:
                    return Platinum;
                case Segment.Emerald:
                    return Emerald;
                case Segment.Sapphire:
                    return Sapphire;
                case Segment.Ruby:
                    return Ruby;
                case Segment.Diamond:
                    return Diamond;
                case Segment.Master:
                    return Master;
                case Segment.Grandmaster:
                    return Grandmaster;
                default:
                    return Active;
            }
        }

        internal static LeaderboardTheme FromIndex(LeaderboardIndex index)
        {
            switch (index)
            {
                case LeaderboardIndex.SegmentTime s:
                    return FromSegment(s.Segment);
                case LeaderboardIndex.TotalPp _:
                    return TotalPp;
                case LeaderboardIndex.SongCount _:
                case LeaderboardIndex.SongsPp _:
                    return Songs;
                default:
                    return Active;
            }
        }

        internal static readonly LeaderboardTheme Inactive = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.clock.png",
            IconColor = "#aaaaaa",
            Gradient = ("#20202f", "#333344"),
        };

        internal static readonly LeaderboardTheme Active = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.clock.png",
        };

        internal static readonly LeaderboardTheme TotalPp = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.master.png",
        };

        internal static readonly LeaderboardTheme Songs = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.star.png",
        };

        internal static readonly LeaderboardTheme Bronze = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.trophy.png",
            IconColor = "#bb8855",
            Gradient = ("#583a24", "#694438"),
            AccentColor = "#ffee77",
        };

        internal static readonly LeaderboardTheme Silver = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.trophy.png",
            IconColor = "#aabbcc",
            Gradient = ("#6a6a75", "#778797"),
            AccentColor = "#ffee77",
        };

        internal static readonly LeaderboardTheme Gold = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.trophy.png",
            IconColor = "#ffee77",
            Gradient = ("#ada840", "#846a10"),
            GradientMainTextColor = "#ffffdd",
            GradientSubTextColor = "#bbddbb",
            AccentColor = "#ff7744",
        };

        internal static readonly LeaderboardTheme Platinum = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.trophy.png",
            IconColor = "#ddffdd",
            Gradient = ("#ffffff", "#ffffff"),
            GradientMainTextColor = "#1a4752",
            GradientSubTextColor = "#779aa7",
            AccentColor = "#ff7744",
        };

        internal static readonly LeaderboardTheme Emerald = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.emerald.png",
            IconColor = "#2ff4ec",
            Gradient = ("#00af81", "#002962"),
            GradientMainTextColor = "#ffffff",
            GradientSubTextColor = "#aaaaaa",
            AccentColor = "#ef96fd",
        };

        internal static readonly LeaderboardTheme Sapphire = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.sapphire.png",
            IconColor = "#4fa4ff",
            Gradient = ("#0a9fdd", "#310a9f"),
            GradientMainTextColor = "#ffffff",
            GradientSubTextColor = "#cccccc",
            AccentColor = "#54eadb",
        };

        internal static readonly LeaderboardTheme Ruby = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.ruby.png",
            IconColor = "#ff3a5a",
            Gradient = ("#ff5a00", "#570060"),
            GradientMainTextColor = "#ffffff",
            GradientSubTextColor = "#cccccc",
            AccentColor = "#ff7777",
        };

        internal static readonly LeaderboardTheme Diamond = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.diamond.png",
            IconColor = "#eeddff",
            Gradient = ("#f4ebff", "#e4a8f0"),
            GradientMainTextColor = "#000000",
            GradientSubTextColor = "#666666",
            AccentColor = "#ee5aee",
        };

        internal static readonly LeaderboardTheme Master = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.master.png",
            IconColor = "#ff47ff",
            Gradient = ("#6a3333", "#11114a"),
            GradientMainTextColor = "#ff7777",
            GradientSubTextColor = "#7777ff",
            AccentColor = "#7777ff",
        };

        internal static readonly LeaderboardTheme Grandmaster = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.master.png",
            IconColor = "#ffb8ff",
            Gradient = ("#ff4f5c", "#5c4fff"),
            GradientMainTextColor = "#ffffff",
            GradientSubTextColor = "#ffffff",
            AccentColor = "#ffb8ff",
        };
    }
}
