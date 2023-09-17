using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSpeedrun.Extensions;
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

        [UIValue("progress")]
        internal LeaderboardProgressView Progress { get; } = new LeaderboardProgressView();

        [UIValue("scores")]
        internal LeaderboardScoresView Scores { get; } = new LeaderboardScoresView();

        [UIValue("footer")]
        internal LeaderboardFooterView Footer { get; } = new LeaderboardFooterView();
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
            _rect.Fill(_rectGradient.Item1, _rectGradient.Item2);
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

    internal class LeaderboardSideControlView : BSMLView
    {
        internal event Action OnProgressSelected;
        internal event Action OnTopScoresSelected;
        internal event Action OnRecentScoresSelected;
        internal event Action OnNextScoresSelected;
        internal event Action OnPrevScoresSelected;

        private string _progressButtonColor;

        [UIValue("progress-button-color")]
        internal string ProgressButtonColor
        {
            get => _progressButtonColor;
            set => ChangeProperty(ref _progressButtonColor, value);
        }

        [UIAction("progress-button-clicked")]
        private void ProgressButtonClicked()
        {
            try
            {
                OnProgressSelected?.Invoke();
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Error while invoking OnProgressSelected event:\n{ex}");
            }
        }

        private string _topScoresButtonColor;

        [UIValue("top-scores-button-color")]
        internal string TopScoresButtonColor
        {
            get => _topScoresButtonColor;
            set => ChangeProperty(ref _topScoresButtonColor, value);
        }

        [UIAction("top-scores-button-clicked")]
        private void TopScoresButtonClicked()
        {
            try
            {
                OnTopScoresSelected?.Invoke();
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Error while invoking OnTopScoresSelected event:\n{ex}");
            }
        }

        private string _recentScoresButtonColor;

        [UIValue("recent-scores-button-color")]
        internal string RecentScoresButtonColor
        {
            get => _recentScoresButtonColor;
            set => ChangeProperty(ref _recentScoresButtonColor, value);
        }

        [UIAction("recent-scores-button-clicked")]
        private void RecentScoresButtonClicked()
        {
            try
            {
                OnRecentScoresSelected?.Invoke();
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Error while invoking OnRecentScoresSelected event:\n{ex}");
            }
        }

        private string _prevScoresButtonColor;

        [UIValue("prev-scores-button-color")]
        internal string PrevScoresButtonColor
        {
            get => _prevScoresButtonColor;
            set => ChangeProperty(ref _prevScoresButtonColor, value);
        }

        [UIAction("prev-scores-button-clicked")]
        private void PrevScoresButtonClicked()
        {
            try
            {
                OnPrevScoresSelected?.Invoke();
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Error while invoking OnPrevScoresSelected event:\n{ex}");
            }
        }

        private string _scoresPageText;

        [UIValue("scores-page-text")]
        internal string ScoresPageText
        {
            get => _scoresPageText;
            set => ChangeProperty(ref _scoresPageText, value);
        }

        private string _nextScoresButtonColor;

        [UIValue("next-scores-button-color")]
        internal string NextScoresButtonColor
        {
            get => _nextScoresButtonColor;
            set => ChangeProperty(ref _nextScoresButtonColor, value);
        }

        [UIAction("next-scores-button-clicked")]
        private void NextScoresButtonClicked()
        {
            try
            {
                OnNextScoresSelected?.Invoke();
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Error while invoking OnNextScoresSelected event:\n{ex}");
            }
        }
    }

    internal class LeaderboardProgressView : BSMLView
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
            Entry prev = null;
            foreach (var entry in entries)
            {
                if (prev != null)
                {
                    _entries.Add(new EntryPair(prev, entry));
                    prev = null;
                    continue;
                }
                prev = entry;
            }
            if (prev != null)
            {
                _entries.Add(new EntryPair(prev));
            }
            _list?.tableView.ReloadData();
        }

        internal class EntryPair : BSMLView
        {
            [UIValue("item1")]
            private readonly Entry _item1;

            [UIValue("item2")]
            private readonly Entry _item2;

            internal EntryPair(Entry item1, Entry item2)
            {
                _item1 = item1;
                _item2 = item2;
            }

            internal EntryPair(Entry item1)
            {
                _item1 = item1;
                _item2 = Entry.Inactive;
            }
        }

        internal class Entry : BSMLView
        {
            [UIComponent("rect")]
            private readonly Backgroundable _rect;

            [UIComponent("icon-rect")]
            private readonly Backgroundable _iconRect;

            [UIValue("icon-source")]
            private string IconSource { get; }

            [UIValue("icon-color")]
            private string IconColor { get; }

            [UIValue("text")]
            private string Text { get; }

            private readonly (string, string) _rectGradient;

            private readonly (string, string) _iconRectGradient;

            internal Entry(
                (string, string) rectGradient,
                string iconSource,
                string iconColor,
                (string, string) iconRectGradient,
                string text)
            {
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

            internal static readonly Entry Inactive = new Entry(
                ("#00000000", "#00000000"),
                "BeatSpeedrun.Source.Resources.route.png",
                "#00000000",
                ("#00000000", "#00000000"),
                "");
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
            _entries.AddRange(entries);
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
            _rect.Fill(_rectGradient.Item1, _rectGradient.Item2);
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

