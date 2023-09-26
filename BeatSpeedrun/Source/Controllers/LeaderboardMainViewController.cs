using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSpeedrun.Extensions;
using BeatSpeedrun.Models;
using BeatSpeedrun.Models.Leaderboard;
using BeatSpeedrun.Models.Speedrun;
using BeatSpeedrun.Providers;
using BeatSpeedrun.Services;
using BeatSpeedrun.Controllers.Support;
using BeatSpeedrun.Views;
using SongCore;
using Zenject;

namespace BeatSpeedrun.Controllers
{
    [HotReload(RelativePathToLayout = "..\\Views\\LeaderboardMain.bsml")]
    [ViewDefinition(LeaderboardMainView.ResourceName)]
    internal class LeaderboardMainViewController : BSMLAutomaticViewController, IInitializable, IDisposable, ITickable
    {
        [Inject]
        private readonly RegulationProvider _regulationProvider;

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

        private Regulation UseRegulation(string regulation) =>
            _taskWaiter.Wait(_regulationProvider.GetAsync(regulation));

        private (Speedrun Speedrun, bool IsLoading) UseSpeedrun(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return (_speedrunFacilitator.Current, _speedrunFacilitator.IsLoading);
            }
            var speedrun = _taskWaiter.Wait(_leaderboardState.GetSpeedrunAsync(id));
            return (speedrun, speedrun != null); // TODO: distinguish load errors
        }

        private LocalLeaderboard UseLocalLeaderboard(string regulation) =>
            _taskWaiter.Wait(_leaderboardState.GetLocalLeaderboardAsync(regulation));

        private IEnumerable<(LeaderboardIndex, LeaderboardRecord)> UseLeaderboardRecords(
            string regulation,
            LeaderboardType leaderboardType,
            LeaderboardIndex.Group indexGroup,
            LeaderboardSort sort)
        {
            switch (leaderboardType)
            {
                case LeaderboardType.Local:
                    return UseLocalLeaderboard(regulation)?.QueryRecords(indexGroup, sort);
                default:
                    throw new NotImplementedException();
            }
        }

        private (IEnumerable<LeaderboardRecord> Records, int Page, int MaxPage) UseLeaderboardRecords(
            string regulation,
            LeaderboardType leaderboardType,
            LeaderboardIndex index,
            int page,
            LeaderboardSort sort)
        {
            switch (leaderboardType)
            {
                case LeaderboardType.Local:
                    var localLeaderboard = UseLocalLeaderboard(regulation);
                    if (localLeaderboard == null) return (null, page, page);
                    var records = localLeaderboard.QueryRecords(index, sort).ToList();
                    var maxPage = (records.Count - 1) / PageRecords;
                    if (page < 0) page = 0;
                    if (maxPage < page) page = maxPage;
                    return (records.Skip(page * PageRecords).Take(PageRecords), page, maxPage);
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region state

        private readonly Stack<LeaderboardState.Node> _stateStack = new Stack<LeaderboardState.Node>();

        private LeaderboardState.Node GetCurrentState()
        {
            if (_stateStack.Count == 0)
            {
                _stateStack.Push(_speedrunFacilitator.Current != null
                    ? (LeaderboardState.Node)new LeaderboardState.ShowSpeedrun()
                    : (LeaderboardState.Node)new LeaderboardState.ShowLeaderboard());
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

        private bool CanGoBack => 1 < _stateStack.Count;

        private void GoBack()
        {
            if (CanGoBack)
            {
                _stateStack.Pop();
                Render();
            }
        }

        private void GoToLeaderboard()
        {
            if (_stateStack.OfType<LeaderboardState.ShowLeaderboard>().Any())
            {
                while (_stateStack.Peek() is LeaderboardState.ShowLeaderboard)
                {
                    _stateStack.Pop();
                }
            }
            else
            {
                _stateStack.Push(new LeaderboardState.ShowLeaderboard());
            }
            Render();
        }

        private void GoToSpeedrun(string id)
        {
            if (id == null) return;

            if (_stateStack.OfType<LeaderboardState.ShowSpeedrun>().Any(s => s.Speedrun == id))
            {
                while (!(_stateStack.Peek() is LeaderboardState.ShowSpeedrun s && s.Speedrun == id))
                {
                    _stateStack.Pop();
                }
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
                case LeaderboardState.ShowLeaderboard l:
                    l.Page += offset;
                    RenderLeaderboardContents(l);
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

        private void SwitchLeaderboardType(LeaderboardType leaderboardType)
        {
            if (GetCurrentState() is LeaderboardState.ShowLeaderboard s)
            {
                s.Type = leaderboardType;
                s.Page = 0;
                RenderLeaderboardContents(s);
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
                case LeaderboardState.ShowLeaderboard l:
                    l.IndexGroup = indexGroup;
                    l.IndexKey = null;
                    RenderLeaderboardContents(l);
                    break;
            }
        }

        private void SwitchIndexKey(string indexKey)
        {
            switch (GetCurrentState())
            {
                case LeaderboardState.ShowSpeedrun s:
                    _stateStack.Push(new LeaderboardState.ShowLeaderboard
                    {
                        IndexGroup = s.ProgressIndexGroup,
                        IndexKey = indexKey,
                        Sort = LeaderboardSort.Best,
                    });
                    break;
                case LeaderboardState.ShowLeaderboard l:
                    l.IndexKey = indexKey;
                    break;
            }
            RenderContents();
        }

        private void SwitchLeaderboardSort(LeaderboardSort sort)
        {
            if (GetCurrentState() is LeaderboardState.ShowLeaderboard s)
            {
                s.Sort = sort;
                s.Page = 0;
                RenderLeaderboardContents(s);
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

                if (s.Speedrun.Progress.FinishedAt.HasValue)
                {
                    // TODO: show runner name
                    segmentText =
                        $"<line-height=45%><$main>Your<size=80%><$sub> Speedrun\n<size=60%><$accent>{s.Speedrun.Progress.StartedAt.ToRelativeString(DateTime.UtcNow)}";
                }
                else
                {
                    segmentText =
                        $"<line-height=45%><$main>{(curr.Segment is Segment c ? c.ToString() : "start")}<size=60%><$sub> at {curr.ReachedAt.Value.ToTimerString()}" +
                        (next is Progress.SegmentProgress n ? $"\n<$accent><size=50%>Next ⇒ <$main>{n.Segment}<$accent> / <$main>{n.RequiredPp}pp" : "");
                }
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
                case LeaderboardState.ShowLeaderboard l:
                    RenderLeaderboardContents(l);
                    break;
            }
        }

        private void RenderSpeedrunContents(LeaderboardState.ShowSpeedrun state)
        {
            var speedrun = UseSpeedrun(state.Speedrun).Speedrun;
            var theme = LeaderboardTheme.FromSpeedrun(speedrun);

            if (1 < _stateStack.Count)
            {
                _view.SideControl.ShowNavigationButtons(goBack: true);
                _view.SideControl.GoBack.Color = EnabledButtonColor;
            }
            else
            {
                _view.SideControl.ShowNavigationButtons(leaderboard: true);
                _view.SideControl.Leaderboard.Color = EnabledButtonColor;
            }
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

        private void RenderLeaderboardContents(LeaderboardState.ShowLeaderboard state)
        {
            var regulation = UseRegulation(_selectionState.SelectedRegulation);
            var speedrun = UseSpeedrun(GetCurrentSpeedrun()).Speedrun;
            var theme = LeaderboardTheme.FromSpeedrun(speedrun);

            _view.SideControl.ShowNavigationButtons(goBack: true);
            _view.SideControl.GoBack.Color = CanGoBack ? EnabledButtonColor : DisabledButtonColor;
            _view.SideControl.ShowLeaderboardTabButtons(state.Type, EnabledButtonColor, theme.AccentColor);

            if (regulation == null)
            {
                _view.TopControl.TitleText = "loading...";
                _view.TopControl.HideIndexGroupButtons();
                _view.TopControl.HideSortButtons();
                _view.TopControl.Show = true;
                _view.ShowLoading();
                return;
            }

            _view.TopControl.TitleText = theme.ReplaceRichText(
                $"<line-height=60%><#ffffff>Leaderboard<$accent> ≫ <#ffffff>{regulation.Title}<$accent> ≫\n<#ffffff>{state.IndexGroup}" +
                (state.IndexKey != null ? $"<$accent> ≫ <#ffffff>{state.IndexKey}" : ""));
            _view.TopControl.ShowIndexGroupButtons(state.IndexGroup, EnabledButtonColor, theme.AccentColor);
            _view.TopControl.ShowSortButtons(state.Sort, EnabledButtonColor, theme.AccentColor);
            _view.TopControl.Show = true;

            if (state.Type != LeaderboardType.Local)
            {
                _view.SideControl.DisablePagingButtons(DisabledButtonColor);
                _view.ShowLoading("coming soon!");
                return;
            }

            if (state.IndexKey == null)
            {
                var records = UseLeaderboardRecords(
                    _selectionState.SelectedRegulation,
                    state.Type,
                    state.IndexGroup,
                    state.Sort);

                _view.SideControl.DisablePagingButtons(DisabledButtonColor);
                if (records != null)
                {
                    var entries = records.Select(r => LeaderboardViewHelper.ToCardEntry(r.Item2, r.Item1));
                    _view.ShowCards(v => v.ReplaceEntries(entries));
                }
                else
                {
                    _view.ShowLoading();
                }
            }
            else
            {
                var index = LeaderboardIndex.Map[state.IndexKey];
                var (records, page, maxPage) = UseLeaderboardRecords(
                    _selectionState.SelectedRegulation,
                    state.Type,
                    index,
                    state.Page,
                    state.Sort);
                state.Page = page;

                _view.SideControl.EnablePagingButtons(state.Page, maxPage, EnabledButtonColor, DisabledButtonColor);
                if (records != null)
                {
                    var entries = records.Select(r => LeaderboardViewHelper.ToRecordEntry(r, index));
                    _view.ShowRecords(v => v.ReplaceEntries(entries));
                }
                else
                {
                    _view.ShowLoading();
                }
            }
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
            }

            _view.Footer.RectGradient = theme.Gradient;
            _view.Footer.Text = theme.ReplaceRichText(text);
        }

        #endregion

        #region callbacks

        void IInitializable.Initialize()
        {
            Loader.SongsLoadedEvent += OnSongsLoaded;
            _view.SideControl.Leaderboard.OnClicked += GoToLeaderboard;
            _view.SideControl.GoBack.OnClicked += GoBack;
            _view.SideControl.OnSpeedrunTabChanged += SwitchSpeedrunTab;
            _view.SideControl.OnLeaderboardTabChanged += SwitchLeaderboardType;
            _view.SideControl.OnPageChanged += MovePage;
            _view.TopControl.OnIndexGroupChanged += SwitchIndexGroup;
            _view.TopControl.OnSortTypeChanged += SwitchLeaderboardSort;
            _view.Records.OnSelected += OnRecordSelected;
            _view.Cards.OnSelected += OnCardSelected;
            _speedrunFacilitator.OnCurrentSpeedrunChanged += OnCurrentSpeedrunChanged;
            _speedrunFacilitator.OnCurrentSpeedrunUpdated += Render;
            _selectionState.OnRegulationSelected += OnRegulationSelected;
            Render();
        }

        void IDisposable.Dispose()
        {
            _selectionState.OnRegulationSelected -= OnRegulationSelected;
            _speedrunFacilitator.OnCurrentSpeedrunUpdated -= Render;
            _speedrunFacilitator.OnCurrentSpeedrunChanged -= OnCurrentSpeedrunChanged;
            _view.Cards.OnSelected -= OnCardSelected;
            _view.Records.OnSelected -= OnRecordSelected;
            _view.TopControl.OnSortTypeChanged -= SwitchLeaderboardSort;
            _view.TopControl.OnIndexGroupChanged -= SwitchIndexGroup;
            _view.SideControl.OnPageChanged -= MovePage;
            _view.SideControl.OnLeaderboardTabChanged -= SwitchLeaderboardType;
            _view.SideControl.OnSpeedrunTabChanged -= SwitchSpeedrunTab;
            _view.SideControl.GoBack.OnClicked -= GoBack;
            _view.SideControl.Leaderboard.OnClicked -= GoToLeaderboard;
            Loader.SongsLoadedEvent -= OnSongsLoaded;
        }

        void ITickable.Tick()
        {
            var speedrun = GetCurrentSpeedrun();
            if (speedrun == null &&
                _speedrunFacilitator.Current?.Progress.ComputeState(DateTime.UtcNow) == Progress.State.Running)
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

        private void OnCardSelected(string id) => SwitchIndexKey(id);
        private void OnRecordSelected(string id) => GoToSpeedrun(id);

        private void OnCurrentSpeedrunChanged((Speedrun, Speedrun) diff)
        {
            if (diff.Item1 != null && diff.Item2 == null && diff.Item1.SongScores.Count != 0)
            {
                _leaderboardState.ClearCaches();
                var state =
                    _stateStack.OfType<LeaderboardState.ShowSpeedrun>().FirstOrDefault(s => s.Speedrun == null) ??
                    new LeaderboardState.ShowSpeedrun();
                state.Speedrun = diff.Item1.Id;
                _stateStack.Clear();
                _stateStack.Push(new LeaderboardState.ShowLeaderboard());
                _stateStack.Push(state);
            }
            else
            {
                _stateStack.Clear();
            }
            Render();
        }

        private void OnRegulationSelected()
        {
            _stateStack.Clear();
            Render();
        }

        #endregion

        private const string EnabledButtonColor = "#ffffff";
        private const string DisabledButtonColor = "#666666";
        private const int PageRecords = 10;
        private const int PageScores = 8;
    }
}
