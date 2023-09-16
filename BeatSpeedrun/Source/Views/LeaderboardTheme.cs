using BeatSpeedrun.Models;

namespace BeatSpeedrun.Views
{
    internal class LeaderboardTheme
    {
        public string IconSource { get; set; }
        public string IconColor { get; set; }
        public string GradientFromColor { get; set; }
        public string GradientToColor { get; set; }
        public string GradientMainTextColor { get; set; }
        public string GradientSubTextColor { get; set; }
        public string AccentColor { get; set; }

        internal (string, string) Gradient => (GradientFromColor, GradientToColor);

        internal string ReplaceRichText(string text)
        {
            return text
                .Replace("<$icon>", $"<{IconColor}>")
                .Replace("<$main>", $"<{GradientMainTextColor}>")
                .Replace("<$sub>", $"<{GradientSubTextColor}>")
                .Replace("<$accent>", $"<{AccentColor}>");
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
                    return Start;
            }
        }

        internal static readonly LeaderboardTheme NotRunning = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.trophy.png",
            IconColor = "#999999",
            GradientFromColor = "#20202f",
            GradientToColor = "#333344",
            GradientMainTextColor = "#cccccc",
            GradientSubTextColor = "#aaaaaa",
            AccentColor = "#ef96fd",
        };

        internal static readonly LeaderboardTheme Start = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.trophy.png",
            IconColor = "#999999",
            GradientFromColor = "#203057",
            GradientToColor = "#333384",
            GradientMainTextColor = "#ffffff",
            GradientSubTextColor = "#aaaaaa",
            AccentColor = "#ef96fd",
        };

        internal static readonly LeaderboardTheme Bronze = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.trophy.png",
            IconColor = "#bb8855",
            GradientFromColor = "#583a24",
            GradientToColor = "#694438",
            GradientMainTextColor = "#ffffff",
            GradientSubTextColor = "#aaaaaa",
            AccentColor = "#ffee77",
        };

        internal static readonly LeaderboardTheme Silver = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.trophy.png",
            IconColor = "#aabbcc",
            GradientFromColor = "#6a6a75",
            GradientToColor = "#778797",
            GradientMainTextColor = "#ffffff",
            GradientSubTextColor = "#aaaaaa",
            AccentColor = "#ffee77",
        };

        internal static readonly LeaderboardTheme Gold = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.trophy.png",
            IconColor = "#ffee77",
            GradientFromColor = "#ada840",
            GradientToColor = "#846a10",
            GradientMainTextColor = "#ffffdd",
            GradientSubTextColor = "#bbddbb",
            AccentColor = "#ff7744",
        };

        internal static readonly LeaderboardTheme Platinum = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.trophy.png",
            IconColor = "#ddffdd",
            GradientFromColor = "#ffffff",
            GradientToColor = "#ffffff",
            GradientMainTextColor = "#1a4752",
            GradientSubTextColor = "#779aa7",
            AccentColor = "#ff7744",
        };

        internal static readonly LeaderboardTheme Emerald = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.emerald.png",
            IconColor = "#2ff4ec",
            GradientFromColor = "#00af81",
            GradientToColor = "#002962",
            GradientMainTextColor = "#ffffff",
            GradientSubTextColor = "#aaaaaa",
            AccentColor = "#ef96fd",
        };

        internal static readonly LeaderboardTheme Sapphire = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.sapphire.png",
            IconColor = "#4fa4ff",
            GradientFromColor = "#0a9fdd",
            GradientToColor = "#310a9f",
            GradientMainTextColor = "#ffffff",
            GradientSubTextColor = "#cccccc",
            AccentColor = "#54eadb",
        };

        internal static readonly LeaderboardTheme Ruby = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.ruby.png",
            IconColor = "#ff3a5a",
            GradientFromColor = "#ff5a00",
            GradientToColor = "#570060",
            GradientMainTextColor = "#ffffff",
            GradientSubTextColor = "#cccccc",
            AccentColor = "#ff7777",
        };

        internal static readonly LeaderboardTheme Diamond = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.diamond.png",
            IconColor = "#eeddff",
            GradientFromColor = "#f4ebff",
            GradientToColor = "#e4a8f0",
            GradientMainTextColor = "#000000",
            GradientSubTextColor = "#666666",
            AccentColor = "#ee5aee",
        };

        internal static readonly LeaderboardTheme Master = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.master.png",
            IconColor = "#ff47ff",
            GradientFromColor = "#6a3333",
            GradientToColor = "#11114a",
            GradientMainTextColor = "#ff7777",
            GradientSubTextColor = "#7777ff",
            AccentColor = "#7777ff",
        };

        internal static readonly LeaderboardTheme Grandmaster = new LeaderboardTheme
        {
            IconSource = "BeatSpeedrun.Source.Resources.master.png",
            IconColor = "#ffb8ff",
            GradientFromColor = "#ff4f5c",
            GradientToColor = "#5c4fff",
            GradientMainTextColor = "#ffffff",
            GradientSubTextColor = "#ffffff",
            AccentColor = "#ffb8ff",
        };
    }
}
