#region using
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace mcs.core
{
    public class RoleManagerService : IServiceManager
    {
        private readonly ILogger<RoleManagerService> _logger;
        private readonly bool _useNewtonsoftSerialization;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _serviceName;

        public RoleManagerService(
            ILogger<RoleManagerService> logger,
            HttpClient httpClient,
            IConfiguration config)
        {
            _logger = logger;
            using (var scope = _logger.BeginMethodScope(new { httpClient, config }))
            {
                _httpClient = httpClient;
                _configuration = config;

                _serviceName = _configuration["RoleManagerApi:Name"];
                
                var baseUrl = _configuration["RoleManagerApi:BaseUrl"];
                _httpClient.BaseAddress = new Uri(baseUrl);

                var useNewtonsoftSerializationString = _configuration["RoleManagerService:UseNewtonsoftSerialization"];
                _useNewtonsoftSerialization = string.IsNullOrEmpty(useNewtonsoftSerializationString) || bool.Parse(useNewtonsoftSerializationString);

                scope.LogDebug($"{nameof(httpClient.BaseAddress)}={httpClient.BaseAddress},{nameof(useNewtonsoftSerializationString)}=\"{useNewtonsoftSerializationString}\"");
            }
        }

        // Summary: Send a GET request to the specified Uri and return the value resulting from deserialize the response body as JSON in an asynchronous operation.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   type: The type of the object to deserialize to and return.
        //   options: Options to control the behavior during deserialization, the default options are System.Text.Json.JsonSerializerDefaults.Web.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Returns: The task object representing the asynchronous operation.
        public Task<object?> GetAsync(string? requestUri, Type type, object settings, CancellationToken cancellationToken = default)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri, type, settings }))
            {
                return null;
            }
        }
        // Summary: Send a GET request to the specified Uri and return the value resulting from deserialize the response body as JSON in an asynchronous operation.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   type: The type of the object to deserialize to and return.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Returns: The task object representing the asynchronous operation.
        public Task<object?> GetAsync(string? requestUri, Type type, CancellationToken cancellationToken = default)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri, type }))
            {
                return null;
            }
        }
        // Summary: Send a GET request to the specified Uri and return the value resulting from deserialize the response body as JSON in an asynchronous operation.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   type: The type of the object to deserialize to and return.
        //   options: Options to control the behavior during deserialization, the default options are System.Text.Json.JsonSerializerDefaults.Web.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Returns: The task object representing the asynchronous operation.
        public Task<object?> GetAsync(Uri? requestUri, Type type, object settings, CancellationToken cancellationToken = default)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri, type, settings }))
            {
                return null;
            }
        }
        // Summary: Send a GET request to the specified Uri and return the value resulting from deserialize the response body as JSON in an asynchronous operation.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   type: The type of the object to deserialize to and return.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Returns: The task object representing the asynchronous operation.
        public Task<object?> GetAsync(Uri? requestUri, Type type, CancellationToken cancellationToken = default)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri, type }))
            {
                return null;
            }
        }
        // Summary: Send a GET request to the specified Uri and return the value resulting from deserialize the response body as JSON in an asynchronous operation.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   options: Options to control the behavior during deserialization, the default options are System.Text.Json.JsonSerializerDefaults.Web.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters: TValue: The target type to deserialize to.
        // Returns: The task object representing the asynchronous operation.
        public async Task<TValue?> GetAsync<TValue>(string? requestUri, object settings, CancellationToken cancellationToken = default)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri, settings }))
            {
                var ret = default(TValue?);
                if (_useNewtonsoftSerialization)
                {
                    var settingsNS = (JsonSerializerSettings?)settings;
                    ret = await _httpClient.GetFromNewtonsoftJsonAsync<TValue>(requestUri, settingsNS, cancellationToken);
                }
                else
                {
                    var settingsSTJ = (JsonSerializerOptions?)settings;
                    ret = await _httpClient.GetFromJsonAsync<TValue>(requestUri, settingsSTJ, cancellationToken);
                }
                scope.Result = ret;
                return ret;
            }
        }
        // Summary: Send a GET request to the specified Uri and return the value resulting from deserialize the response body as JSON in an asynchronous operation.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters: TValue: The target type to deserialize to.
        // Returns: The task object representing the asynchronous operation.
        public async Task<TValue?> GetAsync<TValue>(string? requestUri, CancellationToken cancellationToken = default)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri }))
            {
                scope.LogDebug($"Uri:{_httpClient.BaseAddress}{requestUri}");

                var ret = default(TValue?);
                var client = _httpClient;

                try
                {
                    if (_useNewtonsoftSerialization)
                    {
                        ret = await client.GetFromNewtonsoftJsonAsync<TValue>(requestUri, cancellationToken);
                    }
                    else
                    {
                        ret = await client.GetFromJsonAsync<TValue>(requestUri, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    TraceLogger.LogException(ex);
                }
                scope.Result = ret;
                return ret;
            }
        }
        // Summary: Send a GET request to the specified Uri and return the value resulting from deserialize the response body as JSON in an asynchronous operation.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   options: Options to control the behavior during deserialization, the default options are System.Text.Json.JsonSerializerDefaults.Web.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters: TValue: The target type to deserialize to.
        // Returns: The task object representing the asynchronous operation.
        public async Task<TValue?> GetAsync<TValue>(Uri? requestUri, object settings, CancellationToken cancellationToken = default)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri, settings }))
            {
                var ret = default(TValue?);
                var response = default(HttpResponseMessage);
                try
                {
                    response = await _httpClient.GetAsync(requestUri, cancellationToken);
                }
                finally
                {
                    TraceLogger.LogInformation($"{nameof(GetAsync)} '{_httpClient?.BaseAddress}/{requestUri?.GetLogString()}' returned {response?.StatusCode} (IsSuccessStatusCode:{response?.IsSuccessStatusCode})");
                }

                if (response.IsSuccessStatusCode)
                {
                    var returnuserdata = await response.Content.ReadAsStringAsync(cancellationToken);
                    ret = JsonConvert.DeserializeObject<TValue>(returnuserdata, (JsonSerializerSettings?)settings);
                }
                return ret;
            }
        }
        // Summary: Send a GET request to the specified Uri and return the value resulting from deserialize the response body as JSON in an asynchronous operation.
        // Parameters: client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters: TValue: The target type to deserialize to.
        // Returns: The task object representing the asynchronous operation.
        public async Task<TValue?> GetAsync<TValue>(Uri? requestUri, CancellationToken cancellationToken = default)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri }))
            {
                var ret = default(TValue?);
                var response = default(HttpResponseMessage);
                try
                {
                    await _httpClient.GetAsync(requestUri, cancellationToken);
                }
                finally
                {
                    TraceLogger.LogInformation($"{nameof(GetAsync)} '{_httpClient?.BaseAddress}/{requestUri?.GetLogString()}' returned {response?.StatusCode} (IsSuccessStatusCode:{response?.IsSuccessStatusCode})");
                }

                if (response.IsSuccessStatusCode)
                {
                    var returnuserdata = await response.Content.ReadAsStringAsync(cancellationToken);
                    ret = JsonConvert.DeserializeObject<TValue>(returnuserdata);
                }
                return ret;
            }
        }
        // Summary: Send a POST request to the specified Uri containing the value serialized as JSON in the request body.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   value: The value to serialize.
        //   options: Options to control the behavior during serialization, the default options are System.Text.Json.JsonSerializerDefaults.Web.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters: TValue: The type of the value to serialize.
        // Returns: The task object representing the asynchronous operation.
        public async Task<HttpResponseMessage> PostAsync<TValue>(string? requestUri, TValue value, object settings = null, CancellationToken cancellationToken = default)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri, value, settings }))
            {
                var payload = JsonConvert.SerializeObject(value, (JsonSerializerSettings?)settings);

                HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = default(HttpResponseMessage);
                try
                {
                    response = await _httpClient.PostAsync(requestUri, content, cancellationToken);
                }
                finally
                {
                    TraceLogger.LogInformation($"{nameof(PostAsync)} '{_httpClient?.BaseAddress}/{requestUri?.GetLogString()}' (content:{content?.GetLogString()}) returned {response?.StatusCode} (IsSuccessStatusCode:{response?.IsSuccessStatusCode})");
                }

                return response;
            }
        }
        // Summary: Send a POST request to the specified Uri containing the value serialized as JSON in the request body.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   value: The value to serialize.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters:
        //   TValue: The type of the value to serialize.
        // Returns: The task object representing the asynchronous operation.
        public async Task<HttpResponseMessage> PostAsync<TValue>(string? requestUri, TValue value, CancellationToken cancellationToken)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri, value }))
            {
                var payload = JsonConvert.SerializeObject(value);

                HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = default(HttpResponseMessage);
                try
                {
                    await _httpClient.PostAsync(requestUri, content, cancellationToken);
                }
                finally
                {
                    TraceLogger.LogInformation($"{nameof(PostAsync)} '{_httpClient?.BaseAddress}/{requestUri?.GetLogString()}' (content:{content?.GetLogString()}) returned {response?.StatusCode} (IsSuccessStatusCode:{response?.IsSuccessStatusCode})");
                }

                return response;
            }
        }
        // Summary:
        //     Send a POST request to the specified Uri containing the value serialized as JSON
        //     in the request body.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   value: The value to serialize.
        //   options: Options to control the behavior during serialization, the default options are System.Text.Json.JsonSerializerDefaults.Web.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters:
        //   TValue: The type of the value to serialize.
        //
        // Returns: The task object representing the asynchronous operation.
        public async Task<HttpResponseMessage> PostAsync<TValue>(Uri? requestUri, TValue value, object settings = null, CancellationToken cancellationToken = default)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri, value, settings }))
            {
                var payload = JsonConvert.SerializeObject(value, (JsonSerializerSettings?)settings);
                HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = default(HttpResponseMessage);
                try
                {
                    response = await _httpClient.PostAsync(requestUri, content, cancellationToken);
                }
                finally
                {
                    TraceLogger.LogInformation($"{nameof(PostAsync)} '{_httpClient?.BaseAddress}/{requestUri?.GetLogString()}' (content:{content?.GetLogString()}) returned {response?.StatusCode} (IsSuccessStatusCode:{response?.IsSuccessStatusCode})");
                }

                return response;
            }
        }
        // Summary:
        //     Send a POST request to the specified Uri containing the value serialized as JSON in the request body.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   value: The value to serialize.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters:
        //   TValue: The type of the value to serialize.
        //
        // Returns: The task object representing the asynchronous operation.
        public async Task<HttpResponseMessage> PostAsync<TValue>(Uri? requestUri, TValue value, CancellationToken cancellationToken)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri, value }))
            {
                var payload = JsonConvert.SerializeObject(value);
                HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = default(HttpResponseMessage);
                try
                {
                    response = await _httpClient.PostAsync(requestUri, content, cancellationToken);
                }
                finally
                {
                    TraceLogger.LogInformation($"{nameof(PostAsync)} '{_httpClient?.BaseAddress}/{requestUri?.GetLogString()}' (content:{content?.GetLogString()}) returned {response?.StatusCode} (IsSuccessStatusCode:{response?.IsSuccessStatusCode})");
                }

                return response;
            }
        }
        // Summary: Send a PUT request to the specified Uri containing the value serialized as JSON in the request body.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   value: The value to serialize.
        //   options: Options to control the behavior during serialization, the default options are System.Text.Json.JsonSerializerDefaults.Web.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters: TValue: The type of the value to serialize.
        // Returns: The task object representing the asynchronous operation.
        public async Task<HttpResponseMessage> PutAsync<TValue>(string? requestUri, TValue value, object settings = null, CancellationToken cancellationToken = default)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri, value, settings }))
            {
                var payload = JsonConvert.SerializeObject(value, (JsonSerializerSettings?)settings);
                HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = default(HttpResponseMessage);
                try
                {
                    response = await _httpClient.PutAsync(requestUri, content, cancellationToken);
                }
                finally
                {
                    TraceLogger.LogInformation($"{nameof(PutAsync)} '{_httpClient?.BaseAddress}/{requestUri?.GetLogString()}' (content:{content?.GetLogString()}) returned {response?.StatusCode} (IsSuccessStatusCode:{response?.IsSuccessStatusCode})");
                }

                return response;
            }
        }
        // Summary: Send a PUT request to the specified Uri containing the value serialized as JSON in the request body.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   value: The value to serialize.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters: TValue: The type of the value to serialize.
        // Returns: The task object representing the asynchronous operation.
        public async Task<HttpResponseMessage> PutAsync<TValue>(string? requestUri, TValue value, CancellationToken cancellationToken)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri, value }))
            {
                var payload = JsonConvert.SerializeObject(value);
                HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = default(HttpResponseMessage);
                try
                {
                    response = await _httpClient.PutAsync(requestUri, content, cancellationToken);
                }
                finally
                {
                    TraceLogger.LogInformation($"{nameof(PutAsync)} '{_httpClient?.BaseAddress}/{requestUri?.GetLogString()}' (content:{content?.GetLogString()}) returned {response?.StatusCode} (IsSuccessStatusCode:{response?.IsSuccessStatusCode})");
                }

                return response;
            }
        }
        // Summary: Send a PUT request to the specified Uri containing the value serialized as JSON in the request body.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   value: The value to serialize.
        //   options: Options to control the behavior during serialization, the default options are System.Text.Json.JsonSerializerDefaults.Web.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters:
        //   TValue: The type of the value to serialize.
        //
        // Returns:The task object representing the asynchronous operation.
        public async Task<HttpResponseMessage> PutAsync<TValue>(Uri? requestUri, TValue value, object settings = null, CancellationToken cancellationToken = default)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri, value, settings }))
            {
                var payload = JsonConvert.SerializeObject(value, (JsonSerializerSettings?)settings);
                HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = default(HttpResponseMessage);
                try
                {
                    response = await _httpClient.PutAsync(requestUri, content, cancellationToken);
                }
                finally
                {
                    TraceLogger.LogInformation($"{nameof(PutAsync)} '{_httpClient?.BaseAddress}/{requestUri?.GetLogString()}' (content:{content?.GetLogString()}) returned {response?.StatusCode} (IsSuccessStatusCode:{response?.IsSuccessStatusCode})");
                }

                return response;
            }
        }
        // Summary: Send a PUT request to the specified Uri containing the value serialized as JSON in the request body.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   value: The value to serialize.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters:
        //   TValue: The type of the value to serialize.
        // Returns: The task object representing the asynchronous operation.
        public async Task<HttpResponseMessage> PutAsync<TValue>(Uri? requestUri, TValue value, CancellationToken cancellationToken)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri, value }))
            {
                var payload = JsonConvert.SerializeObject(value);
                HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = default(HttpResponseMessage);
                try
                {
                    response = await _httpClient.PutAsync(requestUri, content, cancellationToken);
                }
                finally
                {
                    TraceLogger.LogInformation($"{nameof(PutAsync)} '{_httpClient?.BaseAddress}/{requestUri?.GetLogString()}' (content:{content?.GetLogString()}) returned {response?.StatusCode} (IsSuccessStatusCode:{response?.IsSuccessStatusCode})");
                }

                return response;
            }
        }
        // Summary: Send a DELETE request to the specified Uri
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   options: Options to control the behavior during serialization, the default options are System.Text.Json.JsonSerializerDefaults.Web.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters: TValue: The type of the value to serialize.
        // Returns: The task object representing the asynchronous operation.
        public async Task<HttpResponseMessage> DeleteAsync<TValue>(string? requestUri, object settings = null, CancellationToken cancellationToken = default)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri, settings }))
            {
                var response = default(HttpResponseMessage);
                try
                {
                    response = await _httpClient.DeleteAsync(requestUri, cancellationToken);
                }
                finally
                {
                    TraceLogger.LogInformation($"{nameof(DeleteAsync)} '{_httpClient?.BaseAddress}/{requestUri?.GetLogString()}' returned {response?.StatusCode} (IsSuccessStatusCode:{response?.IsSuccessStatusCode})");
                }

                return response;
            }
        }
        // Summary: Send a DELETE request to the specified Uri
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters: TValue: The type of the value to serialize.
        // Returns: The task object representing the asynchronous operation.
        public async Task<HttpResponseMessage> DeleteAsync<TValue>(string? requestUri, CancellationToken cancellationToken)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri }))
            {
                var response = default(HttpResponseMessage);
                try
                {
                    response = await _httpClient.DeleteAsync(requestUri, cancellationToken);
                }
                finally
                {
                    TraceLogger.LogInformation($"{nameof(DeleteAsync)} '{_httpClient?.BaseAddress}/{requestUri?.GetLogString()}' returned {response?.StatusCode} (IsSuccessStatusCode:{response?.IsSuccessStatusCode})");
                }

                return response;
            }
        }
        // Summary: Send a DELETE request to the specified Uri
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   options: Options to control the behavior during serialization, the default options are System.Text.Json.JsonSerializerDefaults.Web.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters:
        //   TValue: The type of the value to serialize.
        //
        // Returns:The task object representing the asynchronous operation.
        public async Task<HttpResponseMessage> DeleteAsync<TValue>(Uri? requestUri, object settings = null, CancellationToken cancellationToken = default)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri, settings }))
            {
                var response = default(HttpResponseMessage);
                try
                {
                    response = await _httpClient.DeleteAsync(requestUri, cancellationToken);
                }
                finally
                {
                    TraceLogger.LogInformation($"{nameof(DeleteAsync)} '{_httpClient?.BaseAddress}/{requestUri?.GetLogString()}' returned {response?.StatusCode} (IsSuccessStatusCode:{response?.IsSuccessStatusCode})");
                }

                return response;
            }
        }
        // Summary: Send a DELETE request to the specified Uri
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters:
        //   TValue: The type of the value to serialize.
        // Returns: The task object representing the asynchronous operation.
        public async Task<HttpResponseMessage> DeleteAsync<TValue>(Uri? requestUri, CancellationToken cancellationToken)
        {
            using (var scope = _logger.BeginMethodScope(new { requestUri }))
            {
                var response = default(HttpResponseMessage);
                try
                {
                    response = await _httpClient.DeleteAsync(requestUri, cancellationToken);
                }
                finally
                {
                    TraceLogger.LogInformation($"{nameof(DeleteAsync)} '{_httpClient?.BaseAddress}/{requestUri?.GetLogString()}' returned {response?.StatusCode} (IsSuccessStatusCode:{response?.IsSuccessStatusCode})");
                }

                return response;
            }
        }
    }
}
