#region using
using Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Common
{

    public static class HttpManager
    {
        //private ILogger<HttpManager> logger;
        private static readonly Type T = typeof(HttpManager);

        public static HttpClient GetHttpClient(string mediaType, string authToken = null)
        {
            using (var sec = TraceLogger.BeginMethodScope(T, new { mediaType }))
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
                if (!string.IsNullOrEmpty(authToken))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                }

                sec.Result = client;
                return client;
            }
        }

        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient pthis, string requestUri, HttpContent iContent)
        {
            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = iContent
            };

            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                response = await pthis.SendAsync(request);
                TraceManager.Warning($"Received response. Status: {response.StatusCode}");
            }
            catch (TaskCanceledException tce)
            {
                TraceManager.Exception(tce);
            }

            return response;
        }
        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient pthis, Uri requestUri, HttpContent iContent)
        {
            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = iContent
            };

            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                response = await pthis.SendAsync(request);
                TraceManager.Warning($"Received response. Status: {response.StatusCode}");
            }
            catch (TaskCanceledException tce)
            {
                TraceManager.Exception(tce);
            }

            return response;
        }

        public static Task<HttpResponseMessage> InvokeAsync(this HttpClient pthis, Func<string, Task<HttpResponseMessage>> action, string requestUri)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri);
            return res;
        }
        public static Task<HttpResponseMessage> InvokeAsync(this HttpClient pthis, Func<Uri, Task<HttpResponseMessage>> action, Uri requestUri)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri);
            return res;
        }
        public static Task<HttpResponseMessage> InvokeAsync(this HttpClient pthis, Func<string, HttpCompletionOption, Task<HttpResponseMessage>> action, string requestUri, HttpCompletionOption completionOption)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri, completionOption);
            return res;
        }
        public static Task<HttpResponseMessage> InvokeAsync(this HttpClient pthis, Func<Uri, HttpCompletionOption, Task<HttpResponseMessage>> action, Uri requestUri, HttpCompletionOption completionOption)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri, completionOption);
            return res;
        }
        public static Task<HttpResponseMessage> InvokeAsync(this HttpClient pthis, Func<string, CancellationToken, Task<HttpResponseMessage>> action, string requestUri, CancellationToken cancellationToken)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri, cancellationToken);
            return res;
        }
        public static Task<HttpResponseMessage> InvokeAsync(this HttpClient pthis, Func<Uri, CancellationToken, Task<HttpResponseMessage>> action, Uri requestUri, CancellationToken cancellationToken)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri, cancellationToken);
            return res;
        }
        public static Task<HttpResponseMessage> InvokeAsync(this HttpClient pthis, Func<Uri, HttpCompletionOption, CancellationToken, Task<HttpResponseMessage>> action, Uri requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri, completionOption, cancellationToken);
            return res;
        }
        public static Task<HttpResponseMessage> InvokeAsync(this HttpClient pthis, Func<string, HttpCompletionOption, CancellationToken, Task<HttpResponseMessage>> action, string requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri, completionOption, cancellationToken);
            return res;
        }

        public static Task<HttpResponseMessage> InvokeAsync(this HttpClient pthis, Func<string, HttpContent, Task<HttpResponseMessage>> action, string requestUri, HttpContent content)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri, content);
            return res;
        }
        public static Task<HttpResponseMessage> InvokeAsync(this HttpClient pthis, Func<Uri, HttpContent, Task<HttpResponseMessage>> action, Uri requestUri, HttpContent content)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri, content);
            return res;
        }
        public static Task<HttpResponseMessage> InvokeAsync(this HttpClient pthis, Func<Uri, HttpContent, CancellationToken, Task<HttpResponseMessage>> action, Uri requestUri, HttpContent content, CancellationToken cancellationToken)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri, content, cancellationToken);
            return res;
        }
        public static Task<HttpResponseMessage> InvokeAsync(this HttpClient pthis, Func<string, HttpContent, CancellationToken, Task<HttpResponseMessage>> action, string requestUri, HttpContent content, CancellationToken cancellationToken)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri, content, cancellationToken);
            return res;
        }

        public static Task<byte[]> InvokeAsync(this HttpClient pthis, Func<string, Task<byte[]>> action, string requestUri)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri);
            return res;
        }
        public static Task<byte[]> InvokeAsync(this HttpClient pthis, Func<Uri, Task<byte[]>> action, Uri requestUri)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri);
            return res;
        }

        public static Task<Stream> InvokeAsync(this HttpClient pthis, Func<string, Task<Stream>> action, string requestUri)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri);
            return res;
        }
        public static Task<Stream> InvokeAsync(this HttpClient pthis, Func<Uri, Task<Stream>> action, Uri requestUri)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri);
            return res;
        }
        public static Task<Stream> InvokeAsync(this HttpClient pthis, Func<string, CancellationToken, Task<Stream>> action, string requestUri, CancellationToken cancellationToken)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri, cancellationToken);
            return res;
        }
        public static Task<Stream> InvokeAsync(this HttpClient pthis, Func<Uri, CancellationToken, Task<Stream>> action, Uri requestUri, CancellationToken cancellationToken)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri, cancellationToken);
            return res;
        }

        public static Task<string> InvokeAsync(this HttpClient pthis, Func<string, Task<string>> action, string requestUri)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri);
            return res;
        }
        public static Task<string> InvokeAsync(this HttpClient pthis, Func<Uri, Task<string>> action, Uri requestUri)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri);
            return res;
        }
        public static Task<string> InvokeAsync(this HttpClient pthis, Func<string, CancellationToken, Task<string>> action, string requestUri, CancellationToken cancellationToken)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri, cancellationToken);
            return res;
        }
        public static Task<string> InvokeAsync(this HttpClient pthis, Func<Uri, CancellationToken, Task<string>> action, Uri requestUri, CancellationToken cancellationToken)
        {
            TraceManager.Debug($"InvokeAsync: BaseAddress:{pthis.BaseAddress},requestUri:{requestUri}");
            var res = action(requestUri, cancellationToken);
            return res;
        }


        public static Task<HttpResponseMessage> InvokeAsync(this HttpClient pthis, Func<HttpRequestMessage, Task<HttpResponseMessage>> action, HttpRequestMessage request)
        {
            TraceManager.Debug($"InvokeAsync: Method:{request.Method},BaseAddress:{pthis.BaseAddress},RequestUri:{request.RequestUri},Version:{request.Version},Headers:{request.Headers?.Count()}");
            var res = action(request);
            return res;
        }
        public static Task<HttpResponseMessage> InvokeAsync(this HttpClient pthis, Func<HttpRequestMessage, HttpCompletionOption, Task<HttpResponseMessage>> action, HttpRequestMessage request, HttpCompletionOption completionOption)
        {
            TraceManager.Debug($"InvokeAsync: Method:{request.Method},BaseAddress:{pthis.BaseAddress},RequestUri:{request.RequestUri},Version:{request.Version},Headers:{request.Headers?.Count()}");
            var res = action(request, completionOption);
            return res;
        }
        public static Task<HttpResponseMessage> InvokeAsync(this HttpClient pthis, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> action, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            TraceManager.Debug($"InvokeAsync: Method:{request.Method},BaseAddress:{pthis.BaseAddress},RequestUri:{request.RequestUri},Version:{request.Version},Headers:{request.Headers?.Count()}");
            var res = action(request, cancellationToken);
            return res;
        }
        public static Task<HttpResponseMessage> InvokeAsync(this HttpClient pthis, Func<HttpRequestMessage, HttpCompletionOption, CancellationToken, Task<HttpResponseMessage>> action, HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            TraceManager.Debug($"InvokeAsync: Method:{request.Method},BaseAddress:{pthis.BaseAddress},RequestUri:{request.RequestUri},Version:{request.Version},Headers:{request.Headers?.Count()}");
            var res = action(request, completionOption, cancellationToken);
            return res;
        }
    }
}
