using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BeatSpeedrun.Models
{
    /// <summary>
    /// See <see href="https://github.com/acc-is-sponge/beat-speedrun-regulations/blob/main/latest-regulations.json">latest-regulations.json</see>
    /// </summary>
    internal class LatestRegulations
    {
        // TODO: make this immutable

        [JsonProperty("regulations")]
        internal string[] Regulations { get; set; }

        internal static LatestRegulations FromJson(string json)
        {
            return JsonConvert.DeserializeObject<LatestRegulations>(json);
        }
    }
}
