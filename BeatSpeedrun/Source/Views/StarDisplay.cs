using BeatSaberMarkupLanguage;
using BeatSpeedrun.Controllers;
using BeatSpeedrun.Models;
using BeatSpeedrun.Models.Speedrun;
using BeatSpeedrun.Services;
using BeatSpeedrun.Source.Harmony;
using BeatSpeedrun.Views;
using HMUI;
using IPA.Utilities;
using ModestTree;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

namespace BeatSpeedrun.Source.Views
{
    internal class StarDisplay : IInitializable, IDisposable
    {
        private GameObject extraUI = null;
        private float? star = null;
        private readonly SpeedrunFacilitator _speedrunFacilitator;

        public StarDisplay(SpeedrunFacilitator speedrunFacilitator)
        {
            _speedrunFacilitator = speedrunFacilitator;
        }

        private void UpdateStarDisplay(StandardLevelDetailView standardLevelDetailView)
        {            
            star = GetStar(standardLevelDetailView);

            if (star == null)
            {
                GameObject.Destroy(extraUI);
                return;
            }

            if (extraUI == null)
            {
                // the localPostion of the LevelParamsPanel can change
                LevelParamsPanel levelParamsPanel = FieldAccessor<StandardLevelDetailView, LevelParamsPanel>.Get(standardLevelDetailView, "_levelParamsPanel");
                // the localPostion of the LevelBar would not change
                LevelBar levelBar = FieldAccessor<StandardLevelDetailView, LevelBar>.Get(standardLevelDetailView, "_levelBar");
                TextMeshProUGUI[] temp = levelParamsPanel.GetComponentsInChildren<TextMeshProUGUI>();
                ImageView[] temp2 = levelBar.GetComponentsInChildren<ImageView>();
                extraUI = GameObject.Instantiate(temp[0].transform.parent.gameObject, temp2[0].transform);
                extraUI.transform.localPosition -= new Vector3(-8, 12f);
            }

            ModifyField();
            ModifyHoverHint();
        }

        private void ModifyField()
        {
            CurvedTextMeshPro field = extraUI.GetComponentInChildren<CurvedTextMeshPro>();
            ImageView imageView = field.transform.parent.Find("Icon").GetComponent<ImageView>();
            GameObject.Destroy(imageView);
            field.text = $"★{star.Value:F2}";
            field.color = Color.white;
        }

        private void ModifyHoverHint()
        {
            HoverHint hoverHint = extraUI.GetComponentInChildren<HoverHint>();
            GameObject.Destroy(extraUI.GetComponentInChildren<LocalizedHoverHint>());
            HoverHintController hoverHintController = Resources.FindObjectsOfTypeAll<HoverHintController>().First();
            hoverHint.SetField("_hoverHintController", hoverHintController);
            hoverHint.text = "Speedrun Star!";
        }

        private float? GetStar(StandardLevelDetailView standardLevelDetailView)
        {
            Speedrun speedrun = _speedrunFacilitator.Current;
            IBeatmapLevel beatmapLevel = standardLevelDetailView.selectedDifficultyBeatmap.level;
            BeatmapDifficulty difficulty = standardLevelDetailView.selectedDifficultyBeatmap.difficulty;
            IDifficultyBeatmapSet difficultyBeatmapSet = standardLevelDetailView.selectedDifficultyBeatmap.parentDifficultyBeatmapSet;
            string[] levelInfo = beatmapLevel.levelID.Split('_');
            // For official maps
            if(levelInfo.Length < 3)
            {
                return null;
            }
            string characteristic = difficultyBeatmapSet.beatmapCharacteristic.serializedName;
            float? star = speedrun?.MapSet?.GetStar(levelInfo[2], new DifficultyRaw(difficulty, "Solo" + characteristic));
            return star;
        }

        private void ShowStarWithoutArguments()
        {
            StandardLevelDetailView standardLevelDetailView = Resources.FindObjectsOfTypeAll<StandardLevelDetailView>().First();
            UpdateStarDisplay(standardLevelDetailView);
        }

        public void Initialize()
        {
            StandardLevelDetailViewPatch.didRefreshContentEvent += UpdateStarDisplay;
            _speedrunFacilitator.OnCurrentSpeedrunChanged += ShowStarWithoutArguments;
        }

        public void Dispose()
        {
            StandardLevelDetailViewPatch.didRefreshContentEvent -= UpdateStarDisplay;
            _speedrunFacilitator.OnCurrentSpeedrunChanged -= ShowStarWithoutArguments;
        }
    }
}