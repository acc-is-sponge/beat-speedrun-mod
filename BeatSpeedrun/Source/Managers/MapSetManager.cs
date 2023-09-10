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

        internal Task<MapSet> GetAsync(string uriOrRelativePath, CancellationToken ct = default)
        {
            return _mapSets.GetOrAdd(uriOrRelativePath, LoadAsync).WithCancellation(ct);
        }

        private async Task<MapSet> LoadAsync(string uriOrRelativePath)
        {
            Uri uri;

            try
            {
                uri = new Uri(uriOrRelativePath);
            }
            catch
            {
                uri = null;
            }

            if (uri != null && (uri.Scheme == "https" || uri.Scheme == "http"))
            {
                return await LoadFromWebAsync(uri);
            }

            var relativePath = uriOrRelativePath.Split('/');
            if (relativePath.Any(part => part == ".."))
            {
                throw new ArgumentException("MapSet path cannot contain '..'");
            }

            try
            {
                return LoadFromCache(relativePath);
            }
            catch (Exception ex)
            {
                var isIgnorableError = ex is FileNotFoundException || ex is DirectoryNotFoundException;
                if (!isIgnorableError)
                {
                    Plugin.Log.Warn($"Could not load mapset '{string.Join("/", relativePath)}' from cache:\n{ex}");
                }
            }

            return await LoadFromRepositoryAsync(relativePath);
        }

        private async Task<MapSet> LoadFromWebAsync(Uri uri)
        {
            var json = await HttpClient.GetStringAsync(uri);
            // We don't cache mapsets from the web
            return MapSet.FromJson(json);
        }

        private MapSet LoadFromCache(string[] relativePath)
        {
            // Indeed, you can place your own mapsets in the cache directory
            var cache = Path.Combine(CacheDirectory, Path.Combine(relativePath));
            var json = File.ReadAllText(cache);
            return MapSet.FromJson(json);
        }

        private async Task<MapSet> LoadFromRepositoryAsync(string[] relativePath)
        {
            var uri = new Uri(RepositoryRoot + string.Join("/", relativePath));
            var json = await HttpClient.GetStringAsync(uri);
            var mapSet = MapSet.FromJson(json);
            try
            {
                var cache = Path.Combine(CacheDirectory, Path.Combine(relativePath));
                Directory.CreateDirectory(Path.GetDirectoryName(cache));
                File.WriteAllText(cache, json);
            }
            catch (Exception ex)
            {
                Plugin.Log.Warn($"Failed to cache mapset '{string.Join("/", relativePath)}':\n{ex}");
            }
            return mapSet;
        }

        private static readonly string CacheDirectory = Path.Combine(Environment.CurrentDirectory, "UserData", "BeatSpeedrun", "MapSets");
        private static readonly string RepositoryRoot = "https://raw.githubusercontent.com/acc-is-sponge/beat-speedrun-mapsets/main/";
        private static readonly HttpClient HttpClient = new HttpClient();
    }
}
