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

        internal event Action OnTopScoresSelected;
        internal event Action OnRecentScoresSelected;
        internal event Action OnNextScoresSelected;
        internal event Action OnPrevScoresSelected;

        [UIAction("#post-parse")]
        private void PostParse()
        {
            FillStatusRect();
            FillStatusPpRect();
            FillStatusTimeRect();
            FillFooterRect();
        }

        // ----- STATUS BAR -----

        [UIComponent("status-rect")]
        private readonly Backgroundable _statusRect;

        private (string, string) _statusRectGradient = ("#333333", "#666666");

        internal (string, string) StatusRectGradient
        {
            get => _statusRectGradient;
            set => ChangeProperty(ref _statusRectGradient, value, FillStatusRect);
        }

        private void FillStatusRect()
        {
            _statusRect.Fill(_statusRectGradient.Item1, _statusRectGradient.Item2);
        }

        [UIComponent("status-pp-rect")]
        private readonly Backgroundable _statusPpRect;

        private void FillStatusPpRect()
        {
            _statusPpRect.Fill("#00000066", "#000000bb");
        }

        private string _statusPpText;

        [UIValue("status-pp-text")]
        internal string StatusPpText
        {
            get => _statusPpText;
            set => ChangeProperty(ref _statusPpText, value);
        }

        private string _statusSegmentText;

        [UIValue("status-segment-text")]
        internal string StatusSegmentText
        {
            get => _statusSegmentText;
            set => ChangeProperty(ref _statusSegmentText, value);
        }

        [UIComponent("status-time-rect")]
        private readonly Backgroundable _statusTimeRect;

        private void FillStatusTimeRect()
        {
            _statusTimeRect.Fill("#000000bb", "#00000066");
        }

        private string _statusTimeText;

        [UIValue("status-time-text")]
        internal string StatusTimeText
        {
            get => _statusTimeText;
            set => ChangeProperty(ref _statusTimeText, value);
        }

        // ----- SIDE CONTROL BAR -----

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

        // ----- TOP/RECENT SCORES -----

        [UIComponent("score-list")]
        private readonly CustomCellListTableData _scoreList;

        [UIValue("score-entries")]
        private readonly List<object> _scoreEntries = new List<object>();

        internal void ReplaceScoreEntries(IEnumerable<ScoreEntry> entries)
        {
            _scoreEntries.Clear();
            _scoreEntries.AddRange(entries);
            _scoreList?.tableView.ReloadData();
        }

        internal class ScoreEntry : BSMLView
        {
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

            private readonly Task<Sprite> _coverSprite;

            internal ScoreEntry(
                string rank,
                Task<Sprite> coverSprite,
                string title,
                string subTitle,
                string difficulty,
                string result,
                string meta)
            {
                Rank = rank;
                Title = title;
                SubTitle = subTitle;
                Difficulty = difficulty;
                Result = result;
                Meta = meta;
                _coverSprite = coverSprite;
            }

            [UIAction("#post-parse")]
            private async void SetCoverAsync()
            {
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

        // ----- FOOTER -----

        [UIComponent("footer-rect")]
        private readonly Backgroundable _footerRect;

        private (string, string) _footerRectGradient = ("#333333", "#666666");

        internal (string, string) FooterRectGradient
        {
            get => _footerRectGradient;
            set => ChangeProperty(ref _footerRectGradient, value, FillFooterRect);
        }

        private void FillFooterRect()
        {
            _footerRect.Fill(_footerRectGradient.Item1, _footerRectGradient.Item2);
        }

        private string _footerText;

        [UIValue("footer-text")]
        internal string FooterText
        {
            get => _footerText;
            set => ChangeProperty(ref _footerText, value);
        }
    }
}

