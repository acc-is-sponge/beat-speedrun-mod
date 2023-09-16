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
        private Task<List<string>> _regulationOptions;
        private readonly ConcurrentDictionary<string, Task<Regulation>> _regulations =
            new ConcurrentDictionary<string, Task<Regulation>>();

        internal RegulationManager()
        {
            _regulationOptions = FetchOptionsAsync();
        }

        internal Task<List<string>> GetOptionsAsync(CancellationToken ct = default)
        {
            if (_regulationOptions.IsFaulted)
            {
                _regulationOptions = FetchOptionsAsync();
            }
            return _regulationOptions.WithCancellation(ct);
        }

        private async Task<List<string>> FetchOptionsAsync()
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

        internal Task<Regulation> GetAsync(string regulationPath, CancellationToken ct = default)
        {
            return _regulations
                .AddOrUpdate(
                    regulationPath,
                    path => LoadAsync(path),
                    (path, t) => t.IsFaulted ? LoadAsync(regulationPath, TimeSpan.FromSeconds(3)) : t)
                .WithCancellation(ct);
        }

        private async Task<Regulation> LoadAsync(string regulationPath, TimeSpan delay = default)
        {
            await Task.Delay(delay);

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
