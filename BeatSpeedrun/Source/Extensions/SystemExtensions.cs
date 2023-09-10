using System;
using System.Collections.Generic;
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

    internal class TaskWaiter : IDisposable
    {
        private readonly HashSet<Task> _waitingResources = new HashSet<Task>();
        private readonly Action _action;
        private readonly CancellationTokenSource _disposeCts = new CancellationTokenSource();

        internal TaskWaiter(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            _disposeCts.Cancel();
        }

        internal T Wait<T>(Task<T> resource, T defval = default)
        {
            switch (resource.Status)
            {
                case TaskStatus.Canceled:
                case TaskStatus.Faulted:
                    return defval;
                case TaskStatus.RanToCompletion:
                    return resource.Result;
                default:
                    if (_waitingResources.Add(resource))
                    {
                        _ = ScheduleActionAsync(resource);
                    }
                    return defval;
            }
        }

        private async Task ScheduleActionAsync<T>(Task<T> resource)
        {
            try
            {
                await resource.WithCancellation(_disposeCts.Token);
            }
            catch (TaskCanceledException) when (_disposeCts.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                Plugin.Log.Warn(ex);
            }
            _waitingResources.Remove(resource);
            _action();
        }
    }
}
