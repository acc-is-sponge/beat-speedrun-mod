using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using BeatSpeedrun.Models;
using BeatSpeedrun.Models.Speedrun;

namespace BeatSpeedrun.Managers
{
    internal class SpeedrunManager
    {
        private readonly RegulationManager _regulationManager;
        private readonly MapSetManager _mapSetManager;

        internal SpeedrunManager(RegulationManager regulationManager, MapSetManager mapSetManager)
        {
            _regulationManager = regulationManager;
            _mapSetManager = mapSetManager;
        }

        internal async Task<Speedrun> CreateAsync(
            string regulationPath,
            Segment? targetSegment,
            CancellationToken ct)
        {
            var regulation = await _regulationManager.GetAsync(regulationPath, ct);
            var mapSet = await _mapSetManager.GetAsync(regulation.Rules.MapSet, ct);

            var startedAt = DateTime.UtcNow;
            var snapshot = new Snapshot
            {
                Id = ((long)(startedAt - UnixEpoch).TotalMilliseconds).ToString(),
                StartedAt = startedAt,
                Regulation = regulationPath,
                TargetSegment = targetSegment,
                Checksum = new SnapshotChecksum
                {
                    Regulation = regulation.ComputeChecksum(),
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

        internal async Task<Speedrun> LoadAsync(string id, CancellationToken ct)
        {
            var path = Path.Combine(SpeedrunsDirectory, id);
            var json = SomeDecrypt(File.ReadAllBytes(path));
            var snapshot = Snapshot.FromJson(json);
            var regulation = await _regulationManager.GetAsync(snapshot.Regulation, ct);
            var mapSet = await _mapSetManager.GetAsync(regulation.Rules.MapSet, ct);
            return new Speedrun(snapshot, regulation, mapSet);
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
    }
}
