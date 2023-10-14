using System.Linq;
using HMUI;
using IPA.Utilities;
using TMPro;
using UnityEngine;

namespace BeatSpeedrun.Views
{
    internal class SpeedrunStarView
    {
        private GameObject extraUI = null;

        internal void Appear(string text, string hoverHint)
        {
            Disappear();

            var standardLevelDetailView = Resources.FindObjectsOfTypeAll<StandardLevelDetailView>().First();
            // the localPostion of the LevelParamsPanel can change
            var levelParamsPanel = FieldAccessor<StandardLevelDetailView, LevelParamsPanel>.Get(standardLevelDetailView, "_levelParamsPanel");
            // the localPostion of the LevelBar would not change
            var levelBar = FieldAccessor<StandardLevelDetailView, LevelBar>.Get(standardLevelDetailView, "_levelBar");
            var source = levelParamsPanel.GetComponentsInChildren<TextMeshProUGUI>();
            var dest = levelBar.GetComponentsInChildren<ImageView>();
            extraUI = GameObject.Instantiate(source[0].transform.parent.gameObject, dest[0].transform);
            extraUI.transform.localPosition -= new Vector3(-8, 12f);

            GameObject.Destroy(extraUI.transform.Find("Icon").GetComponent<ImageView>());
            GameObject.Destroy(extraUI.GetComponentInChildren<LocalizedHoverHint>());

            var textField = extraUI.GetComponentInChildren<CurvedTextMeshPro>();
            textField.text = text;

            var hoverHintField = extraUI.GetComponentInChildren<HoverHint>();
            var hoverHintController = Resources.FindObjectsOfTypeAll<HoverHintController>().First();
            hoverHintField.SetField("_hoverHintController", hoverHintController);
            hoverHintField.text = hoverHint;
        }

        internal void Disappear()
        {
            if (extraUI != null)
            {
                GameObject.Destroy(extraUI);
            }
        }
    }
}
