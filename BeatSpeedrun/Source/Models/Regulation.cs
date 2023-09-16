using System;
using System.Linq;
using BeatSpeedrun.Extensions;
using Newtonsoft.Json;

namespace BeatSpeedrun.Models
{
    /// <summary>
    /// See <see href="https://github.com/acc-is-sponge/beat-speedrun-regulations">Format</see>
    /// </summary>
    internal class Regulation
    {
        // TODO: make this immutable

        [JsonProperty("version")]
        internal int Version { get; set; }

        [JsonProperty("title")]
        internal string Title { get; set; }

        [JsonProperty("description")]
        internal string Description { get; set; }

        [JsonProperty("rules")]
        internal Rules Rules { get; set; }

        internal string ComputeChecksum()
        {
            // Since attributes other than rules are allowed to be changed,
            // we can use `Rules.ComputeChecksum()` (see format for details).
            return Rules.ComputeChecksum();
        }

        internal static Regulation FromJson(string json)
        {
            var regulation = JsonConvert.DeserializeObject<Regulation>(json);
            if (regulation.Version != 1)
            {
                throw new ArgumentException($"Unsupported regulation version: {regulation.Version}");
            }
            // TODO: more validations
            return regulation;
        }

        internal static bool IsCustomPath(string path)
        {
            return path.StartsWith("http:") || path.StartsWith("https:") || path.StartsWith("custom:");
        }

        internal static string ShortenPath(string path)
        {
            if (path.StartsWith("http:") || path.StartsWith("https:"))
            {
                try
                {
                    var uri = new Uri(path);
                    path = uri.LocalPath;
                }
                catch (Exception ex)
                {
                    Plugin.Log.Warn($"Cannot parse regulation path '{path}' as URI:\n{ex}");
                    return path;
                }
                return path.EndsWith(".json") ? path.Substring(0, path.Length - 5) : path;
            }

            if (path.StartsWith("custom:"))
            {
                path = "(custom) " + path.Substring("custom:".Length);
                return path.EndsWith(".json") ? path.Substring(0, path.Length - 5) : path;
            }

            // omit the last part
            var pathParts = path.Split('/');
            return 1 < pathParts.Length
                ? string.Join("/", pathParts.Take(pathParts.Length - 1))
                : path;
        }
    }

    internal class Rules
    {
        [JsonProperty("mapSet")]
        internal string MapSet { get; set; }

        [JsonProperty("base")]
        internal float Base { get; set; }

        [JsonProperty("curve")]
        internal float[][] Curve { get; set; }

        [JsonProperty("weight")]
        internal float Weight { get; set; }

        [JsonProperty("timeLimit")]
        internal int TimeLimit { get; set; }

        [JsonProperty("segmentRequirements")]
        internal SegmentRequirements SegmentRequirements { get; set; } = new SegmentRequirements();

        [JsonProperty("modifiersOverride")]
        internal ModifiersOverride ModifiersOverride { get; set; } = new ModifiersOverride();

        internal string ComputeChecksum()
        {
            return JsonConvert.SerializeObject(this).ComputeChecksum();
        }
    }

    internal class SegmentRequirements
    {
        public int Bronze { get; set; } = 1000;
        public int Silver { get; set; } = 2000;
        public int Gold { get; set; } = 3000;
        public int Platinum { get; set; } = 4000;
        public int Emerald { get; set; } = 5000;
        public int Sapphire { get; set; } = 6000;
        public int Ruby { get; set; } = 7000;
        public int Diamond { get; set; } = 8000;
        public int Master { get; set; } = 9000;
        public int Grandmaster { get; set; } = 10000;

        internal int GetValue(Segment segment)
        {
            switch (segment)
            {
                case Segment.Bronze:
                    return Bronze;
                case Segment.Silver:
                    return Silver;
                case Segment.Gold:
                    return Gold;
                case Segment.Platinum:
                    return Platinum;
                case Segment.Emerald:
                    return Emerald;
                case Segment.Sapphire:
                    return Sapphire;
                case Segment.Ruby:
                    return Ruby;
                case Segment.Diamond:
                    return Diamond;
                case Segment.Master:
                    return Master;
                case Segment.Grandmaster:
                    return Grandmaster;
                default:
                    throw new ArgumentOutOfRangeException(nameof(segment), segment, null);
            }
        }
    }

    internal class ModifiersOverride
    {
        public float UsePause { get; set; } = 1f;
        public float BatteryEnergy { get; set; } = 1f;
        public float NoFail { get; set; } = 0.5f;
        public float InstaFail { get; set; } = 1f;
        public float NoObstacles { get; set; } = 0.95f;
        public float NoBombs { get; set; } = 0.9f;
        public float StrictAngles { get; set; } = 1f;
        public float DisappearingArrows { get; set; } = 1.07f;
        public float FasterSong { get; set; } = 1.08f;
        public float SlowerSong { get; set; } = 0.7f;
        public float NoArrows { get; set; } = 0.7f;
        public float GhostNotes { get; set; } = 1.11f;
        public float SuperFastSong { get; set; } = 1.1f;
        public float ProMode { get; set; } = 1f;
        public float SmallCubes { get; set; } = 1f;
    }
}
