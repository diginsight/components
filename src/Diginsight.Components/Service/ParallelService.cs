using Diginsight.Diagnostics;
using Diginsight.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

namespace Diginsight.Components
{

    public sealed class ParallelServiceOptions : IParallelServiceOptions, IDynamicallyConfigurable, IVolatilelyConfigurable
    {
        public int MaxConcurrency { get; set; }
        public int LowConcurrency { get; set; }
        public int MediumConcurrency { get; set; }
        public int HighConcurrency { get; set; }
    }

    public class ParallelService : IParallelService
    {
        private const int LOWCONCURRENCY_DEFAULT = 3;
        private const int MEDIUMCONCURRENCY_DEFAULT = 6;
        private const int HIGHCONCURRENCY_DEFAULT = 12;

        private readonly ILogger<ParallelService> logger;
        //private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IParallelServiceOptions options;

        private int? lowConcurrency;
        private int? mediumConcurrency;
        private int? highConcurrency;

        public int LowConcurrency
        {
            get
            {
                if (lowConcurrency != null) { return lowConcurrency.Value; }
                var baseLowConcurrency = options.LowConcurrency > 0 ? options.LowConcurrency : LOWCONCURRENCY_DEFAULT;
                lowConcurrency = options.MaxConcurrency > 0 && options.MaxConcurrency < baseLowConcurrency ? options.MaxConcurrency : baseLowConcurrency;
                return lowConcurrency.Value;
            }
        }
        public int MediumConcurrency
        {
            get
            {
                if (mediumConcurrency != null) { return mediumConcurrency.Value; }
                var baseMediumConcurrency = options.MediumConcurrency > 0 ? options.MediumConcurrency : MEDIUMCONCURRENCY_DEFAULT;
                mediumConcurrency = options.MaxConcurrency > 0 && options.MaxConcurrency < baseMediumConcurrency ? options.MaxConcurrency : baseMediumConcurrency;
                return mediumConcurrency.Value;
            }
        }
        public int HighConcurrency
        {
            get
            {
                if (highConcurrency != null) { return highConcurrency.Value; }
                var baseHighConcurrency = options.HighConcurrency > 0 ? options.HighConcurrency : HIGHCONCURRENCY_DEFAULT;
                highConcurrency = options.MaxConcurrency > 0 && options.MaxConcurrency < baseHighConcurrency ? options.MaxConcurrency : baseHighConcurrency;
                return highConcurrency.Value;
            }
        }

        public ParallelService(ILogger<ParallelService> logger,
                               //IHttpContextAccessor httpContextAccessor,
                               IOptions<ParallelServiceOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;
            //this.httpContextAccessor = httpContextAccessor;
        }

        public void ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource> body)
        {
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { source, parallelOptions });

            if (source?.Any() != true) { return; }

            SetMaxConcurrency(parallelOptions); //, scope

            Parallel.ForEach(source, parallelOptions ?? new ParallelOptions(), body);
        }

        public async Task ForEachAsync<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Func<TSource, Task> body)
        {
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { source, parallelOptions });

            if (source?.Any() != true) { return; }

            if (parallelOptions == null)
            {
                parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = options.MaxConcurrency > 0 ? options.MaxConcurrency : Environment.ProcessorCount };
            }

            try
            {
#if NET6_0_OR_GREATER
                await Parallel.ForEachAsync(
                    source,
                    parallelOptions ?? new ParallelOptions(),
                    async (item, _) => { await body(item); });
#else
                    await Parallel_ForEachAsync<TSource>(source, parallelOptions.MaxDegreeOfParallelism, body);
#endif
            }
            catch (BreakLoopException ex)
            {
                var item = ex.Data.Contains("item") ? ex.Data["item"] : null;
                //scope.LogDebug($"Break Loop occurred on source:{source.GetLogString()}, item:{item.GetLogString()}");
            }
        }

        async Task IParallelService.WhenAllAsync(IEnumerable<Func<Task>> taskFactories, ParallelOptions parallelOptions)
        {
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { taskFactories, parallelOptions });

            if (taskFactories?.Any() != true) { return; }

            await ForEachAsync(taskFactories, parallelOptions, async (taskFactory) => { await taskFactory(); });
        }
        async Task<IEnumerable<T>> IParallelService.WhenAllAsync<T>(IEnumerable<Func<Task<T>>> taskFactories, ParallelOptions parallelOptions)
        {
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { taskFactories, parallelOptions });
            if (taskFactories?.Any() != true) { return default(IEnumerable<T>)!; }

            var taskFactoryList = taskFactories.ToList();
            var results = new ConcurrentDictionary<int, T>();
            
            await ForEachAsync(taskFactoryList.Select((factory, index) => new { Factory = factory, Index = index }), 
                parallelOptions, 
                async (item) =>
                {
                    var result = await item.Factory();
                    results[item.Index] = result;
                });

            return taskFactoryList.Select((_, index) => results[index]);
        }
        // Tuple decomposition overloads
        async Task<(T1, T2)> IParallelService.WhenAllAsync<T1, T2>(Func<Task<T1>> taskFactory1, Func<Task<T2>> taskFactory2, ParallelOptions parallelOptions)
        {
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { taskFactory1, taskFactory2, parallelOptions });

            var results = await ((IParallelService)this).WhenAllAsync(new Func<Task<object>>[]
            {
                async () => (await taskFactory1())!,
                async () => (await taskFactory2())!
            }, parallelOptions);

            var resultArray = results.ToArray();
            return ((T1)resultArray[0], (T2)resultArray[1]);
        }
        async Task<(T1, T2, T3)> IParallelService.WhenAllAsync<T1, T2, T3>(Func<Task<T1>> taskFactory1, Func<Task<T2>> taskFactory2, Func<Task<T3>> taskFactory3, ParallelOptions parallelOptions)
        {
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { taskFactory1, taskFactory2, taskFactory3, parallelOptions });

            var results = await ((IParallelService)this).WhenAllAsync(new Func<Task<object>>[]
            {
                async () => (await taskFactory1())!,
                async () => (await taskFactory2())!,
                async () => (await taskFactory3())!
            }, parallelOptions);

            var resultArray = results.ToArray();
            return ((T1)resultArray[0], (T2)resultArray[1], (T3)resultArray[2]);
        }
        async Task<(T1, T2, T3, T4)> IParallelService.WhenAllAsync<T1, T2, T3, T4>(Func<Task<T1>> taskFactory1, Func<Task<T2>> taskFactory2, Func<Task<T3>> taskFactory3, Func<Task<T4>> taskFactory4, ParallelOptions parallelOptions)
        {
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { taskFactory1, taskFactory2, taskFactory3, taskFactory4, parallelOptions });

            var results = await ((IParallelService)this).WhenAllAsync(new Func<Task<object>>[]
            {
                async () => (await taskFactory1())!,
                async () => (await taskFactory2())!,
                async () => (await taskFactory3())!,
                async () => (await taskFactory4())!
            }, parallelOptions);

            var resultArray = results.ToArray();
            return ((T1)resultArray[0], (T2)resultArray[1], (T3)resultArray[2], (T4)resultArray[3]);
        }

        const string headerName = "MaxConcurrency";
        private void SetMaxConcurrency(ParallelOptions parallelOptions) // , CodeSectionScope scope
        {
            if (parallelOptions == null) { return; }

            const string settingName = "MaxConcurrency";

            //if (httpContextAccessor?.HttpContext?.Request.Headers.TryGetValue(settingName, out StringValues headerValues) == true
            //    && int.TryParse(headerValues.LastOrDefault(), out int headerMaxConcurrency))
            //{
            //    ////scope.LogInformation($"From header: {settingName}={headerMaxConcurrency}");
            //    parallelOptions.MaxDegreeOfParallelism = headerMaxConcurrency;
            //    return;
            //}

            var maxConcurrencyVariable = Environment.GetEnvironmentVariable(settingName);
            if (!string.IsNullOrEmpty(maxConcurrencyVariable) && int.TryParse(maxConcurrencyVariable, out int variableMaxConcurrency))
            {
                ////scope.LogInformation($"From environment: {settingName}={variableMaxConcurrency}");
                parallelOptions.MaxDegreeOfParallelism = variableMaxConcurrency;
                return;
            }
        }

        public static Task Parallel_ForEachAsync<T>(IEnumerable<T> source, int maxDegreeOfParallelism, Func<T, Task> action)
        {
            var options = new ExecutionDataflowBlockOptions();
            options.MaxDegreeOfParallelism = maxDegreeOfParallelism;
            var block = new ActionBlock<T>(action, options);
            foreach (var item in source) block.Post(item);
            block.Complete();
            return block.Completion;
        }
    }
}
