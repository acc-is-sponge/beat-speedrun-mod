using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSpeedrun.Extensions;
using BeatSpeedrun.Views;
using BeatSpeedrun.Models;
using BeatSpeedrun.Models.Leaderboard;
using BeatSpeedrun.Models.Speedrun;
using BeatSpeedrun.Services;
using BeatSpeedrun.Controllers.Support;
using SongCore;
using Zenject;

namespace BeatSpeedrun.Controllers
{
    [HotReload(RelativePathToLayout = "..\\Views\\LeaderboardMain.bsml")]
    [ViewDefinition(LeaderboardMainView.ResourceName)]
    internal class LeaderboardMainViewController : BSMLAutomaticViewController, IInitializable, IDisposable, ITickable
    {
        [Inject]
        private readonly SpeedrunFacilitator _speedrunFacilitator;

        [Inject]
        private readonly LeaderboardState _leaderboardState;

        [Inject]
        private readonly SelectionState _selectionState;

        private readonly TaskWaiter _taskWaiter;

        internal LeaderboardMainViewController()
        {
            _taskWaiter = new TaskWaiter(Render);
        }

        #region hooks

        private (Speedrun Speedrun, bool IsLoading) UseSpeedrun(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return (_speedrunFacilitator.Current, _speedrunFacilitator.IsLoading);
            }
            var speedrun = _taskWaiter.Wait(_leaderboardState.GetSpeedrunAsync(id));
            return (speedrun, speedrun != null); // TODO: distinguish load errors
        }

        #endregion

        #region state

        private readonly Stack<LeaderboardState.Node> _stateStack = new Stack<LeaderboardState.Node>();

        private LeaderboardState.Node GetCurrentState()
        {
            if (_stateStack.Count == 0)
            {
                _stateStack.Push(new LeaderboardState.ShowSpeedrun());
            }
            return _stateStack.Peek();
        }

        // Used by Render methods that always display speedrun status
        private string GetCurrentSpeedrun()
        {
            return GetCurrentState() is LeaderboardState.ShowSpeedrun s
                ? s.Speedrun
                : null;
        }

        private void MoveToSpeedrun(string id)
        {
            if (string.IsNullOrEmpty(id) && _speedrunFacilitator.Current == null)
            {
                return;
            }

            if (_stateStack.Any(state => state is LeaderboardState.ShowSpeedrun s && s.Speedrun == id))
            {
                // go back to the speedrun state
                while (!(_stateStack.Peek() is LeaderboardState.ShowSpeedrun s && s.Speedrun == id)) _stateStack.Pop();
            }
            else
            {
                _stateStack.Push(new LeaderboardState.ShowSpeedrun { Speedrun = id });
            }
            Render();
        }

        private void MovePage(int offset)
        {
            switch (GetCurrentState())
            {
                case LeaderboardState.ShowSpeedrun s:
                    s.Page += offset;
                    RenderSpeedrunContents(s);
                    break;
            }
        }

        private void SwitchSpeedrunTab(LeaderboardSideControlView.SpeedrunTab tab)
        {
            if (GetCurrentState() is LeaderboardState.ShowSpeedrun s)
            {
                s.Tab = tab;
                s.Page = 0;
                RenderSpeedrunContents(s);
            }
        }

        private void SwitchIndexGroup(LeaderboardIndex.Group indexGroup)
        {
            switch (GetCurrentState())
            {
                case LeaderboardState.ShowSpeedrun s:
                    s.ProgressIndexGroup = indexGroup;
                    RenderSpeedrunContents(s);
                    break;
            }
        }

        #endregion

        #region render

        [UIValue("view")]
        private readonly LeaderboardMainView _view = new LeaderboardMainView();

        private void Render()
        {
            RenderHeader();
            RenderContents();
            RenderFooter();
        }

        private void RenderHeader()
        {
            var s = UseSpeedrun(GetCurrentSpeedrun());
            var theme = LeaderboardTheme.FromSpeedrun(s.Speedrun);

            string ppText;
            string segmentText;
            if (s.Speedrun == null)
            {
                ppText = "<$sub>0pp";
                segmentText = "<size=70%><$main>" + (s.IsLoading ? "loading..." : "(not running)");
            }
            else
            {
                var curr = s.Speedrun.Progress.Current;
                var next = s.Speedrun.Progress.FinishedAt.HasValue ? null : s.Speedrun.Progress.Next;

                ppText = $"<$main>{s.Speedrun.TotalPp:0.#}<size=80%>pp";
                segmentText =
                    "<line-height=45%><$main>" + (curr.Segment is Segment c ? c.ToString() : "start") + $"<size=60%><$sub> at {curr.ReachedAt.Value.ToTimerString()}" +
                    (next is Progress.SegmentProgress n ? $"\n<$accent><size=50%>Next ⇒ <$main>{n.Segment}<$accent> / <$main>{n.RequiredPp}pp" : "");

                // TODO: show runner name
            }

            _view.StatusHeader.RectGradient = theme.Gradient;
            _view.StatusHeader.PpText = theme.ReplaceRichText(ppText);
            _view.StatusHeader.SegmentText = theme.ReplaceRichText(segmentText);
            RenderStatusHeaderTime();
        }

        private void RenderStatusHeaderTime()
        {
            var speedrun = UseSpeedrun(GetCurrentSpeedrun()).Speedrun;
            var theme = LeaderboardTheme.FromSpeedrun(speedrun);

            string text;
            if (speedrun != null)
            {
                var now = DateTime.UtcNow;
                var time = speedrun.Progress.ElapsedTime(now);
                if (speedrun.Progress.ComputeState(now) == Progress.State.TimeIsUp &&
                    !speedrun.Progress.FinishedAt.HasValue)
                {
                    text = $"<line-height=40%><size=80%><$main>TIME IS UP!\n<size=60%><$sub>{time.ToTimerString()}";
                }
                else
                {
                    text = $"<$main>{time.ToTimerString()}";
                }
            }
            else
            {
                text = "<$sub>0:00:00";
            }
            _view.StatusHeader.TimeText = theme.ReplaceRichText(text);
        }

        private void RenderContents()
        {
            switch (GetCurrentState())
            {
                case LeaderboardState.ShowSpeedrun s:
                    RenderSpeedrunContents(s);
                    break;
            }
        }

        private void RenderSpeedrunContents(LeaderboardState.ShowSpeedrun state)
        {
            var speedrun = UseSpeedrun(state.Speedrun).Speedrun;
            var theme = LeaderboardTheme.FromSpeedrun(speedrun);

            _view.SideControl.ShowSpeedrunTabButtons(state.Tab, EnabledButtonColor, theme.AccentColor);
            _view.TopControl.Show = false;

            switch (state.Tab)
            {
                case LeaderboardSideControlView.SpeedrunTab.Progress:
                    RenderSpeedrunProgressContents(state);
                    break;
                case LeaderboardSideControlView.SpeedrunTab.TopScores:
                    RenderSpeedrunScoresContents(
                        state,
                        speedrun?.TopScores.Count ?? 0,
                        speedrun?.TopScores ?? Enumerable.Empty<SongScore>());
                    break;
                case LeaderboardSideControlView.SpeedrunTab.RecentScores:
                    RenderSpeedrunScoresContents(
                        state,
                        speedrun?.SongScores.Count ?? 0,
                        speedrun != null ? Enumerable.Reverse(speedrun.SongScores) : Enumerable.Empty<SongScore>());
                    break;
            }
        }

        private void RenderSpeedrunProgressContents(LeaderboardState.ShowSpeedrun state)
        {
            var speedrun = UseSpeedrun(state.Speedrun).Speedrun;
            var theme = LeaderboardTheme.FromSpeedrun(speedrun);
            var entries = LeaderboardIndex.List(state.ProgressIndexGroup)
                .Select(index => LeaderboardViewHelper.ToProgressCardEntry(speedrun, index));

            _view.SideControl.DisablePagingButtons(DisabledButtonColor);
            _view.TopControl.TitleText = theme.ReplaceRichText(
                $"<#ffffff>This Speedrun<$accent> ≫ <#ffffff>{state.ProgressIndexGroup}");
            _view.TopControl.ShowIndexGroupButtons(state.ProgressIndexGroup, EnabledButtonColor, theme.AccentColor);
            _view.TopControl.HideSortButtons();
            _view.TopControl.Show = true;
            _view.ShowCards(v => v.ReplaceEntries(entries));
        }

        private void RenderSpeedrunScoresContents(
            LeaderboardState.ShowSpeedrun state,
            int scoresCount,
            IEnumerable<SongScore> scores)
        {
            if (scoresCount == 0)
            {
                _view.SideControl.DisablePagingButtons(DisabledButtonColor);
                _view.ShowScores(v => v.ReplaceEntries(Enumerable.Empty<LeaderboardScoresView.Entry>()));
                return;
            }

            var maxPage = (scoresCount - 1) / PageScores;
            if (state.Page < 0) state.Page = 0;
            if (maxPage < state.Page) state.Page = maxPage;

            var entries = scores.Skip(state.Page * PageScores).Take(PageScores)
                .Select(LeaderboardViewHelper.ToScoreEntry);

            _view.SideControl.EnablePagingButtons(state.Page, maxPage, EnabledButtonColor, DisabledButtonColor);
            _view.ShowScores(v => v.ReplaceEntries(entries));
        }

        private void RenderFooter()
        {
            var s = UseSpeedrun(GetCurrentSpeedrun());
            var theme = LeaderboardTheme.FromSpeedrun(s.Speedrun);

            string text;

            if (s.Speedrun == null)
            {
                text = s.IsLoading
                    ? "<$main>loading..."
                    : "<$main>You can start your speedrun on the Beat Speedrun MOD tab";
            }
            else
            {
                text = Regulation.IsCustomPath(s.Speedrun.RegulationPath)
                    ? "<$accent>(custom) <$main>" + s.Speedrun.Regulation.Title
                    : "<$main>" + s.Speedrun.Regulation.Title;
                text += "<$accent> ::: ";
                text += s.Speedrun.Progress.Target is Progress.SegmentProgress t
                    ? $"<$main>{t.Segment}<$accent> / <$main>{t.RequiredPp}pp"
                    : "<$main>endless";
                if (s.Speedrun.Progress.FinishedAt.HasValue)
                {
                    text += "<$accent> ::: ";
                    text += $"<$main>{s.Speedrun.Progress.StartedAt.ToRelativeString(DateTime.UtcNow)}";
                }
            }

            _view.Footer.RectGradient = theme.Gradient;
            _view.Footer.Text = theme.ReplaceRichText(text);
        }

        #endregion

        #region callbacks

        void IInitializable.Initialize()
        {
            Loader.SongsLoadedEvent += OnSongsLoaded;
            _view.SideControl.OnSpeedrunTabChanged += SwitchSpeedrunTab;
            _view.SideControl.OnPageChanged += MovePage;
            _view.TopControl.OnIndexGroupChanged += SwitchIndexGroup;
            _view.Cards.OnSelected += OnCardSelected;
            _speedrunFacilitator.OnCurrentSpeedrunChanged += Render;
            _speedrunFacilitator.OnCurrentSpeedrunUpdated += Render;
            _selectionState.OnRegulationSelected += Render;
            Render();
        }

        void IDisposable.Dispose()
        {
            _selectionState.OnRegulationSelected -= Render;
            _speedrunFacilitator.OnCurrentSpeedrunUpdated -= Render;
            _speedrunFacilitator.OnCurrentSpeedrunChanged -= Render;
            _view.Cards.OnSelected -= OnCardSelected;
            _view.TopControl.OnIndexGroupChanged -= SwitchIndexGroup;
            _view.SideControl.OnPageChanged -= MovePage;
            _view.SideControl.OnSpeedrunTabChanged -= SwitchSpeedrunTab;
            Loader.SongsLoadedEvent -= OnSongsLoaded;
        }

        void ITickable.Tick()
        {
            if (GetCurrentState() is LeaderboardState.ShowSpeedrun s && s.Speedrun == null)
            {
                RenderStatusHeaderTime();
            }
        }

        private void OnSongsLoaded(
            Loader _loader,
            ConcurrentDictionary<string, CustomPreviewBeatmapLevel> _dictionary)
        {
            Render();
        }

        private void OnCardSelected(string id) { /* TODO */ }

        #endregion

        private const string EnabledButtonColor = "#ffffff";
        private const string DisabledButtonColor = "#777777";
        private const int PageScores = 8;
    }
}
