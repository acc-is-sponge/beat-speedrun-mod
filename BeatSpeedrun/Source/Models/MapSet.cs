using System.Collections.Generic;
using BeatSpeedrun.Extensions;
using Newtonsoft.Json;

namespace BeatSpeedrun.Models
{
    /// <summary>
    /// See <see href="https://github.com/acc-is-sponge/beat-speedrun-mapsets">Format</see>
    /// </summary>
    internal class MapSet
    {
        private readonly Dictionary<string, Dictionary<string, float>> _payload;

        internal string Checksum { get; }

        internal MapSet(Dictionary<string, Dictionary<string, float>> payload, string checksum)
        {
            _payload = payload;
            Checksum = checksum;
        }

        internal float? GetStar(string songHash, DifficultyRaw difficultyRaw)
        {
            if (_payload.TryGetValue(songHash.ToUpper(), out var difficulties) &&
                difficulties.TryGetValue(difficultyRaw.ToString(), out var star))
            {
                return star;
            }
            return null;
        }

        internal static MapSet FromJson(string json)
        {
            var payload = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, float>>>(json);
            // Here, for simplicity, the checksum computed from the original JSON string is used
            var checksum = json.ComputeChecksum();
            return new MapSet(payload, checksum);
        }
    }
}
