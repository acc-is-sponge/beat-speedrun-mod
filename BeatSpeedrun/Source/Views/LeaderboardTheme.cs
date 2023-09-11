using BeatSpeedrun.Models;

namespace BeatSpeedrun.Views
{
    internal class LeaderboardTheme
    {
        public string PrimaryColor { get; set; }
        public string PrimaryGradFromColor { get; set; }
        public string PrimaryGradToColor { get; set; }
        public string PrimaryInvColor { get; set; }
        public string PrimaryInvSubColor { get; set; }
        public string AccentColor { get; set; }

        internal (string, string) PrimaryGrad => (PrimaryGradFromColor, PrimaryGradToColor);

        internal string ReplaceRichText(string text)
        {
            return text
                .Replace("<$p>", $"<{PrimaryColor}>")
                .Replace("<$p-inv>", $"<{PrimaryInvColor}>")
                .Replace("<$p-inv-sub>", $"<{PrimaryInvSubColor}>")
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
            PrimaryColor = "#999999",
            PrimaryGradFromColor = "#20202f",
            PrimaryGradToColor = "#333344",
            PrimaryInvColor = "#cccccc",
            PrimaryInvSubColor = "#aaaaaa",
            AccentColor = "#ef96fd",
        };

        internal static readonly LeaderboardTheme Start = new LeaderboardTheme
        {
            PrimaryColor = "#ffffff",
            PrimaryGradFromColor = "#203057",
            PrimaryGradToColor = "#333384",
            PrimaryInvColor = "#ffffff",
            PrimaryInvSubColor = "#aaaaaa",
            AccentColor = "#ef96fd",
        };

        internal static readonly LeaderboardTheme Bronze = new LeaderboardTheme
        {
            PrimaryColor = "#bb8855",
            PrimaryGradFromColor = "#583a24",
            PrimaryGradToColor = "#694438",
            PrimaryInvColor = "#ffffff",
            PrimaryInvSubColor = "#aaaaaa",
            AccentColor = "#ffee77",
        };

        internal static readonly LeaderboardTheme Silver = new LeaderboardTheme
        {
            PrimaryColor = "#aabbcc",
            PrimaryGradFromColor = "#6a6a75",
            PrimaryGradToColor = "#778797",
            PrimaryInvColor = "#ffffff",
            PrimaryInvSubColor = "#aaaaaa",
            AccentColor = "#ffee77",
        };

        internal static readonly LeaderboardTheme Gold = new LeaderboardTheme
        {
            PrimaryColor = "#ffee77",
            PrimaryGradFromColor = "#ada840",
            PrimaryGradToColor = "#846a10",
            PrimaryInvColor = "#ffffdd",
            PrimaryInvSubColor = "#bbddbb",
            AccentColor = "#ff7744",
        };

        internal static readonly LeaderboardTheme Platinum = new LeaderboardTheme
        {
            PrimaryColor = "#ddffdd",
            PrimaryGradFromColor = "#ffffff",
            PrimaryGradToColor = "#ffffff",
            PrimaryInvColor = "#1a4752",
            PrimaryInvSubColor = "#779aa7",
            AccentColor = "#ff7744",
        };

        internal static readonly LeaderboardTheme Emerald = new LeaderboardTheme
        {
            PrimaryColor = "#2ff4ec",
            PrimaryGradFromColor = "#00af81",
            PrimaryGradToColor = "#002962",
            PrimaryInvColor = "#ffffff",
            PrimaryInvSubColor = "#aaaaaa",
            AccentColor = "#ef96fd",
        };

        internal static readonly LeaderboardTheme Sapphire = new LeaderboardTheme
        {
            PrimaryColor = "#2f84ff",
            PrimaryGradFromColor = "#0a9fdd",
            PrimaryGradToColor = "#310a9f",
            PrimaryInvColor = "#ffffff",
            PrimaryInvSubColor = "#cccccc",
            AccentColor = "#54eadb",
        };

        internal static readonly LeaderboardTheme Ruby = new LeaderboardTheme
        {
            PrimaryColor = "#ff3a5a",
            PrimaryGradFromColor = "#ff5a00",
            PrimaryGradToColor = "#570060",
            PrimaryInvColor = "#ffffff",
            PrimaryInvSubColor = "#cccccc",
            AccentColor = "#ff7777",
        };

        internal static readonly LeaderboardTheme Diamond = new LeaderboardTheme
        {
            PrimaryColor = "#eddff",
            PrimaryGradFromColor = "#ffffff",
            PrimaryGradToColor = "#e4a8f0",
            PrimaryInvColor = "#000000",
            PrimaryInvSubColor = "#666666",
            AccentColor = "#ee5aee",
        };

        internal static readonly LeaderboardTheme Master = new LeaderboardTheme
        {
            PrimaryColor = "#bbbbbb",
            PrimaryGradFromColor = "#6a3333",
            PrimaryGradToColor = "#11114a",
            PrimaryInvColor = "#ff7777",
            PrimaryInvSubColor = "#7777ff",
            AccentColor = "#7777ff",
        };

        internal static readonly LeaderboardTheme Grandmaster = new LeaderboardTheme
        {
            PrimaryColor = "#ffffff",
            PrimaryGradFromColor = "#ff4f5c",
            PrimaryGradToColor = "#5c4fff",
            PrimaryInvColor = "#ffffff",
            PrimaryInvSubColor = "#ffffff",
            AccentColor = "#ffb8ff",
        };
    }
}
