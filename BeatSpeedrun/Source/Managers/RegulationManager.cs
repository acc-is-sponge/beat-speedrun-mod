using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using BeatSpeedrun.Models;
using BeatSpeedrun.Extensions;
using System.Linq;

namespace BeatSpeedrun.Managers
{
    internal class RegulationManager
    {
        private readonly Task<List<string>> _regulationOptions;
        private readonly ConcurrentDictionary<string, Task<Regulation>> _regulations =
            new ConcurrentDictionary<string, Task<Regulation>>();

        internal RegulationManager()
        {
            _regulationOptions = FetchOptionsAsync();
        }

        internal Task<List<string>> GetOptionsAsync(CancellationToken ct = default)
        {
            return _regulationOptions.WithCancellation(ct);
        }

        private async Task<List<string>> FetchOptionsAsync()
        {
            if (PluginConfig.Instance.RegulationOptions != null)
            {
                return PluginConfig.Instance.RegulationOptions.ToList();
            }
            var uri = new Uri(RepositoryRoot + "latest-regulations.json");
            var json = await HttpClient.GetStringAsync(uri);
            var latestRegulations = LatestRegulations.FromJson(json);
            return latestRegulations.Regulations.ToList();
        }

        internal Task<Regulation> GetAsync(string uriOrRelativePath, CancellationToken ct = default)
        {
            return _regulations.GetOrAdd(uriOrRelativePath, LoadAsync).WithCancellation(ct);
        }

        private async Task<Regulation> LoadAsync(string uriOrRelativePath)
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
                throw new ArgumentException("Regulation path cannot contain '..'");
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
                    Plugin.Log.Warn($"Could not load regulation '{string.Join("/", relativePath)}' from cache:\n{ex}");
                }
            }

            return await LoadFromRepositoryAsync(relativePath);
        }

        private async Task<Regulation> LoadFromWebAsync(Uri uri)
        {
            var json = await HttpClient.GetStringAsync(uri);
            // We don't cache regulations from the web
            return Regulation.FromJson(json);
        }

        private Regulation LoadFromCache(string[] relativePath)
        {
            // Indeed you can place your own regulations in the cache directory
            var cache = Path.Combine(CacheDirectory, Path.Combine(relativePath));
            var json = File.ReadAllText(cache);
            return Regulation.FromJson(json);
        }

        private async Task<Regulation> LoadFromRepositoryAsync(string[] relativePath)
        {
            var uri = new Uri(RepositoryRoot + string.Join("/", relativePath));
            var json = await HttpClient.GetStringAsync(uri);
            var regulation = Regulation.FromJson(json);
            try
            {
                var cache = Path.Combine(CacheDirectory, Path.Combine(relativePath));
                Directory.CreateDirectory(Path.GetDirectoryName(cache));
                File.WriteAllText(cache, json);
            }
            catch (Exception ex)
            {
                Plugin.Log.Warn($"Failed to cache regulation '{string.Join("/", relativePath)}':\n{ex}");
            }
            return regulation;
        }

        private static readonly string CacheDirectory = Path.Combine(Environment.CurrentDirectory, "UserData", "BeatSpeedrun", "Regulations");
        private static readonly string RepositoryRoot = "https://raw.githubusercontent.com/acc-is-sponge/beat-speedrun-regulations/main/";
        private static readonly HttpClient HttpClient = new HttpClient();
    }
}
