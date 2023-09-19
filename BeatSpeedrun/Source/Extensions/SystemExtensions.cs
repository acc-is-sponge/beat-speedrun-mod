using System;
using System.Collections.Concurrent;
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

    internal static class TimeSpanExtensions
    {
        internal static string ToTimerString(this TimeSpan s)
        {
            return $"{(int)s.TotalHours}:{s:mm}:{s:ss}";
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

        /// <param name="action">An action triggerred after the task completion</param>
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

    internal class TaskCache<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, Task<TValue>> _payload;
        private readonly Func<TKey, Task<TValue>, Task<TValue>> _loadAsync;

        internal TaskCache(Func<TKey, Task<TValue>, Task<TValue>> loadAsync)
        {
            _payload = new ConcurrentDictionary<TKey, Task<TValue>>();
            _loadAsync = loadAsync;
        }

        internal Task<TValue> GetAsync(TKey key)
        {
            return _payload.AddOrUpdate(
                key,
                k => _loadAsync(k, null),
                (k, t) => t.IsFaulted ? _loadAsync(k, t) : t);
        }
    }
}
