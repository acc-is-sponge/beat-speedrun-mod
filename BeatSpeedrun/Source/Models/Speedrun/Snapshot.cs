using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BeatSpeedrun.Models.Speedrun
{
    /// <summary>
    /// A serializable Speedrun snapshot.
    /// This allows players to continue the speedrun even if the application is restarted.
    /// </summary>
    internal class Snapshot
    {
        // TODO: make this immutable

        [JsonProperty("id")]
        internal string Id { get; set; }

        [JsonProperty("startedAt")]
        internal DateTime StartedAt { get; set; }

        [JsonProperty("finishedAt")]
        internal DateTime? FinishedAt { get; set; }

        [JsonProperty("regulation")]
        internal string Regulation { get; set; }

        [JsonProperty("targetSegment"), JsonConverter(typeof(StringEnumConverter))]
        internal Segment? TargetSegment { get; set; }

        [JsonProperty("checksum")]
        internal SnapshotChecksum Checksum { get; set; }

        [JsonProperty("songScores")]
        internal List<SnapshotSongScore> SongScores { get; set; }

        internal static Snapshot FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Snapshot>(json);
        }

        internal string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.None, JsonSerializerSettings);
        }

        internal void Validate(Regulation regulation, MapSet mapSet)
        {
            if (Checksum.Regulation != regulation.Checksum)
            {
                throw new ArgumentException("Regulation checksum mismatch");
            }
            if (Checksum.MapSet != mapSet.Checksum)
            {
                throw new ArgumentException("MapSet checksum mismatch");
            }
        }

        internal static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };
    }

    /// <summary>
    /// Used to verify that the reference {regulation,mapset} is not modified
    /// </summary>
    internal class SnapshotChecksum
    {
        [JsonProperty("regulation")]
        internal string Regulation { get; set; }

        [JsonProperty("mapSet")]
        internal string MapSet { get; set; }
    }

    /// <summary>
    /// Every song score (including non-high score) are recorded as this data
    /// </summary>
    internal class SnapshotSongScore
    {
        [JsonProperty("completedAt")]
        internal DateTime CompletedAt { get; set; }

        [JsonProperty("songHash")]
        internal string SongHash { get; set; }

        [JsonProperty("difficultyRaw")]
        internal string DifficultyRaw { get; set; }

        [JsonProperty("baseAccuracy")]
        internal float BaseAccuracy { get; set; }

        [JsonProperty("badCutCount")]
        internal int BadCutCount { get; set; }

        [JsonProperty("missNoteCount")]
        internal int MissNoteCount { get; set; }

        [JsonProperty("fullCombo")]
        internal bool FullCombo { get; set; }

        [JsonProperty("modifiers")]
        internal SnapshotModifiers Modifiers { get; set; }

        [JsonIgnore]
        internal int MissOrBadCutNoteCount => BadCutCount + MissNoteCount;

        // NOTE:
        // We can embed more useful information about the score. For example,
        // we can store replays in UserData/BeatSpeedrun/Replays and keep the address
        // of the replay here (like git objects). How hard is it to implement?
    }

    internal class SnapshotModifiers
    {
        public bool UsePause { get; set; }
        public bool BatteryEnergy { get; set; }
        public bool NoFail { get; set; }
        public bool InstaFail { get; set; }
        public bool NoObstacles { get; set; }
        public bool NoBombs { get; set; }
        public bool StrictAngles { get; set; }
        public bool DisappearingArrows { get; set; }
        public bool FasterSong { get; set; }
        public bool SlowerSong { get; set; }
        public bool NoArrows { get; set; }
        public bool GhostNotes { get; set; }
        public bool SuperFastSong { get; set; }
        public bool ProMode { get; set; }
        public bool SmallCubes { get; set; }

        public override string ToString()
        {
            var mods = new List<string>();

            if (UsePause) mods.Add("*P");
            if (BatteryEnergy) mods.Add("BE");
            if (NoFail) mods.Add("NF");
            if (InstaFail) mods.Add("IF");
            if (NoObstacles) mods.Add("NO");
            if (NoBombs) mods.Add("NB");
            if (StrictAngles) mods.Add("SA");
            if (DisappearingArrows) mods.Add("DA");
            if (FasterSong) mods.Add("FS");
            if (SlowerSong) mods.Add("SS");
            if (NoArrows) mods.Add("NA");
            if (GhostNotes) mods.Add("GN");
            if (SuperFastSong) mods.Add("SF");
            if (ProMode) mods.Add("PM");
            if (SmallCubes) mods.Add("SC");

            return string.Join(",", mods);
        }
    }
}
