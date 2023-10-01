using HarmonyLib;
using System;

/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace BeatSpeedrun.Harmony
{
    [HarmonyPatch(typeof(StandardLevelDetailView), nameof(StandardLevelDetailView.RefreshContent))]
    public class StandardLevelDetailViewPatch
    {
        public static event Action<StandardLevelDetailView> OnRefreshContent;

        private static void Postfix(StandardLevelDetailView __instance)
        {
            try
            {
                OnRefreshContent?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Error while invoking OnRefreshContent:\n{ex}");
            }
        }
    }
}
