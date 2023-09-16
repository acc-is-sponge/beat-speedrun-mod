using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BeatSpeedrun.Models;
using BeatSpeedrun.Extensions;

namespace BeatSpeedrun.Managers
{
    internal class MapSetManager
    {
        private readonly ConcurrentDictionary<string, Task<MapSet>> _mapSets =
            new ConcurrentDictionary<string, Task<MapSet>>();

        internal Task<MapSet> GetAsync(string mapSetPath, CancellationToken ct = default)
        {
            return _mapSets
                .AddOrUpdate(
                    mapSetPath,
                    path => LoadAsync(path),
                    (path, t) => t.IsFaulted ? LoadAsync(path, TimeSpan.FromSeconds(3)) : t)
                .WithCancellation(ct);
        }

        private async Task<MapSet> LoadAsync(string mapSetPath, TimeSpan delay = default)
        {
            await Task.Delay(delay);

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
            catch (OperationCanceledException)
            {
                throw;
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
