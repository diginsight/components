#region using
#endregion

using Diginsight.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Diginsight.Components;

public static class ParallelExtensions
{
    private static readonly Type T = typeof(ParallelExtensions);

    private static ILogger? cachedLogger;
    private static ILogger? logger => cachedLogger ??= Observability.LoggerFactory?.CreateLogger(typeof(ParallelExtensions));

    public static async Task<IEnumerable<R>> ForEachAsync<T, R>(this IEnumerable<T> source, Func<T, Task<R>> body, ParallelOptions options)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(ParallelExtensions.T, logger, new { body, options });

        var tasks = new List<Task<R>>();

        int maxDegreeOfParallelism = options.MaxDegreeOfParallelism > 0 ? options.MaxDegreeOfParallelism : Environment.ProcessorCount;
        using (var semaphore = new SemaphoreSlim(maxDegreeOfParallelism))
        {
            foreach (var item in source)
            {
                await semaphore.WaitAsync();
                try
                {
                    tasks.Add(body(item));
                }
                finally { semaphore.Release(); }
            }
        }
        var result = await Task.WhenAll(tasks);
        activity?.SetOutput(result);
        return result;
    }

    public static async Task<IEnumerable<T>> WhenAllAsync<T>(this IEnumerable<Func<Task<T>>> asyncFuncs, ParallelOptions parallelOptions = null)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(ParallelExtensions.T, logger, new { asyncFuncs, parallelOptions });

        if (asyncFuncs == null) throw new ArgumentNullException(nameof(asyncFuncs));
        parallelOptions ??= new ParallelOptions();

        int index = 0;
        var resultsDictionary = new ConcurrentDictionary<int, T>();
        var indexedFuncs = asyncFuncs.Select(func => (Index: Interlocked.Increment(ref index) - 1, Func: func)).ToList();
        

        await Parallel.ForEachAsync(indexedFuncs, parallelOptions, async (item, cancellationToken) =>
        {
            var result = await item.Func();
            resultsDictionary[item.Index] = result;
        });

        var result = indexedFuncs.OrderBy(x => x.Index).Select(x => resultsDictionary[x.Index]);

        activity?.SetOutput(result);
        return result;
    }


}
