
using System;
using System.Linq;
using BeatSpeedrun.Harmony;
using BeatSpeedrun.Models;
using BeatSpeedrun.Models.Speedrun;
using BeatSpeedrun.Services;
using BeatSpeedrun.Views;
using UnityEngine;
using Zenject;

namespace BeatSpeedrun.Controllers
{
    internal class SpeedrunStarViewController : IInitializable, IDisposable
    {
        private readonly SpeedrunFacilitator _speedrunFacilitator;
        private readonly SpeedrunStarView _view = new SpeedrunStarView();

        public SpeedrunStarViewController(SpeedrunFacilitator speedrunFacilitator)
        {
            _speedrunFacilitator = speedrunFacilitator;
        }

        private float? GetStar(IDifficultyBeatmap difficultyBeatmap)
        {
            if (difficultyBeatmap == null) return null;
            var speedrun = _speedrunFacilitator.Current;
            if (speedrun == null) return null;
            if (!(difficultyBeatmap.level is CustomBeatmapLevel)) return null;
            var difficulty = difficultyBeatmap.difficulty;
            var characteristic = $"Solo{difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName}";
            var difficultyRaw = new DifficultyRaw(difficulty, characteristic);
            var levelInfo = difficultyBeatmap.level.levelID.Split('_');
            return speedrun.MapSet.GetStar(levelInfo[2], difficultyRaw);
        }

        private void Update(StandardLevelDetailView standardLevelDetailView = null)
        {
            if (standardLevelDetailView == null)
            {
                standardLevelDetailView = Resources.FindObjectsOfTypeAll<StandardLevelDetailView>().FirstOrDefault();
                if (standardLevelDetailView == null) return;
            }

            var star = GetStar(standardLevelDetailView.selectedDifficultyBeatmap);
            if (star.HasValue)
            {
                _view.Appear($"★{star.Value:F2}", "BeatSpeedrun Star!");
            }
            else
            {
                _view.Disappear();
            }
        }

        #region callbacks

        void IInitializable.Initialize()
        {
            StandardLevelDetailViewPatch.OnRefreshContent += OnRefreshContent;
            _speedrunFacilitator.OnCurrentSpeedrunChanged += OnCurrentSpeedrunChanged;
        }

        void IDisposable.Dispose()
        {
            _speedrunFacilitator.OnCurrentSpeedrunChanged -= OnCurrentSpeedrunChanged;
            StandardLevelDetailViewPatch.OnRefreshContent -= OnRefreshContent;
        }

        private void OnRefreshContent(StandardLevelDetailView standardLevelDetailView) => Update(standardLevelDetailView);
        private void OnCurrentSpeedrunChanged((Speedrun, Speedrun) _) => Update();

        #endregion
    }
}
