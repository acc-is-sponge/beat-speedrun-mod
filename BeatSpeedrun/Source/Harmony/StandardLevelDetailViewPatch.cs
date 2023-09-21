using BeatSaberMarkupLanguage;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace BeatSpeedrun.Source.Harmony
{
    [HarmonyPatch(typeof(StandardLevelDetailView), nameof(StandardLevelDetailView.RefreshContent))]
    public class StandardLevelDetailViewPatch
    {
        public static event Action<StandardLevelDetailView> didRefreshContentEvent;

        private static void Postfix(StandardLevelDetailView __instance)
        {
            didRefreshContentEvent?.Invoke(__instance);
        }
    }
}