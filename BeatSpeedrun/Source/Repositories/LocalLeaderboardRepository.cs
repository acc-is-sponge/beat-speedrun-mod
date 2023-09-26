using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using BeatSpeedrun.Models;
using BeatSpeedrun.Models.Leaderboard;

namespace BeatSpeedrun.Repositories
{
    internal class LocalLeaderboardRepository
    {
        internal LocalLeaderboard Get(Regulation regulation)
        {
            var key = LocalLeaderboard.GetKey(regulation);
            try
            {
                var path = Path.Combine(LeaderboardsDirectory, key);
                if (File.Exists(path))
                {
                    var json = SomeDecrypt(File.ReadAllBytes(path));
                    return LocalLeaderboard.FromJson(json);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Cannot load local leaderboard file for {regulation.Title}:\n{ex}");
            }

            return new LocalLeaderboard()
            {
                Key = key,
                Records = new Dictionary<string, LocalLeaderboardRecord>(),
            };
        }

        internal void Save(LocalLeaderboard leaderboard)
        {
            var path = Path.Combine(LeaderboardsDirectory, leaderboard.Key);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, SomeEncrypt(leaderboard.ToJson()));
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

        private static readonly string LeaderboardsDirectory =
            Path.Combine(Environment.CurrentDirectory, "UserData", "BeatSpeedrun", "LocalLeaderboards");
    }
}
