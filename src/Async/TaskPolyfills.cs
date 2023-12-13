using System;
using System.Threading.Tasks;

namespace VarDump.Async
{
    public static class TaskPolyfills
    {
#if NETSTANDARD
        public static Task<T> FromResult<T>(T result)
        {
            return Task.FromResult(result);
        }

        public static Task FromException(Exception exception)
        {
            return Task.FromException(exception);
        }

        public static Task<T> FromException<T>(Exception exception)
        {
            return Task.FromException<T>(exception);
        }

        public static Task CompletedTask => TaskPolyfills.CompletedTask;
#else
        public static Task<T> FromResult<T>(T result)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(result);
            return tcs.Task;
        }

        public static Task FromException(Exception exception)
        {
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetException(exception);
            return tcs.Task;
        }

        public static Task<T> FromException<T>(Exception exception)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetException(exception);
            return tcs.Task;
        }

        public static Task CompletedTask
        {
            get
            {
                var tcs = new TaskCompletionSource<bool>();
                tcs.SetResult(true);
                return tcs.Task;
            }
        }
#endif
    }
}
