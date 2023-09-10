using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSpeedrun.Extensions;
using BeatSpeedrun.Views;
using Zenject;
using System.Threading.Tasks;

namespace BeatSpeedrun.Controllers
{
    [HotReload(RelativePathToLayout = "..\\Views\\LeaderboardMain.bsml")]
    [ViewDefinition(LeaderboardMainView.ResourceName)]
    internal class LeaderboardMainViewController : BSMLAutomaticViewController, IInitializable
    {
        [UIValue("view")]
        private readonly LeaderboardMainView _view = new LeaderboardMainView();

        public void Initialize()
        {
            MockUp();
        }


        private void MockUp()
        {
            var theme = LeaderboardTheme.Running;

            var scoreEntries = Enumerable.Range(0, 8).Select(i =>
            {
                return new LeaderboardMainView.ScoreEntry(
                    i.ToString("00"),
                    Task.FromResult<UnityEngine.Sprite>(null),
                    "SongName <size=80%><#cccccc>SongSubName",
                    "<#bbbbbb>SongAuthorName [<#aaaaaa>LevelAuthorName<#bbbbbb>]",
                    $"<{BeatmapDifficulty.ExpertPlus.ToTextColor()}>★12.34",
                    $"<line-height=45%>91.23<size=70%>%" +
                    $"\n<size=75%><#ff3333>×123",
                    "<#ffff99>123<size=50%>.45<size=80%>pp"
                );
            });

            _view.StatusRectGradient = theme.PrimaryGrad;
            _view.StatusPpText = theme.ReplaceRichText("<$p-inv>12345<size=50%>.67<size=80%>pp");
            _view.StatusSegmentText = theme.ReplaceRichText(
                "<line-height=45%><$p-inv>Master<size=60%><$p-inv-sub> at 12:34:56" +
                "\n<$accent><size=50%>Next ⇒ <$p-inv>Grandmaster<$p-inv-sub> / 20000pp");
            _view.StatusTimeText = "13:57:09";
            _view.TopScoresButtonColor = theme.AccentColor;
            _view.RecentScoresButtonColor = "#ffffff";
            _view.PrevScoresButtonColor = "#999999";
            _view.ScoresPageText = "1";
            _view.NextScoresButtonColor = "#ffffff";
            _view.ReplaceScoreEntries(scoreEntries);
            _view.FooterRectGradient = theme.PrimaryGrad;
            _view.FooterText = theme.ReplaceRichText(
                "<$p-inv>ScoreSaber v3 Any% (092023)<$accent> / <$p-inv>Grandmaster / 10000pp");
        }
    }
}
