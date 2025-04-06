#region using
#endregion

namespace Diginsight.Components;

public static class ParallelExtensions
{
    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body, int maxDegreeOfParallelism = 4)
    {
        var tasks = new List<Task>();
        using SemaphoreSlim semaphore = new(maxDegreeOfParallelism);
        foreach (var item in source)
        {
            await semaphore.WaitAsync();
            try
            {
                tasks.Add(body(item));
            }
            finally
            {
                semaphore.Release();
            }
        }
        await Task.WhenAll(tasks);
    }
}
