using System;
using System.Reflection;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPALogger = IPA.Logging.Logger;
using SiraUtil.Zenject;
using BeatSpeedrun.Installers;

namespace BeatSpeedrun
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        [Init]
        public void Init(IPALogger logger, Config conf, Zenjector zenjector)
        {
            Instance = this;
            Log = logger;
            PluginConfig.Instance = conf.Generated<PluginConfig>();
            zenjector.Install<AppInstaller>(Location.App);
            zenjector.Install<MenuInstaller>(Location.Menu);
            Log.Info("BeatSpeedrun initialized.");
        }

        /// <summary>
        /// Called when the plugin is enabled (including when the game starts if the plugin is enabled).
        /// </summary>
        [OnEnable]
        public void OnEnable()
        {
            ApplyHarmonyPatches();
        }

        /// <summary>
        /// Called when the plugin is disabled and on Beat Saber quit. It is important to clean up any Harmony patches, GameObjects, and Monobehaviours here.
        /// The game should be left in a state as if the plugin was never started.
        /// Methods marked [OnDisable] must return void or Task.
        /// </summary>
        [OnDisable]
        public void OnDisable()
        {
            RemoveHarmonyPatches();
        }

        #region Harmony

        /// <summary>
        /// Attempts to apply all the Harmony patches in this assembly.
        /// </summary>
        private void ApplyHarmonyPatches()
        {
            try
            {
                Harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Log?.Error("Error applying Harmony patches: " + ex.Message);
                Log?.Debug(ex);
            }
        }

        /// <summary>
        /// Attempts to remove all the Harmony patches that used our HarmonyId.
        /// </summary>
        private void RemoveHarmonyPatches()
        {
            try
            {
                Harmony.UnpatchSelf();
            }
            catch (Exception ex)
            {
                Log?.Error("Error removing Harmony patches: " + ex.Message);
                Log?.Debug(ex);
            }
        }

        private static readonly HarmonyLib.Harmony Harmony =
            new HarmonyLib.Harmony("com.github.acc-is-sponge.beat-speedrun-mod");

        #endregion
    }
}
