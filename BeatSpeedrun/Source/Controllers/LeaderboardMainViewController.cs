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
            RenderStatusHeader();
            RenderContents();
            RenderFooter();
        }

        private void RenderStatusHeader()
        {
            var speedrun = _currentSpeedrunManager.Current;
            var theme = CurrentTheme;
            var view = _view.StatusHeader;

            view.RectGradient = theme.Gradient;

            if (speedrun == null)
            {
                view.PpText = theme.ReplaceRichText("<$main>0<size=80%>pp");
                view.SegmentText = theme.ReplaceRichText("<$main>; )");
            }
            else
            {
                var curr = speedrun.Progress.GetCurrentSegment();
                var next = speedrun.Progress.GetNextSegment();

                view.PpText = theme.ReplaceRichText(
                    $"<$main>{speedrun.TotalPp:0.#}<size=80%>pp");
                view.SegmentText = theme.ReplaceRichText(
                    "<line-height=45%><$main>" + (curr.Segment is Segment c ? c.ToString() : "start") + $"<size=60%><$sub> at {curr.ReachedAt.Value:h\\:mm\\:ss}" +
                    (next is Progress.SegmentProgress n ? $"\n<$accent><size=50%>Next ⇒ <$main>{n.Segment}<$accent> / <$main>{n.RequiredPp}pp" : ""));
            }

            RenderStatusHeaderTime();
        }

        private void RenderStatusHeaderTime()
        {
            var speedrun = _currentSpeedrunManager.Current;
            var theme = CurrentTheme;
            var view = _view.StatusHeader;

            if (speedrun == null)
            {
                view.TimeText = theme.ReplaceRichText($"<$main>0:00:00");
            }
            else
            {
                var time = speedrun.Progress.ElapsedTime(DateTime.UtcNow);
                view.TimeText =
                    speedrun.Progress.TimeLimit <= time
                        ? theme.ReplaceRichText($"<line-height=45%><size=70%><$main>TIME IS UP!\n<size=50%><$sub>{time:h\\:mm\\:ss}")
                        : theme.ReplaceRichText($"<$main>{time:h\\:mm\\:ss}");
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
                _view.SideControl.ProgressButtonColor = DisabledButtonColor;
                _view.SideControl.TopScoresButtonColor = DisabledButtonColor;
                _view.SideControl.RecentScoresButtonColor = DisabledButtonColor;
                _view.SideControl.PrevScoresButtonColor = DisabledButtonColor;
                _view.SideControl.NextScoresButtonColor = DisabledButtonColor;
                _view.SideControl.ScoresPageText = "-";
                _view.Progress.Show = false;
                _view.Scores.Show = false;
                return;
            }

            _view.SideControl.ProgressButtonColor = _show == Show.Progress ? theme.AccentColor : EnabledButtonColor;
            _view.SideControl.TopScoresButtonColor = _show == Show.TopScores ? theme.AccentColor : EnabledButtonColor;
            _view.SideControl.RecentScoresButtonColor = _show == Show.RecentScores ? theme.AccentColor : EnabledButtonColor;

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
            var progressEntries = new List<LeaderboardProgressView.Entry>();

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
                    ? $"<line-height=70%><$main>{p.Segment}<size=80%><$accent> / <$main>{p.RequiredPp}pp"
                        + $"\n<size=80%><$accent>reached at <$main>{at:h\\:mm\\:ss}"
                    : $"<#aaaaaa>{p.Segment}<size=80%><$icon> / <#888888>{p.RequiredPp}pp";

                progressEntries.Add(new LeaderboardProgressView.Entry(
                    rectGradient,
                    theme.IconSource,
                    iconColor,
                    ("#000000aa", "#000000dd"),
                    theme.ReplaceRichText(text)));

                if (p.Segment == speedrun.Progress.TargetSegment?.Segment) break;
            }

            _view.SideControl.ScoresPageText = "-";
            _view.Scores.Show = false;
            _view.Progress.Show = true;
            _view.Progress.ReplaceEntries(progressEntries);
        }

        private void RenderScores(Speedrun speedrun)
        {
            var scoresCount = _show == Show.TopScores
                ? speedrun.TopScores.Count
                : speedrun.SongScores.Count;
            if (scoresCount == 0)
            {
                _view.SideControl.PrevScoresButtonColor = DisabledButtonColor;
                _view.SideControl.NextScoresButtonColor = DisabledButtonColor;
                _view.SideControl.ScoresPageText = "1";
                _view.Progress.Show = false;
                _view.Scores.Show = false;
                return;
            }

            var maxPage = (scoresCount - 1) / 8;
            if (_scoresPage < 0) _scoresPage = 0;
            if (maxPage < _scoresPage) _scoresPage = maxPage;

            _view.SideControl.PrevScoresButtonColor =
                0 < _scoresPage ? EnabledButtonColor : DisabledButtonColor;
            _view.SideControl.NextScoresButtonColor =
                _scoresPage < maxPage ? EnabledButtonColor : DisabledButtonColor;
            _view.SideControl.ScoresPageText = (_scoresPage + 1).ToString();

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

                return new LeaderboardScoresView.Entry(
                    rectGradient, rank, cover, title, subTitle, difficulty, result, meta);
            });

            _view.Progress.Show = false;
            _view.Scores.Show = true;
            _view.Scores.ReplaceEntries(scoreEntries);
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
            var view = _view.Footer;

            view.RectGradient = theme.Gradient;

            if (speedrun == null)
            {
                view.Text = theme.ReplaceRichText(
                    "<$main>You can start your speedrun on the Beat Speedrun MOD tab");
            }
            else
            {
                var title = Regulation.IsCustomPath(speedrun.RegulationPath)
                    ? "<$accent>(custom) <$main>" + speedrun.Regulation.Title
                    : "<$main>" + speedrun.Regulation.Title;
                var target = speedrun.Progress.TargetSegment is Progress.SegmentProgress t
                    ? $"<$main>{t.Segment}<$accent> / <$main>{t.RequiredPp}pp"
                    : "<$main>endless";
                view.Text = theme.ReplaceRichText($"{title}<$accent> / {target}");
            }
        }

        public void Initialize()
        {
            Render();
            Loader.SongsLoadedEvent += RenderScoresForSongCore;
            _view.SideControl.OnNextScoresSelected += ShowNextScores;
            _view.SideControl.OnPrevScoresSelected += ShowPrevScores;
            _view.SideControl.OnProgressSelected += ShowProgress;
            _view.SideControl.OnTopScoresSelected += ShowTopScores;
            _view.SideControl.OnRecentScoresSelected += ShowRecentScores;
            _currentSpeedrunManager.OnCurrentSpeedrunChanged += Render;
            _currentSpeedrunManager.OnCurrentSpeedrunUpdated += Render;
        }

        public void Dispose()
        {
            _currentSpeedrunManager.OnCurrentSpeedrunUpdated -= Render;
            _currentSpeedrunManager.OnCurrentSpeedrunChanged -= Render;
            _view.SideControl.OnRecentScoresSelected -= ShowRecentScores;
            _view.SideControl.OnTopScoresSelected -= ShowTopScores;
            _view.SideControl.OnProgressSelected -= ShowProgress;
            _view.SideControl.OnPrevScoresSelected -= ShowPrevScores;
            _view.SideControl.OnNextScoresSelected -= ShowNextScores;
            Loader.SongsLoadedEvent -= RenderScoresForSongCore;
        }

        public void Tick()
        {
            var speedrun = _currentSpeedrunManager.Current;
            if (speedrun == null || speedrun.Progress.IsTargetReached) return;

            RenderStatusHeaderTime();
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
