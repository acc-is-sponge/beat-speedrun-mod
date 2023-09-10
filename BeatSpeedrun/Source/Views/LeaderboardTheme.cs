using BeatSpeedrun.Models;

namespace BeatSpeedrun.Views
{
    internal class LeaderboardTheme
    {
        internal string PrimaryColor { get; set; }
        internal string PrimaryGradFromColor { get; set; }
        internal string PrimaryGradToColor { get; set; }
        internal float PrimaryGradSkew { get; set; }
        internal string PrimaryInvColor { get; set; }
        internal string PrimaryInvSubColor { get; set; }
        internal string AccentColor { get; set; }

        internal (string, string, float) PrimaryGrad =>
            (PrimaryGradFromColor, PrimaryGradToColor, PrimaryGradSkew);

        internal string ReplaceRichText(string text)
        {
            return text
                .Replace("<$p>", $"<{PrimaryColor}>")
                .Replace("<$p-inv>", $"<{PrimaryInvColor}>")
                .Replace("<$p-inv-sub>", $"<{PrimaryInvSubColor}>")
                .Replace("<$accent>", $"<{AccentColor}>");
        }

        internal static LeaderboardTheme FromSegment(Segment? _segment)
        {
            // TODO
            return Running;
        }

        internal static readonly LeaderboardTheme Running = new LeaderboardTheme
        {
            PrimaryColor = "#2ff4ec",
            PrimaryGradFromColor = "#00af81",
            PrimaryGradToColor = "#002962",
            PrimaryGradSkew = 0,
            PrimaryInvColor = "#ffffff",
            PrimaryInvSubColor = "#aaaaaa",
            AccentColor = "#ef96fd",
        };
    }
}
