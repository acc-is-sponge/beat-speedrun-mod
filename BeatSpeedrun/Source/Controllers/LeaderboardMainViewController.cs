using System;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSpeedrun.Extensions;
using BeatSpeedrun.Managers;
using BeatSpeedrun.Views;
using BeatSpeedrun.Models.Speedrun;
using Zenject;
using SongCore;
using System.Collections.Concurrent;
using BeatSpeedrun.Models;

namespace BeatSpeedrun.Controllers
{
    [HotReload(RelativePathToLayout = "..\\Views\\LeaderboardMain.bsml")]
    [ViewDefinition(LeaderboardMainView.ResourceName)]
    internal class LeaderboardMainViewController : BSMLAutomaticViewController, IInitializable, IDisposable, ITickable
    {
        [Inject]
        private readonly CurrentSpeedrunManager _currentSpeedrunManager;

        [UIValue("view")]
        private readonly LeaderboardMainView _view = new LeaderboardMainView();

        private LeaderboardTheme CurrentTheme =>
            _currentSpeedrunManager.Current is Speedrun speedrun
                ? LeaderboardTheme.FromSegment(speedrun.Progress.GetCurrentSegment().Segment)
                : LeaderboardTheme.NotRunning;

        private int _scoresPage = 0;
        private bool _showsTopScores = true;

        private void Render()
        {
            RenderStatusBar();
            RenderScores();
            RenderFooter();
        }

        private void RenderStatusBar()
        {
            var speedrun = _currentSpeedrunManager.Current;
            var theme = CurrentTheme;

            _view.StatusRectGradient = theme.PrimaryGrad;

            if (speedrun == null)
            {
                _view.StatusPpText = theme.ReplaceRichText("<$p-inv>0<size=80%>pp");
                _view.StatusSegmentText = theme.ReplaceRichText("<$p-inv>; )");
                _view.StatusTimeText = theme.ReplaceRichText("<$p-inv>0:00:00");
            }
            else
            {
                var curr = speedrun.Progress.GetCurrentSegment();
                var next = speedrun.Progress.GetNextSegment();
                var time = speedrun.Progress.ElapsedTime(DateTime.UtcNow);

                _view.StatusPpText = theme.ReplaceRichText(
                    $"<$p-inv>{speedrun.TotalPp.ToString("0.##").Replace(".", "<size=50%>.")}<size=80%>pp");
                _view.StatusSegmentText = theme.ReplaceRichText(
                    "<line-height=45%><$p-inv>" + (curr.Segment is Segment c ? c.ToString() : "start") + $"<size=60%><$p-inv-sub> at {curr.ReachedAt.Value:h\\:mm\\:ss}" +
                    (next is Progress.SegmentProgress n ? $"\n<$accent><size=50%>Next ⇒ <$p-inv>{n.Segment}<$p-inv-sub> / {n.RequiredPp:0.##}pp" : ""));
                _view.StatusTimeText = theme.ReplaceRichText($"<$p-inv>{time:h\\:mm\\:ss}");
            }
        }

        private void RenderStatusBarTime()
        {
            var speedrun = _currentSpeedrunManager.Current;
            var theme = CurrentTheme;

            if (speedrun == null || speedrun.Progress.IsTargetReached) return;
            var time = speedrun.Progress.ElapsedTime(DateTime.UtcNow);
            _view.StatusTimeText = theme.ReplaceRichText($"<$p-inv>{time:h\\:mm\\:ss}");
        }

        const string EnabledButtonColor = "#ffffff";
        const string DisabledButtonColor = "#777777";

        private void RenderScores()
        {
            var speedrun = _currentSpeedrunManager.Current;
            var theme = CurrentTheme;

            if (speedrun == null)
            {
                _view.TopScoresButtonColor = DisabledButtonColor;
                _view.RecentScoresButtonColor = DisabledButtonColor;
                _view.PrevScoresButtonColor = DisabledButtonColor;
                _view.NextScoresButtonColor = DisabledButtonColor;
                _view.ScoresPageText = "-";
                _view.ReplaceScoreEntries(Enumerable.Empty<LeaderboardMainView.ScoreEntry>());
                return;
            }

            _view.TopScoresButtonColor = _showsTopScores ? theme.AccentColor : EnabledButtonColor;
            _view.RecentScoresButtonColor = !_showsTopScores ? theme.AccentColor : EnabledButtonColor;

            var scoresCount = _showsTopScores ? speedrun.TopScores.Count : speedrun.SongScores.Count;
            if (scoresCount == 0)
            {
                _view.PrevScoresButtonColor = DisabledButtonColor;
                _view.NextScoresButtonColor = DisabledButtonColor;
                _view.ScoresPageText = "1";
                _view.ReplaceScoreEntries(Enumerable.Empty<LeaderboardMainView.ScoreEntry>());
                return;
            }

            var maxPage = (scoresCount - 1) / 8;
            if (_scoresPage < 0) _scoresPage = 0;
            if (maxPage < _scoresPage) _scoresPage = maxPage;

            _view.PrevScoresButtonColor =
                0 < _scoresPage ? EnabledButtonColor : DisabledButtonColor;
            _view.NextScoresButtonColor =
                _scoresPage < maxPage ? EnabledButtonColor : DisabledButtonColor;
            _view.ScoresPageText = (_scoresPage + 1).ToString();

            var scores = _showsTopScores
                ? speedrun.TopScores.Skip(_scoresPage * 8).Take(8)
                : Enumerable.Reverse(speedrun.SongScores).Skip(_scoresPage * 8).Take(8);

            var scoreEntries = scores.Select(score =>
            {
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
                    ? $"<{(score.Rank.HasValue ? "#ffff99" : "#777777")}>{score.Pp.ToString("0.##").Replace(".", "<size=50%>.")}<size=80%>pp"
                    : "<#777777>---";

                return new LeaderboardMainView.ScoreEntry(rank, cover, title, subTitle, difficulty, result, meta);
            });

            _view.ReplaceScoreEntries(scoreEntries);
        }

        private void RenderScoresForSongCore(
            Loader _loader,
            ConcurrentDictionary<string, CustomPreviewBeatmapLevel> _dictionary)
        {
            RenderScores();
        }

        private void RenderFooter()
        {
            var speedrun = _currentSpeedrunManager.Current;
            var theme = CurrentTheme;

            _view.FooterRectGradient = theme.PrimaryGrad;

            if (speedrun == null)
            {
                _view.FooterText = theme.ReplaceRichText(
                    "<$p-inv>You can start your speedrun on the Beat Speedrun MOD tab");
            }
            else
            {
                _view.FooterText = theme.ReplaceRichText(
                    $"<$p-inv>{speedrun.Regulation.Title}<$accent> / <$p-inv>" +
                    (speedrun.Progress.TargetSegment is Progress.SegmentProgress t ? $"{t.Segment}<$accent> / <$p-inv>{t.RequiredPp:0.##}pp" : "no segment")
                );
            }
        }

        public void Initialize()
        {
            Render();
            Loader.SongsLoadedEvent += RenderScoresForSongCore;
            _view.OnNextScoresSelected += ShowNextScores;
            _view.OnPrevScoresSelected += ShowPrevScores;
            _view.OnTopScoresSelected += ShowTopScores;
            _view.OnRecentScoresSelected += ShowRecentScores;
            _currentSpeedrunManager.OnCurrentSpeedrunChanged += Render;
            _currentSpeedrunManager.OnCurrentSpeedrunUpdated += Render;
        }

        public void Dispose()
        {
            _currentSpeedrunManager.OnCurrentSpeedrunUpdated -= Render;
            _currentSpeedrunManager.OnCurrentSpeedrunChanged -= Render;
            _view.OnRecentScoresSelected -= ShowRecentScores;
            _view.OnTopScoresSelected -= ShowTopScores;
            _view.OnPrevScoresSelected -= ShowPrevScores;
            _view.OnNextScoresSelected -= ShowNextScores;
            Loader.SongsLoadedEvent -= RenderScoresForSongCore;
        }

        public void Tick()
        {
            RenderStatusBarTime();
        }

        internal void ShowNextScores()
        {
            _scoresPage++;
            RenderScores();
        }

        internal void ShowPrevScores()
        {
            _scoresPage--;
            RenderScores();
        }

        internal void ShowTopScores()
        {
            _showsTopScores = true;
            _scoresPage = 0;
            RenderScores();
        }

        internal void ShowRecentScores()
        {
            _showsTopScores = false;
            _scoresPage = 0;
            RenderScores();
        }
    }
}
