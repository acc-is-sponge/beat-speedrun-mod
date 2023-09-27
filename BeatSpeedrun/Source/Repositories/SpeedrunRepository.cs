using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using BeatSpeedrun.Extensions;
using BeatSpeedrun.Providers;
using BeatSpeedrun.Models;
using BeatSpeedrun.Models.Speedrun;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zenject;

namespace BeatSpeedrun.Repositories
{
    internal class SpeedrunRepository : IInitializable
    {
        private readonly RegulationProvider _regulationProvider;
        private readonly MapSetProvider _mapSetProvider;

        internal SpeedrunRepository(RegulationProvider regulationProvider, MapSetProvider mapSetProvider)
        {
            _regulationProvider = regulationProvider;
            _mapSetProvider = mapSetProvider;
        }

        void IInitializable.Initialize()
        {
            Migrate();
        }

        internal async Task<Speedrun> CreateAsync(
            string regulationPath,
            Segment? targetSegment,
            CancellationToken ct = default)
        {
            var regulation = await _regulationProvider.GetAsync(regulationPath).WithCancellation(ct);
            var mapSet = await _mapSetProvider.GetAsync(regulation.Rules.MapSet).WithCancellation(ct);

            var startedAt = DateTime.UtcNow;
            var snapshot = new Snapshot
            {
                Id = ((long)(startedAt - UnixEpoch).TotalMilliseconds).ToString(),
                StartedAt = startedAt,
                Regulation = regulationPath,
                TargetSegment = targetSegment,
                Checksum = new SnapshotChecksum
                {
                    Regulation = regulation.Checksum,
                    MapSet = mapSet.Checksum,
                },
                SongScores = new List<SnapshotSongScore>(),
            };

            var speedrun = new Speedrun(snapshot, regulation, mapSet);
            Save(speedrun);
            return speedrun;
        }

        internal void Save(Speedrun speedrun)
        {
            var snapshot = speedrun.ToSnapshot();
            var path = Path.Combine(SpeedrunsDirectory, snapshot.Id);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, SomeEncrypt(snapshot.ToJson()));
        }

        internal void Delete(string speedrunId)
        {
            var path = Path.Combine(SpeedrunsDirectory, speedrunId);
            File.Delete(path);
        }

        internal async Task<Speedrun> LoadAsync(string id, CancellationToken ct = default)
        {
            var path = Path.Combine(SpeedrunsDirectory, id);
            var json = SomeDecrypt(File.ReadAllBytes(path));
            var snapshot = Snapshot.FromJson(json);
            var regulation = await _regulationProvider.GetAsync(snapshot.Regulation).WithCancellation(ct);
            var mapSet = await _mapSetProvider.GetAsync(regulation.Rules.MapSet).WithCancellation(ct);
            return new Speedrun(snapshot, regulation, mapSet);
        }

        internal IEnumerable<string> List()
        {
            if (!Directory.Exists(SpeedrunsDirectory)) return Enumerable.Empty<string>();
            return Directory.GetFiles(SpeedrunsDirectory).Select(Path.GetFileName);
        }

        private static byte[] SomeEncrypt(string src)
        {
            var key = new byte[32];
            var iv = new byte[16];

            var r = new Random();
            r.NextBytes(key);
            r.NextBytes(iv);

            using (var m = new MemoryStream())
            {
                m.Write(key, 0, key.Length);
                m.Write(iv, 0, iv.Length);

                using (var aes = Aes.Create())
                using (var encryptor = aes.CreateEncryptor(key, iv))
                using (var c = new CryptoStream(m, encryptor, CryptoStreamMode.Write))
                {
                    using (var w = new StreamWriter(c)) w.Write(src);
                    return m.ToArray();
                }
            }
        }

        private static string SomeDecrypt(byte[] src)
        {
            var key = new byte[32];
            var iv = new byte[16];

            Array.Copy(src, 0, key, 0, 32);
            Array.Copy(src, 32, iv, 0, 16);

            using (var m = new MemoryStream(src, 48, src.Length - 48))
            using (var aes = Aes.Create())
            using (var decryptor = aes.CreateDecryptor(key, iv))
            using (var c = new CryptoStream(m, decryptor, CryptoStreamMode.Read))
            using (var r = new StreamReader(c))
            {
                return r.ReadToEnd();
            }
        }

        private static readonly string SpeedrunsDirectory = Path.Combine(Environment.CurrentDirectory, "UserData", "BeatSpeedrun", "Speedruns");
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        #region migrations

        /// <summary>
        /// Migrate allows the plugin to load previous versions of speedruns.
        /// Whether or not they have been migrated is recorded in PluginConfig.Instance.SpeedrunFileVersion.
        /// </summary>
        private void Migrate()
        {
            var version = PluginConfig.Instance.SpeedrunFileVersion;

            if (version < 1) // _ -> 1
            {
                MigrateToV1();
                version = 1;
            }

            if (version == PluginConfig.Instance.SpeedrunFileVersion) return;
            PluginConfig.Instance.SpeedrunFileVersion = version;
            PluginConfig.Instance.Changed();
        }

        private void MigrateToV1()
        {
            if (!Directory.Exists(SpeedrunsDirectory)) return;

            var modProvidedRegulationChecksumChanges = new Dictionary<string, string>()
            {
                {
                    "72f386326e140a092fa5094528873c84cac8fa0abd953328b4eda259a58d6a3e",
                    "9db301b6408ef5e6e0b2bbe994b98462498dbca4c5b95705fa8efb9d2cb3e9eb"
                },
                {
                    "081a5125658050d85cbe90196c2afd4c47467cd8f144da37929725db62949aad",
                    "78530580bfd33b4054e3857cb373dbaa30280b77f7661587736d1cc488ae135d"
                },
            };

            foreach (var path in Directory.GetFiles(SpeedrunsDirectory))
            {
                try
                {
                    var text = SomeDecrypt(File.ReadAllBytes(path));
                    var json = JObject.Parse(text);
                    var isChanged = false;

                    // speedruns before v1 are not finished explicitly
                    if (json.Value<string>("id") != PluginConfig.Instance.CurrentSpeedrun &&
                        string.IsNullOrEmpty(json.Value<string>("finishedAt")))
                    {
                        json["finishedAt"] = DateTime.UtcNow;
                        isChanged = true;
                    }

                    // speedruns before v1 have incompatible checksum, we try to fix it about MOD-provided regulations
                    var checksum = json["checksum"];
                    if (modProvidedRegulationChecksumChanges.TryGetValue(checksum.Value<string>("regulation"), out var c))
                    {
                        checksum["regulation"] = c;
                        isChanged = true;
                    }

                    if (isChanged)
                    {
                        text = json.ToString(Formatting.None);
                        File.WriteAllBytes(path, SomeEncrypt(text));
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.Warn($"Error while speedrun file migrating (v1):\n{ex}");
                }
            }
        }

        #endregion
    }
}
