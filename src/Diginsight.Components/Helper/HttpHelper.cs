#region using
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using System.Runtime.CompilerServices;
#endregion

namespace Diginsight.Components;

internal static class HttpHelper
{
    private static readonly Type T = typeof(HttpHelper);
    //private ILogger<HttpHelper> logger;

    public static HttpClient GetHttpClient(string mediaType, string authToken = default)
    {
        //using (var scope = TraceLogger.BeginMethodScope(T, new { mediaType }))
        //{
        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
        if (authToken != null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        }

        //scope.Result = client;
        return client;
        //}
    }
}
