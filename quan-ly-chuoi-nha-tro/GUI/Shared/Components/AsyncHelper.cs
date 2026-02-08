using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace quan_ly_chuoi_nha_tro.GUI.Shared.Components
{
    /// <summary>
    /// Helper utilities for async operations
    /// </summary>
    public static class AsyncHelper
    {
        /// <summary>
        /// Execute with retry logic
        /// </summary>
        public static async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            int maxRetries = 3,
            int delayMs = 500)
        {
            Exception lastException = null;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (i < maxRetries - 1)
                    {
                        // Exponential backoff
                        await Task.Delay(delayMs * (int)Math.Pow(2, i));
                    }
                }
            }

            throw new Exception($"Operation failed after {maxRetries} retries", lastException);
        }

        /// <summary>
        /// Execute with retry logic (no return value)
        /// </summary>
        public static async Task ExecuteWithRetryAsync(
            Func<Task> operation,
            int maxRetries = 3,
            int delayMs = 500)
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await operation();
                return true;
            }, maxRetries, delayMs);
        }

        /// <summary>
        /// Execute with timeout
        /// </summary>
        public static async Task<T> ExecuteWithTimeoutAsync<T>(
            Func<Task<T>> operation,
            TimeSpan timeout)
        {
            using (var cts = new CancellationTokenSource(timeout))
            {
                var task = operation();
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout));

                if (completedTask == task)
                {
                    return await task;
                }

                throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds} seconds");
            }
        }
    }

    /// <summary>
    /// Debouncer for search input
    /// Delays execution until user stops typing
    /// </summary>
    public class Debouncer : IDisposable
    {
        private CancellationTokenSource _cts;
        private readonly int _delayMs;

        public Debouncer(int delayMs = 300)
        {
            _delayMs = delayMs;
        }

        /// <summary>
        /// Debounce an action
        /// </summary>
        public async Task DebounceAsync(Action action)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                await Task.Delay(_delayMs, _cts.Token);
                action?.Invoke();
            }
            catch (TaskCanceledException)
            {
                // Expected when cancelled
            }
        }

        /// <summary>
        /// Debounce an async action
        /// </summary>
        public async Task DebounceAsync(Func<Task> action)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                await Task.Delay(_delayMs, _cts.Token);
                if (action != null)
                    await action();
            }
            catch (TaskCanceledException)
            {
                // Expected when cancelled
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }

    /// <summary>
    /// Extension methods for async UI operations
    /// </summary>
    public static class AsyncUIExtensions
    {
        /// <summary>
        /// Run action on UI thread
        /// </summary>
        public static void InvokeIfRequired(this Control control, Action action)
        {
            if (control == null || control.IsDisposed) return;

            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// Run action on UI thread async
        /// </summary>
        public static async Task InvokeAsync(this Control control, Action action)
        {
            if (control == null || control.IsDisposed) return;

            if (control.InvokeRequired)
            {
                await Task.Run(() => control.Invoke(action));
            }
            else
            {
                action();
            }
        }
    }
}
