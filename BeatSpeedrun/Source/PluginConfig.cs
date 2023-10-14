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
        /// Tracks the latest selected regulation.
        /// </summary>
        public virtual string LatestSelectedRegulation { get; set; }

        /// <summary>
        /// Tracks the latest selected segment.
        /// </summary>
        public virtual string LatestSelectedSegment { get; set; }

        /// <summary>
        /// List of custom regulation URIs. Each string must be an URI.
        /// </summary>
        [UseConverter(typeof(ListConverter<string>))]
        public virtual List<string> RegulationUris { get; set; }

        /// <summary>
        /// See <see cref="Repositories.SpeedrunRepository.Migrate"/>.
        /// </summary>
        public virtual int SpeedrunFileVersion { get; set; }

        /// <summary>
        /// See <see cref="Services.SpeedrunFacilitator.WritePastSpeedrunsToLocalLeaderboardsAsync"/>.
        /// </summary>
        public virtual int LeaderboardWriteVersion { get; set; }

        /// <summary>
        /// Hidden option!
        /// </summary>
        public virtual bool StopTargetReachedSpeedrunsAutomatically { get; set; }

        /// <summary>
        /// Whether SpeedrunStar is displayed on the playlist.
        /// </summary>
        public virtual bool SpeedrunStarEnabled { get; set; }

        /// <summary>
        /// Whether FloatingTimer is displayed during the game.
        /// </summary>
        public virtual bool FloatingTimerEnabled { get; set; }

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
