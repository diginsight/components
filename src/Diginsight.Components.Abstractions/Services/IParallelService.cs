using System.Runtime.Serialization;

namespace Diginsight.Components
{
    public interface IParallelService
    {
        int LowConcurrency { get; }
        int MediumConcurrency { get; }
        int HighConcurrency { get; }

        void ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource> body);

        Task ForEachAsync<TSource>(IEnumerable<TSource> source, ParallelOptions options, Func<TSource, Task> body);

        Task WhenAllAsync(IEnumerable<Func<Task>> taskFactories, ParallelOptions parallelOptions);
        Task<IEnumerable<T>> WhenAllAsync<T>(IEnumerable<Func<Task<T>>> taskFactories, ParallelOptions parallelOptions);
        
        // Tuple decomposition overloads
        Task<(T1, T2)> WhenAllAsync<T1, T2>(Func<Task<T1>> taskFactory1, Func<Task<T2>> taskFactory2, ParallelOptions parallelOptions);
        Task<(T1, T2, T3)> WhenAllAsync<T1, T2, T3>(Func<Task<T1>> taskFactory1, Func<Task<T2>> taskFactory2, Func<Task<T3>> taskFactory3, ParallelOptions parallelOptions);
        Task<(T1, T2, T3, T4)> WhenAllAsync<T1, T2, T3, T4>(Func<Task<T1>> taskFactory1, Func<Task<T2>> taskFactory2, Func<Task<T3>> taskFactory3, Func<Task<T4>> taskFactory4, ParallelOptions parallelOptions);
    }

    public interface IParallelServiceOptions
    {
        int MaxConcurrency { get; set; }
        int LowConcurrency { get; set; }
        int MediumConcurrency { get; set; }
        int HighConcurrency { get; set; }
    }

    [Serializable]
    public class BreakLoopException : Exception
    {
        object Item { get; set; }

        public BreakLoopException() : base() { }
        public BreakLoopException(string message) : base(message) { }
        public BreakLoopException(string message, Exception inner) : base(message, inner) { }
        //public BreakLoopException(string message, object item) : base(message) { this.Item = item; }
        //public BreakLoopException(string message, object item, Exception inner) : base(message, inner) { this.Item = item; }
        protected BreakLoopException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    }
}
