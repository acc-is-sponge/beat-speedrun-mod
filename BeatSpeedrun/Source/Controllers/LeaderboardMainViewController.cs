using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSpeedrun.Extensions;
using BeatSpeedrun.Managers;
using BeatSpeedrun.Views;
using BeatSpeedrun.Models;
using BeatSpeedrun.Models.Speedrun;
using Zenject;
using SongCore;

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

        private enum Show
        {
            TopScores,
            RecentScores,
            Progress,
        }

        private int _scoresPage = 0;
        private Show _show = Show.TopScores;

        private void Render()
        {
            RenderStatusBar();
            RenderContents();
            RenderFooter();
        }

        private void RenderStatusBar()
        {
            var speedrun = _currentSpeedrunManager.Current;
            var theme = CurrentTheme;

            _view.StatusRectGradient = theme.Gradient;

            if (speedrun == null)
            {
                _view.StatusPpText = theme.ReplaceRichText("<$main>0<size=80%>pp");
                _view.StatusSegmentText = theme.ReplaceRichText("<$main>; )");
            }
            else
            {
                var curr = speedrun.Progress.GetCurrentSegment();
                var next = speedrun.Progress.GetNextSegment();

                _view.StatusPpText = theme.ReplaceRichText(
                    $"<$main>{speedrun.TotalPp:0.#}<size=80%>pp");
                _view.StatusSegmentText = theme.ReplaceRichText(
                    "<line-height=45%><$main>" + (curr.Segment is Segment c ? c.ToString() : "start") + $"<size=60%><$sub> at {curr.ReachedAt.Value:h\\:mm\\:ss}" +
                    (next is Progress.SegmentProgress n ? $"\n<$accent><size=50%>Next ⇒ <$main>{n.Segment}<$accent> / <$main>{n.RequiredPp:0.#}pp" : ""));
            }

            RenderStatusBarTime();
        }

        private void RenderStatusBarTime()
        {
            var speedrun = _currentSpeedrunManager.Current;
            var theme = CurrentTheme;

            if (speedrun == null)
            {
                _view.StatusTimeText = theme.ReplaceRichText($"<$main>0:00:00");
            }
            else
            {
                var time = speedrun.Progress.ElapsedTime(DateTime.UtcNow);
                _view.StatusTimeText =
                    speedrun.Progress.TimeLimit <= time
                        ? theme.ReplaceRichText($"<line-height=45%><size=70%><$main>TIME IS UP!\n<size=50%><$sub>{time:h\\:mm\\:ss}")
                        : _view.StatusTimeText = theme.ReplaceRichText($"<$main>{time:h\\:mm\\:ss}");
            }

        }

        const string EnabledButtonColor = "#ffffff";
        const string DisabledButtonColor = "#777777";

        private void RenderContents()
        {
            var speedrun = _currentSpeedrunManager.Current;
            var theme = CurrentTheme;

            if (speedrun == null)
            {
                _view.ProgressButtonColor = DisabledButtonColor;
                _view.TopScoresButtonColor = DisabledButtonColor;
                _view.RecentScoresButtonColor = DisabledButtonColor;
                _view.PrevScoresButtonColor = DisabledButtonColor;
                _view.NextScoresButtonColor = DisabledButtonColor;
                _view.ScoresPageText = "-";
                _view.ShowProgress = false;
                _view.ShowScores = false;
                return;
            }

            _view.ProgressButtonColor = _show == Show.Progress ? theme.AccentColor : EnabledButtonColor;
            _view.TopScoresButtonColor = _show == Show.TopScores ? theme.AccentColor : EnabledButtonColor;
            _view.RecentScoresButtonColor = _show == Show.RecentScores ? theme.AccentColor : EnabledButtonColor;

            if (_show == Show.Progress)
            {
                RenderProgress(speedrun);
            }
            else
            {
                RenderScores(speedrun);
            }
        }

        private void RenderProgress(Speedrun speedrun)
        {
            _view.ScoresPageText = "-";

            var progressEntries = new List<LeaderboardMainView.ProgressEntry>();

            foreach (var p in speedrun.Progress.Segments)
            {
                if (!p.Segment.HasValue) continue;

                var theme = LeaderboardTheme.FromSegment(p.Segment);
                var rectGradient = p.ReachedAt.HasValue
                    ? theme.Gradient
                    : LeaderboardTheme.NotRunning.Gradient;
                var iconColor = p.ReachedAt.HasValue
                    ? theme.IconColor
                    : "#666666";
                var text = p.ReachedAt is TimeSpan at
                    ? $"<line-height=70%><$main>{p.Segment}<size=80%><$accent> / <$main>{p.RequiredPp:0.#}pp"
                        + $"\n<size=80%><$accent>reached at <$main>{at:h\\:mm\\:ss}"
                    : $"<#aaaaaa>{p.Segment}<size=80%><$icon> / <#888888>{p.RequiredPp:0.#}pp";

                progressEntries.Add(new LeaderboardMainView.ProgressEntry(
                    rectGradient,
                    theme.IconSource,
                    iconColor,
                    ("#000000aa", "#000000dd"),
                    theme.ReplaceRichText(text)));

                if (p.Segment == speedrun.Progress.TargetSegment?.Segment) break;
            }

            _view.ShowProgress = true;
            _view.ShowScores = false;
            _view.ReplaceProgressEntries(progressEntries);
        }

        private void RenderScores(Speedrun speedrun)
        {
            var scoresCount = _show == Show.TopScores
                ? speedrun.TopScores.Count
                : speedrun.SongScores.Count;
            if (scoresCount == 0)
            {
                _view.PrevScoresButtonColor = DisabledButtonColor;
                _view.NextScoresButtonColor = DisabledButtonColor;
                _view.ScoresPageText = "1";
                _view.ShowProgress = false;
                _view.ShowScores = false;
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

            var scores = _show == Show.TopScores
                ? speedrun.TopScores.Skip(_scoresPage * 8).Take(8)
                : Enumerable.Reverse(speedrun.SongScores).Skip(_scoresPage * 8).Take(8);

            var scoreEntries = scores.Select(score =>
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

                return new LeaderboardMainView.ScoreEntry(
                    rectGradient, rank, cover, title, subTitle, difficulty, result, meta);
            });

            _view.ShowProgress = false;
            _view.ShowScores = true;
            _view.ReplaceScoreEntries(scoreEntries);
        }

        private void RenderScoresForSongCore(
            Loader _loader,
            ConcurrentDictionary<string, CustomPreviewBeatmapLevel> _dictionary)
        {
            RenderContents();
        }

        private void RenderFooter()
        {
            var speedrun = _currentSpeedrunManager.Current;
            var theme = CurrentTheme;

            _view.FooterRectGradient = theme.Gradient;

            if (speedrun == null)
            {
                _view.FooterText = theme.ReplaceRichText(
                    "<$main>You can start your speedrun on the Beat Speedrun MOD tab");
            }
            else
            {
                var title = Regulation.IsCustomPath(speedrun.RegulationPath)
                    ? "<$accent>(custom) <$main>" + speedrun.Regulation.Title
                    : "<$main>" + speedrun.Regulation.Title;
                var target = speedrun.Progress.TargetSegment is Progress.SegmentProgress t
                    ? $"<$main>{t.Segment}<$accent> / <$main>{t.RequiredPp:0.#}pp"
                    : "<$main>endless";
                _view.FooterText = theme.ReplaceRichText($"{title}<$accent> / {target}");
            }
        }

        public void Initialize()
        {
            Render();
            Loader.SongsLoadedEvent += RenderScoresForSongCore;
            _view.OnNextScoresSelected += ShowNextScores;
            _view.OnPrevScoresSelected += ShowPrevScores;
            _view.OnProgressSelected += ShowProgress;
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
            _view.OnProgressSelected -= ShowProgress;
            _view.OnPrevScoresSelected -= ShowPrevScores;
            _view.OnNextScoresSelected -= ShowNextScores;
            Loader.SongsLoadedEvent -= RenderScoresForSongCore;
        }

        public void Tick()
        {
            var speedrun = _currentSpeedrunManager.Current;
            if (speedrun == null || speedrun.Progress.IsTargetReached) return;

            RenderStatusBarTime();
        }

        internal void ShowNextScores()
        {
            _scoresPage++;
            RenderContents();
        }

        internal void ShowPrevScores()
        {
            _scoresPage--;
            RenderContents();
        }

        internal void ShowProgress()
        {
            _show = Show.Progress;
            _scoresPage = 0;
            RenderContents();
        }

        internal void ShowTopScores()
        {
            _show = Show.TopScores;
            _scoresPage = 0;
            RenderContents();
        }

        internal void ShowRecentScores()
        {
            _show = Show.RecentScores;
            _scoresPage = 0;
            RenderContents();
        }
    }
}
