using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BeatSpeedrun.Models;
using BeatSpeedrun.Extensions;

namespace BeatSpeedrun.Managers
{
    internal class MapSetManager
    {
        private readonly TaskCache<string, MapSet> _cache;

        internal MapSetManager()
        {
            _cache = new TaskCache<string, MapSet>(LoadAsync);
        }

        internal Task<MapSet> GetAsync(string mapSetPath)
        {
            return _cache.GetAsync(mapSetPath);
        }

        private async Task<MapSet> LoadAsync(string mapSetPath, Task<MapSet> previousLoadFail)
        {
            if (previousLoadFail != null) await Task.Delay(TimeSpan.FromSeconds(3));

            try
            {
                if (mapSetPath.StartsWith("https:") || mapSetPath.StartsWith("http:"))
                {
                    var uri = new Uri(mapSetPath);
                    var json = await HttpClient.GetStringAsync(uri);
                    return MapSet.FromJson(json);
                }

                if (mapSetPath.StartsWith("custom:"))
                {
                    var relativePath = mapSetPath.Substring("custom:".Length).Replace('\\', '/').Split('/');
                    if (relativePath.Any(part => part == ".."))
                    {
                        throw new ArgumentException("Custom MapSet path cannot contain '..'");
                    }
                    var filePath = Path.Combine(CustomDirectory, Path.Combine(relativePath));
                    var json = File.ReadAllText(filePath);
                    return MapSet.FromJson(json);
                }

                {
                    var uri = new Uri(ModProvidedBaseUri + mapSetPath);
                    var json = await HttpClient.GetStringAsync(uri);
                    return MapSet.FromJson(json);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Warn($"Could not load mapset '{mapSetPath}':\n{ex}");
                throw;
            }
        }

        private static readonly string CustomDirectory = Path.Combine(Environment.CurrentDirectory, "UserData", "BeatSpeedrun", "CustomMapSets");
        private static readonly string ModProvidedBaseUri = "https://raw.githubusercontent.com/acc-is-sponge/beat-speedrun-mapsets/main/";
        private static readonly HttpClient HttpClient = new HttpClient();
    }
}
