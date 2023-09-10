using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeatSpeedrun.Extensions
{
    internal static class StringExtensions
    {
        internal static string ComputeChecksum(this string s)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(s));
                var dest = new StringBuilder();
                foreach (byte b in hash)
                {
                    dest.Append(b.ToString("x2"));
                }
                return dest.ToString();
            }
        }
    }

    internal static class TaskExtensions
    {
        internal static Task<T> WithCancellation<T>(this Task<T> task, CancellationToken ct)
        {
            if (task.IsCompleted || ct == CancellationToken.None) return task;
            var tcs = new TaskCompletionSource<T>();
            ct.Register(s => tcs.TrySetCanceled(), tcs);
            return Task.WhenAny(task, tcs.Task).ContinueWith(t => task.Result);
        }
    }
}
