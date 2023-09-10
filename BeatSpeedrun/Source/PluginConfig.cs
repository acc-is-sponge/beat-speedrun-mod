using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace BeatSpeedrun
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }

        /// <summary>
        /// ID of the current speedrun. It will be null if not running.
        /// </summary>
        public virtual string CurrentSpeedrun { get; set; }

        /// <summary>
        /// List of regulations. Each string must be an URI or a relative path to the regulation
        /// JSON from regulations repository (https://github.com/acc-is-sponge/beat-speedrun-regulations).
        /// For example, `scoresaber/092023.json` denotes the regulation defined in
        /// https://github.com/acc-is-sponge/beat-speedrun-regulations/blob/main/scoresaber/092023.json.
        /// Notice that, since this MOD automatically fetches latest regulations from
        /// https://github.com/acc-is-sponge/beat-speedrun-regulations/blob/main/latest-regulations.json,
        /// we usually don't need to specify this option. Specifying this option disables the automatic fetching.
        /// </summary>
        ///
        [UseConverter(typeof(ListConverter<string>))]
        public virtual List<string> RegulationOptions { get; set; }

        public virtual void OnReload()
        {
            // Do stuff after config is read from disk.
        }

        public virtual void Changed()
        {
            // Do stuff when the config is changed.
        }

        public virtual void CopyFrom(PluginConfig other)
        {
            // This instance's members populated from other
        }
    }
}
