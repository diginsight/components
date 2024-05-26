#region using
using Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Common.Core.Blazor
{
    public static class HttpClientExtension
    {
        static readonly ILogger _logger = null;

        static HttpClientExtension() { }

        // Summary: Send a GET request to the specified Uri and return the value resulting from deserialize the response body as JSON in an asynchronous operation.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   type: The type of the object to deserialize to and return.
        //   options: Options to control the behavior during deserialization, the default options are System.Text.Json.JsonSerializerDefaults.Web.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Returns: The task object representing the asynchronous operation.
        public static Task<object?> GetFromNewtonsoftJsonAsync(this HttpClient client, string? requestUri, Type type, JsonSerializerSettings? settings, CancellationToken cancellationToken = default)
        {
            return null;
        }
        // Summary: Send a GET request to the specified Uri and return the value resulting from deserialize the response body as JSON in an asynchronous operation.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   type: The type of the object to deserialize to and return.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Returns: The task object representing the asynchronous operation.
        public static Task<object?> GetFromNewtonsoftJsonAsync(this HttpClient client, string? requestUri, Type type, CancellationToken cancellationToken = default)
        {
            return null;
        }
        // Summary: Send a GET request to the specified Uri and return the value resulting from deserialize the response body as JSON in an asynchronous operation.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   type: The type of the object to deserialize to and return.
        //   options: Options to control the behavior during deserialization, the default options are System.Text.Json.JsonSerializerDefaults.Web.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Returns: The task object representing the asynchronous operation.
        public static Task<object?> GetFromNewtonsoftJsonAsync(this HttpClient client, Uri? requestUri, Type type, JsonSerializerSettings? settings, CancellationToken cancellationToken = default)
        {
            return null;
        }
        // Summary: Send a GET request to the specified Uri and return the value resulting from deserialize the response body as JSON in an asynchronous operation.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   type: The type of the object to deserialize to and return.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Returns: The task object representing the asynchronous operation.
        public static Task<object?> GetFromNewtonsoftJsonAsync(this HttpClient client, Uri? requestUri, Type type, CancellationToken cancellationToken = default)
        {
            return null;
        }
        // Summary: Send a GET request to the specified Uri and return the value resulting from deserialize the response body as JSON in an asynchronous operation.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   options: Options to control the behavior during deserialization, the default options are System.Text.Json.JsonSerializerDefaults.Web.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters: TValue: The target type to deserialize to.
        // Returns: The task object representing the asynchronous operation.
        public static async Task<TValue?> GetFromNewtonsoftJsonAsync<TValue>(this HttpClient client, string? requestUri, JsonSerializerSettings? settings, CancellationToken cancellationToken = default)
        {

            var ret = default(TValue?);
            var response = default(HttpResponseMessage);

            //try { 
            response = await client.GetAsync(requestUri, cancellationToken);
            //}
            //finally { TraceLogger.LogInformation($"await client.GetAsync({requestUri}, cancellationToken); returned {response.StatusCode}"); }

            if (response.IsSuccessStatusCode)
            {
                var returnuserdata = await response.Content.ReadAsStringAsync(cancellationToken);
                ret = JsonConvert.DeserializeObject<TValue>(returnuserdata, settings);
            }
            return ret;
        }
        // Summary: Send a GET request to the specified Uri and return the value resulting from deserialize the response body as JSON in an asynchronous operation.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters: TValue: The target type to deserialize to.
        // Returns: The task object representing the asynchronous operation.
        public static async Task<TValue?> GetFromNewtonsoftJsonAsync<TValue>(this HttpClient client, string? requestUri, CancellationToken cancellationToken = default)
        {
            var ret = default(TValue?);

            var response = default(HttpResponseMessage);
            response = await client.GetAsync(requestUri, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var returnuserdata = await response.Content.ReadAsStringAsync(cancellationToken);
                ret = JsonConvert.DeserializeObject<TValue>(returnuserdata);
            }
            return ret;
        }
        // Summary: Send a GET request to the specified Uri and return the value resulting from deserialize the response body as JSON in an asynchronous operation.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   options: Options to control the behavior during deserialization, the default options are System.Text.Json.JsonSerializerDefaults.Web.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters: TValue: The target type to deserialize to.
        // Returns: The task object representing the asynchronous operation.
        public static async Task<TValue?> GetFromNewtonsoftJsonAsync<TValue>(this HttpClient client, Uri? requestUri, JsonSerializerSettings? settings, CancellationToken cancellationToken = default)
        {
            var ret = default(TValue?);
            var response = default(HttpResponseMessage);
            response = await client.GetAsync(requestUri, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var returnuserdata = await response.Content.ReadAsStringAsync(cancellationToken);
                ret = JsonConvert.DeserializeObject<TValue>(returnuserdata, settings);
            }
            return ret;
        }
        // Summary: Send a GET request to the specified Uri and return the value resulting from deserialize the response body as JSON in an asynchronous operation.
        // Parameters: client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters: TValue: The target type to deserialize to.
        // Returns: The task object representing the asynchronous operation.
        public static async Task<TValue?> GetFromNewtonsoftJsonAsync<TValue>(this HttpClient client, Uri? requestUri, CancellationToken cancellationToken = default)
        {
            var ret = default(TValue?);
            var response = default(HttpResponseMessage);
            response = await client.GetAsync(requestUri, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var returnuserdata = await response.Content.ReadAsStringAsync(cancellationToken);
                ret = JsonConvert.DeserializeObject<TValue>(returnuserdata);
            }
            return ret;
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
        public static async Task<HttpResponseMessage> PostAsNewtonsoftJsonAsync<TValue>(this HttpClient client, string? requestUri, TValue value, JsonSerializerSettings? settings = null, CancellationToken cancellationToken = default)
        {
            var payload = "";
            HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = default(HttpResponseMessage);
            response = await client.PostAsync(requestUri, content, cancellationToken);
            return response;
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
        public static async Task<HttpResponseMessage> PostAsNewtonsoftJsonAsync<TValue>(this HttpClient client, string? requestUri, TValue value, CancellationToken cancellationToken)
        {
            var payload = "";
            HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = default(HttpResponseMessage);
            response = await client.PostAsync(requestUri, content, cancellationToken);
            return response;
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
        public static async Task<HttpResponseMessage> PostAsNewtonsoftJsonAsync<TValue>(this HttpClient client, Uri? requestUri, TValue value, JsonSerializerSettings? settings = null, CancellationToken cancellationToken = default)
        {
            var payload = "";
            HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = default(HttpResponseMessage);
            response = await client.PostAsync(requestUri, content, cancellationToken);
            return response;
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
        public static async Task<HttpResponseMessage> PostAsNewtonsoftJsonAsync<TValue>(this HttpClient client, Uri? requestUri, TValue value, CancellationToken cancellationToken)
        {
            var payload = "";
            HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = default(HttpResponseMessage);
            response = await client.PostAsync(requestUri, content, cancellationToken);
            return response;
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
        public static async Task<HttpResponseMessage> PutAsNewtonsoftJsonAsync<TValue>(this HttpClient client, string? requestUri, TValue value, JsonSerializerSettings? settings = null, CancellationToken cancellationToken = default)
        {
            var payload = "";
            HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = default(HttpResponseMessage);
            response = await client.PutAsync(requestUri, content, cancellationToken);
            return response;
        }
        // Summary: Send a PUT request to the specified Uri containing the value serialized as JSON in the request body.
        // Parameters:
        //   client: The client used to send the request.
        //   requestUri: The Uri the request is sent to.
        //   value: The value to serialize.
        //   cancellationToken: A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        // Type parameters: TValue: The type of the value to serialize.
        // Returns: The task object representing the asynchronous operation.
        public static async Task<HttpResponseMessage> PutAsNewtonsoftJsonAsync<TValue>(this HttpClient client, string? requestUri, TValue value, CancellationToken cancellationToken)
        {
            var payload = "";
            HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = default(HttpResponseMessage);
            response = await client.PutAsync(requestUri, content, cancellationToken);
            return response;
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
        public static async Task<HttpResponseMessage> PutAsNewtonsoftJsonAsync<TValue>(this HttpClient client, Uri? requestUri, TValue value, JsonSerializerSettings? settings = null, CancellationToken cancellationToken = default)
        {
            var payload = "";
            HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = default(HttpResponseMessage);
            response = await client.PutAsync(requestUri, content, cancellationToken);
            return response;
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
        public static async Task<HttpResponseMessage> PutAsNewtonsoftJsonAsync<TValue>(this HttpClient client, Uri? requestUri, TValue value, CancellationToken cancellationToken)
        {
            var payload = "";
            HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = default(HttpResponseMessage);
            response = await client.PutAsync(requestUri, content, cancellationToken);
            return response;
        }
    }
}
