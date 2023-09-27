using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using BeatSpeedrun.Models;
using BeatSpeedrun.Extensions;
using System.Linq;

namespace BeatSpeedrun.Providers
{
    internal class RegulationProvider
    {
        private Task<List<string>> _regulationOptions;
        private readonly TaskCache<string, Regulation> _cache;

        internal RegulationProvider()
        {
            _regulationOptions = LoadOptionsAsync();
            _cache = new TaskCache<string, Regulation>(LoadAsync);
        }

        internal Task<List<string>> GetOptionsAsync()
        {
            if (_regulationOptions.IsFaulted)
            {
                Plugin.Log.Warn($"Failed to get regualtion options:\n{_regulationOptions.Exception}");
                _regulationOptions = LoadOptionsAsync();
            }
            return _regulationOptions;
        }

        internal Task<Regulation> GetAsync(string regulationPath)
        {
            if (string.IsNullOrEmpty(regulationPath)) return Task.FromResult<Regulation>(null);
            return _cache.GetAsync(regulationPath);
        }

        private async Task<List<string>> LoadOptionsAsync()
        {
            var uri = new Uri(ModProvidedBaseUri + "latest-regulations.json");
            var json = await HttpClient.GetStringAsync(uri);
            var latestRegulations = LatestRegulations.FromJson(json);

            var localCustomRegulations = Directory.Exists(CustomDirectory)
                ? ListCustomRegulations(CustomDirectory)
                : Enumerable.Empty<string>();

            var uriCustomRegulations =
                PluginConfig.Instance.RegulationUris?.Where(r => r.StartsWith("https:") || r.StartsWith("http:"))
                ?? Enumerable.Empty<string>();

            return latestRegulations.Regulations
                .Concat(localCustomRegulations)
                .Concat(uriCustomRegulations)
                .ToList();
        }

        private async Task<Regulation> LoadAsync(string regulationPath, Task<Regulation> previousTaskFail)
        {
            if (previousTaskFail != null) await Task.Delay(TimeSpan.FromSeconds(3));

            try
            {
                if (regulationPath.StartsWith("https:") || regulationPath.StartsWith("http:"))
                {
                    var uri = new Uri(regulationPath);
                    var json = await HttpClient.GetStringAsync(uri);
                    return Regulation.FromJson(json);
                }

                if (regulationPath.StartsWith("custom:"))
                {
                    var relativePath = regulationPath.Substring("custom:".Length).Replace('\\', '/').Split('/');
                    if (relativePath.Any(part => part == ".."))
                    {
                        throw new ArgumentException("Custom Regulation path cannot contain '..'");
                    }
                    var filePath = Path.Combine(CustomDirectory, Path.Combine(relativePath));
                    var json = File.ReadAllText(filePath);
                    return Regulation.FromJson(json);
                }

                {
                    var uri = new Uri(ModProvidedBaseUri + regulationPath);
                    var json = await HttpClient.GetStringAsync(uri);
                    return Regulation.FromJson(json);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Plugin.Log.Warn($"Could not load regulation '{regulationPath}':\n{ex}");
                throw;
            }
        }

        private static IEnumerable<string> ListCustomRegulations(string directory)
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                yield return "custom:" + file.Substring(CustomDirectory.Length + 1).Replace('\\', '/');
            }

            foreach (var dir in Directory.GetDirectories(directory))
            {
                foreach (var file in ListCustomRegulations(dir)) yield return file;
            }
        }

        private static readonly string CustomDirectory = Path.Combine(Environment.CurrentDirectory, "UserData", "BeatSpeedrun", "CustomRegulations");
        private static readonly string ModProvidedBaseUri = "https://raw.githubusercontent.com/acc-is-sponge/beat-speedrun-regulations/main/";
        private static readonly HttpClient HttpClient = new HttpClient();
    }
}
