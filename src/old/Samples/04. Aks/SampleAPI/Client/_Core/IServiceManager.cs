using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace mcs.core
{
    public interface IServiceManager
    {
        Task<object?> GetAsync(string? requestUri, Type type, object settings, CancellationToken cancellationToken = default);
        Task<object?> GetAsync(Uri? requestUri, Type type, object settings, CancellationToken cancellationToken = default);
        Task<object?> GetAsync(Uri? requestUri, Type type, CancellationToken cancellationToken = default);
        Task<TValue?> GetAsync<TValue>(string? requestUri, object settings, CancellationToken cancellationToken = default);
        Task<TValue?> GetAsync<TValue>(string? requestUri, CancellationToken cancellationToken = default);
        Task<TValue?> GetAsync<TValue>(Uri? requestUri, object settings, CancellationToken cancellationToken = default);
        Task<TValue?> GetAsync<TValue>(Uri? requestUri, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> PostAsync<TValue>(string? requestUri, TValue value, object settings = null, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> PostAsync<TValue>(string? requestUri, TValue value, CancellationToken cancellationToken);
        Task<HttpResponseMessage> PostAsync<TValue>(Uri? requestUri, TValue value, object settings = null, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> PostAsync<TValue>(Uri? requestUri, TValue value, CancellationToken cancellationToken);
        Task<HttpResponseMessage> PutAsync<TValue>(string? requestUri, TValue value, object settings = null, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> PutAsync<TValue>(string? requestUri, TValue value, CancellationToken cancellationToken);
        Task<HttpResponseMessage> PutAsync<TValue>(Uri? requestUri, TValue value, object settings = null, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> PutAsync<TValue>(Uri? requestUri, TValue value, CancellationToken cancellationToken);
        Task<HttpResponseMessage> DeleteAsync<TValue>(string? requestUri, object settings = null, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> DeleteAsync<TValue>(string? requestUri, CancellationToken cancellationToken);
        Task<HttpResponseMessage> DeleteAsync<TValue>(Uri? requestUri, object settings = null, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> DeleteAsync<TValue>(Uri? requestUri, CancellationToken cancellationToken);
    }
}