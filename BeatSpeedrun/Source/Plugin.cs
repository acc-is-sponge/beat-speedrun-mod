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
            Log.Info("BeatSpeedrun initialized.");
        }
    }
}
