using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSpeedrun.Extensions;
using BeatSpeedrun.Models.Leaderboard;
using HMUI;
using UnityEngine;

namespace BeatSpeedrun.Views
{
    internal class LeaderboardMainView : BSMLView
    {
        internal const string ResourceName = "BeatSpeedrun.Source.Views.LeaderboardMain.bsml";

        [UIValue("status-header")]
        internal LeaderboardStatusHeaderView StatusHeader { get; } = new LeaderboardStatusHeaderView();

        [UIValue("side-control")]
        internal LeaderboardSideControlView SideControl { get; } = new LeaderboardSideControlView();

        [UIValue("top-control")]
        internal LeaderboardTopControlView TopControl { get; } = new LeaderboardTopControlView();

        [UIValue("cards")]
        internal LeaderboardCardView Cards { get; } = new LeaderboardCardView();

        [UIValue("scores")]
        internal LeaderboardScoresView Scores { get; } = new LeaderboardScoresView();

        [UIValue("footer")]
        internal LeaderboardFooterView Footer { get; } = new LeaderboardFooterView();

        internal void ShowCards(Action<LeaderboardCardView> render)
        {
            render(Cards);
            Cards.Show = true;
            Scores.Show = false;
        }

        internal void ShowScores(Action<LeaderboardScoresView> render)
        {
            render(Scores);
            Cards.Show = false;
            Scores.Show = true;
        }
    }

    internal class LeaderboardStatusHeaderView : BSMLView
    {
        [UIComponent("rect")]
        private readonly Backgroundable _rect;

        private (string, string) _rectGradient = ("#333333", "#666666");

        internal (string, string) RectGradient
        {
            get => _rectGradient;
            set => ChangeProperty(ref _rectGradient, value, FillRect);
        }

        private void FillRect()
        {
            _rect?.Fill(_rectGradient.Item1, _rectGradient.Item2);
        }

        [UIComponent("pp-rect")]
        private readonly Backgroundable _ppRect;

        private void FillPpRect()
        {
            _ppRect.Fill("#00000066", "#000000bb");
        }

        private string _ppText;

        [UIValue("pp-text")]
        internal string PpText
        {
            get => _ppText;
            set => ChangeProperty(ref _ppText, value);
        }

        private string _segmentText;

        [UIValue("segment-text")]
        internal string SegmentText
        {
            get => _segmentText;
            set => ChangeProperty(ref _segmentText, value);
        }

        [UIComponent("time-rect")]
        private readonly Backgroundable _timeRect;

        private void FillTimeRect()
        {
            _timeRect.Fill("#000000bb", "#00000066");
        }

        private string _timeText;

        [UIValue("time-text")]
        internal string TimeText
        {
            get => _timeText;
            set => ChangeProperty(ref _timeText, value);
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            FillRect();
            FillPpRect();
            FillTimeRect();
        }
    }

    internal class LeaderboardControlView : BSMLView
    {
        internal class ButtonGroup : BSMLView
        {
            [UIValue("buttons")]
            internal Button[] Buttons { get; }

            private bool _active;

            [UIValue("active")]
            internal bool Active
            {
                get => _active;
                set => ChangeProperty(ref _active, value);
            }

            internal ButtonGroup(params Button[] buttons)
            {
                Buttons = buttons;
            }
        }

        internal class Button : BSMLView
        {
            internal event Action OnClicked;

            private string _source;

            [UIValue("source")]
            internal string Source
            {
                get => _source;
                set => ChangeProperty(ref _source, value);
            }

            private string _color = "#ffffff";

            [UIValue("color")]
            internal string Color
            {
                get => _color;
                set => ChangeProperty(ref _color, value);
            }

            private string _hoverHint;

            [UIValue("hover-hint")]
            internal string HoverHint
            {
                get => _hoverHint;
                set => ChangeProperty(ref _hoverHint, value);
            }

            [UIAction("clicked")]
            private void Clicked()
            {
                try
                {
                    OnClicked?.Invoke();
                }
                catch (Exception ex)
                {
                    Plugin.Log.Error($"Error while invoking OnClicked event:\n{ex}");
                }
            }

            internal Button(string source, string hoverHint)
            {
                _source = source;
                _hoverHint = hoverHint;
            }
        }
    }

    internal class LeaderboardSideControlView : LeaderboardControlView
    {
        internal enum SpeedrunTab
        {
            TopScores,
            RecentScores,
            Progress,
        }

        internal event Action<SpeedrunTab> OnSpeedrunTabChanged;
        internal event Action<int> OnPageChanged;

        internal LeaderboardSideControlView()
        {
            Progress.OnClicked += () => OnSpeedrunTabChanged?.Invoke(SpeedrunTab.Progress);
            TopScores.OnClicked += () => OnSpeedrunTabChanged?.Invoke(SpeedrunTab.TopScores);
            RecentScores.OnClicked += () => OnSpeedrunTabChanged?.Invoke(SpeedrunTab.RecentScores);

            PrevPage.OnClicked += () => OnPageChanged?.Invoke(-1);
            NextPage.OnClicked += () => OnPageChanged?.Invoke(1);
        }

        [UIValue("tab-button-groups")]
        private readonly ButtonGroup[] _tabButtonGroups = new[]
        {
            new ButtonGroup(
                new Button("BeatSpeedrun.Source.Resources.running.png", "Progress"),
                new Button("BeatSpeedrun.Source.Resources.trophy.png", "Top Scores"),
                new Button("BeatSpeedrun.Source.Resources.clock.png", "Recent Scores")
            ),
        };

        [UIValue("paging-buttons")]
        private readonly Button[] _pagingButtons = new[]
        {
            new Button("▲", "Previous Page"),
            new Button("-", ""),
            new Button("▼", "Next Page"),
        };

        private Button Progress => _tabButtonGroups[0].Buttons[0];
        private Button TopScores => _tabButtonGroups[0].Buttons[1];
        private Button RecentScores => _tabButtonGroups[0].Buttons[2];

        internal void ShowSpeedrunTabButtons(SpeedrunTab tab, string color, string activeColor)
        {
            Progress.Color = tab == SpeedrunTab.Progress ? activeColor : color;
            TopScores.Color = tab == SpeedrunTab.TopScores ? activeColor : color;
            RecentScores.Color = tab == SpeedrunTab.RecentScores ? activeColor : color;
            _tabButtonGroups[0].Active = true;
        }

        private Button PrevPage => _pagingButtons[0];
        private Button CurrentPage => _pagingButtons[1];
        private Button NextPage => _pagingButtons[2];

        internal void EnablePagingButtons(int currentPage, int maxPage, string activeColor, string inactiveColor)
        {
            PrevPage.Color = 0 < currentPage ? activeColor : inactiveColor;
            CurrentPage.Source = (currentPage + 1).ToString();
            NextPage.Color = currentPage < maxPage ? activeColor : inactiveColor;
        }

        internal void DisablePagingButtons(string inactiveColor)
        {
            PrevPage.Color = inactiveColor;
            CurrentPage.Source = "-";
            NextPage.Color = inactiveColor;
        }
    }

    internal class LeaderboardTopControlView : LeaderboardControlView
    {
        internal event Action<LeaderboardSort> OnSortTypeChanged;
        internal event Action<LeaderboardIndex.Group> OnIndexGroupChanged;

        internal LeaderboardTopControlView()
        {
            Segments.OnClicked += () => OnIndexGroupChanged?.Invoke(LeaderboardIndex.Group.Segments);
            Stats.OnClicked += () => OnIndexGroupChanged?.Invoke(LeaderboardIndex.Group.Stats);

            Best.OnClicked += () => OnSortTypeChanged?.Invoke(LeaderboardSort.Best);
            Recent.OnClicked += () => OnSortTypeChanged?.Invoke(LeaderboardSort.Recent);
        }

        private bool _show;

        [UIValue("show")]
        internal bool Show
        {
            get => _show;
            set => ChangeProperty(ref _show, value);
        }

        private string _titleText;

        [UIValue("title-text")]
        internal string TitleText
        {
            get => _titleText;
            set => ChangeProperty(ref _titleText, value);
        }

        [UIValue("tab-button-groups")]
        private readonly ButtonGroup[] _tabButtonGroups = new[]
        {
            new ButtonGroup(
                new Button("BeatSpeedrun.Source.Resources.route.png", "Segments"),
                new Button("BeatSpeedrun.Source.Resources.chart.png", "Stats")),
            new ButtonGroup(
                new Button("BeatSpeedrun.Source.Resources.trophy.png", "Best"),
                new Button("BeatSpeedrun.Source.Resources.clock.png", "Recent")),
        };

        private Button Segments => _tabButtonGroups[0].Buttons[0];
        private Button Stats => _tabButtonGroups[0].Buttons[1];

        internal void ShowIndexGroupButtons(LeaderboardIndex.Group group, string color, string activeColor)
        {
            Segments.Color = group == LeaderboardIndex.Group.Segments ? activeColor : color;
            Stats.Color = group == LeaderboardIndex.Group.Stats ? activeColor : color;
            _tabButtonGroups[0].Active = true;
        }

        internal void HideIndexGroupButtons()
        {
            _tabButtonGroups[0].Active = false;
        }

        private Button Best => _tabButtonGroups[1].Buttons[0];
        private Button Recent => _tabButtonGroups[1].Buttons[1];

        internal void ShowSortButtons(LeaderboardSort sort, string color, string activeColor)
        {
            Best.Color = sort == LeaderboardSort.Best ? activeColor : color;
            Recent.Color = sort == LeaderboardSort.Recent ? activeColor : color;
            _tabButtonGroups[1].Active = true;
        }

        internal void HideSortButtons()
        {
            _tabButtonGroups[1].Active = false;
        }
    }

    internal class LeaderboardCardView : BSMLView
    {
        internal event Action<string> OnSelected;

        internal LeaderboardCardView()
        {
            _columns = new[]
            {
                new Column(this),
                new Column(this),
            };
        }

        private bool _show;

        [UIValue("show")]
        internal bool Show
        {
            get => _show;
            set => ChangeProperty(ref _show, value);
        }

        [UIValue("columns")]
        private readonly Column[] _columns;

        internal void ReplaceEntries(IEnumerable<Entry> entries)
        {
            var list = entries.ToArray();
            var c = 0;
            foreach (var column in _columns)
            {
                var i = 0;
                column.ReplaceEntries(list.Where(e => i++ % _columns.Length == c));
                c++;
            }
        }

        internal class Column : BSMLView
        {
            private readonly LeaderboardCardView _parent;

            internal Column(LeaderboardCardView parent)
            {
                _parent = parent;
            }

            [UIComponent("list")]
            private readonly CustomCellListTableData _list;

            [UIValue("entries")]
            private readonly List<object> _entries = new List<object>();

            internal void ReplaceEntries(IEnumerable<Entry> entries)
            {
                _entries.Clear();
                _entries.AddRange(entries.Where(e => e != null));
                _list?.tableView.ReloadData();
            }

            [UIAction("selected")]
            private void Selected(TableView tableView, Entry entry)
            {
                try
                {
                    _parent.OnSelected?.Invoke(entry.Id);
                }
                catch (Exception ex)
                {
                    Plugin.Log.Error($"Error while invoking OnSelected event:\n{ex}");
                }

                tableView.ClearSelection();
            }
        }

        internal class Entry : BSMLView
        {
            [UIComponent("rect")]
            private readonly Backgroundable _rect;

            [UIComponent("icon-rect")]
            private readonly Backgroundable _iconRect;

            internal string Id { get; }

            [UIValue("icon-source")]
            private string IconSource { get; }

            [UIValue("icon-color")]
            private string IconColor { get; }

            [UIValue("text")]
            private string Text { get; }

            private readonly (string, string) _rectGradient;

            private readonly (string, string) _iconRectGradient;

            internal Entry(
                string id,
                (string, string) rectGradient,
                string iconSource,
                string iconColor,
                (string, string) iconRectGradient,
                string text)
            {
                Id = id;
                _rectGradient = rectGradient;
                IconSource = iconSource;
                IconColor = iconColor;
                _iconRectGradient = iconRectGradient;
                Text = text;
            }

            [UIAction("#post-parse")]
            private void PostParse()
            {
                _rect.Fill(_rectGradient.Item1, _rectGradient.Item2);
                _iconRect.Fill(_iconRectGradient.Item1, _iconRectGradient.Item2);
            }
        }
    }

    internal class LeaderboardScoresView : BSMLView
    {
        private bool _show;

        [UIValue("show")]
        internal bool Show
        {
            get => _show;
            set => ChangeProperty(ref _show, value);
        }

        [UIComponent("list")]
        private readonly CustomCellListTableData _list;

        [UIValue("entries")]
        private readonly List<object> _entries = new List<object>();

        internal void ReplaceEntries(IEnumerable<Entry> entries)
        {
            _entries.Clear();
            _entries.AddRange(entries.Where(e => e != null));
            _list?.tableView.ReloadData();
        }

        internal class Entry : BSMLView
        {
            [UIComponent("rect")]
            private readonly Backgroundable _rect;

            [UIValue("rank")]
            private string Rank { get; }

            [UIComponent("cover")]
            private readonly ImageView _cover;

            [UIValue("title")]
            private string Title { get; }

            [UIValue("sub-title")]
            private string SubTitle { get; }

            [UIValue("difficulty")]
            private string Difficulty { get; }

            [UIValue("result")]
            private string Result { get; }

            [UIValue("meta")]
            private string Meta { get; }

            private readonly (string, string) _rectGradient;
            private readonly Task<Sprite> _coverSprite;

            internal Entry(
                (string, string) rectGradient,
                string rank,
                Task<Sprite> coverSprite,
                string title,
                string subTitle,
                string difficulty,
                string result,
                string meta)
            {
                _rectGradient = rectGradient;
                Rank = rank;
                Title = title;
                SubTitle = subTitle;
                Difficulty = difficulty;
                Result = result;
                Meta = meta;
                _coverSprite = coverSprite;
            }

            [UIAction("#post-parse")]
            private async void PostParse()
            {
                _rect.Fill(_rectGradient.Item1, _rectGradient.Item2);

                try
                {
                    var sprite = await _coverSprite;
                    if (_cover != null && sprite != null) _cover.sprite = sprite;
                }
                catch (Exception ex)
                {
                    Plugin.Log.Warn($"Failed to get cover sprite:\n{ex}");
                }
            }
        }
    }

    internal class LeaderboardFooterView : BSMLView
    {
        [UIComponent("rect")]
        private readonly Backgroundable _rect;

        private (string, string) _rectGradient = ("#333333", "#666666");

        internal (string, string) RectGradient
        {
            get => _rectGradient;
            set => ChangeProperty(ref _rectGradient, value, FillRect);
        }

        private void FillRect()
        {
            _rect?.Fill(_rectGradient.Item1, _rectGradient.Item2);
        }

        private string _text;

        [UIValue("text")]
        internal string Text
        {
            get => _text;
            set => ChangeProperty(ref _text, value);
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            FillRect();
        }
    }
}
