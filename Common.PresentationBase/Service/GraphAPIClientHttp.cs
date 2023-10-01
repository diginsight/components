using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ExternalConnectors;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.SmartCache;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using System.Configuration;
using Application = Microsoft.Graph.Models.Application;

namespace Common.PresentationBase
{
    public class GraphAPIClientHttp : IGraphAPIClientHttp
    {
        private ILogger<GraphAPIClientHttp> logger;
        private readonly ICacheService cacheService;

        public GraphAPIClientHttp(ILogger<GraphAPIClientHttp> logger, ICacheService cacheService)
        {
            this.logger = logger;
            this.cacheService = cacheService;
        }

        public async Task<IEnumerable<Application>> GetUserApplicationsAsync(Identity identity, string tenantId, Guid clientId, CacheContext cacheContext) // pageSize, $count
        {
            //using var scope = logger.BeginMethodScope(new { identity = identity.GetLogString(), tenantId, clientId, cacheContext = cacheContext.GetLogString() }, properties: PROPS.Get(new[] { ("Tags", "Event,Call" as object),  ("User", ApplicationBase.GetUser()), ("MaxMessageLen", 0) }));
            using var scope = logger.BeginMethodScope(new { upn = identity.Upn, tenantId, clientId, maxAge = cacheContext.MaxAge }, properties: PROPS.Get(new[] { ("Tags", "Event,Call" as object), ("User", ApplicationBase.GetUser()), ("MaxMessageLen", 0) }));

            EnsureCacheContext<IGraphAPIClientHttp>(ref cacheContext);

            var cacheKey = new GetUserApplicationsAsyncKey(identity.Upn, tenantId, clientId);

            var result = await cacheService.GetAsync(
                cacheKey, async () =>
                {
                    var applications = await GetUserApplicationsImplAsync(identity, tenantId, clientId);
                    return applications;
                },
                cacheContext);

            return result;
        }

        public async Task<IEnumerable<Application>> GetUserApplicationsImplAsync(Identity identity, string tenantId, Guid clientId)
        {
            using var scope = logger.BeginMethodScope(new { identity = identity.GetLogString(), tenantId, clientId });
            
            var credentialOptions1 = new DefaultAzureCredentialOptions { SharedTokenCacheUsername = identity.Upn, ExcludeInteractiveBrowserCredential = false, ExcludeSharedTokenCacheCredential = false, ExcludeAzureCliCredential = false, ExcludeEnvironmentCredential = true, ExcludeManagedIdentityCredential = true, ExcludeVisualStudioCodeCredential = true, ExcludeVisualStudioCredential = true };
            credentialOptions1.TenantId = tenantId;
            var credential = new DefaultAzureCredential(credentialOptions1);

            var pageSize = 30;
            var applications = new List<Application>();
            var responseAppsContent = default(string);
            try
            {
                var requestUri = $"https://graph.microsoft.com/v1.0/me/ownedObjects/microsoft.graph.application?$top={pageSize}&$count=true";

                for (var page = 0; ; page++)
                {
                    using var scopeLoop = logger.BeginNamedScope("Loop", new { identity = identity.GetLogString(), tenantId, clientId, page });

                    if (page > 10) { scope.LogDebug($"Page limit reached: {page - 1}"); break; }
                    if (page > 0 && string.IsNullOrEmpty(requestUri)) { scope.LogDebug($"Enumeration completed: {page - 1}"); break; }

                    var token = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { "https://graph.microsoft.com/.default" }), new CancellationToken());
                    using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                    {
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                        request.Headers.Add("ConsistencyLevel", "eventual");

                        using (var clientApps = new HttpClient())
                        using (var responseApps = await clientApps.SendAsync(request))
                        {
                            responseApps.EnsureSuccessStatusCode();

                            responseAppsContent = await responseApps.Content.ReadAsStringAsync();
                            var responseAppsOjbect = JObject.Parse(responseAppsContent);
                            var nextLink = responseAppsOjbect["@odata.nextLink"]?.Value<string>();
                            requestUri = nextLink;
                            scope.LogDebug(new { page, nextLink });

                            var applicationList = responseAppsOjbect["value"];
                            foreach (var item in applicationList)
                            {
                                var id = item["id"]?.Value<string>();
                                var appId = item["appId"]?.Value<string>();
                                var displayName = item["displayName"]?.Value<string>();
                                var description = item["description"]?.Value<string>();
                                var createdDateTime = item["createdDateTime"]?.Value<DateTime?>();
                                var signInAudience = item["signInAudience"]?.Value<string>();
                                var identifierUris = item["identifierUris"]?.Select(uri => uri?.Value<string>()).ToList();
                                var publisherDomain = item["publisherDomain"]?.Value<string>();

                                var applicationItem = new Application()
                                {
                                    Id = id,
                                    AppId = appId,
                                    DisplayName = displayName,
                                    Description = description,
                                    CreatedDateTime = createdDateTime,
                                    SignInAudience = signInAudience,
                                    PublisherDomain = publisherDomain,
                                    IdentifierUris = identifierUris
                                };
                                scope.LogDebug(new { applicationItem = applicationItem.GetLogString() });
                                applications.Add(applicationItem);
                            }
                        }
                    }
                }

                //ApplicationCollectionResponse response;
                //for (var page = 0; ; page++)
                //{
                //    var appRegistrations = response = await client.Applications.GetAsync(requestConfiguration =>
                //    {
                //        requestConfiguration.QueryParameters.Count = true;
                //        requestConfiguration.QueryParameters.Top = pageSize;
                //        if (page > 0) { requestConfiguration.QueryParameters.Skip = page * pageSize; }
                //        requestConfiguration.QueryParameters.Select = new string[] { "AppId", "Id", "DisplayName", "Owners" };
                //        //requestConfiguration.QueryParameters.Filter = $"appOwnerOrganizationId eq '{tenantId}'";  // Filter = "Owners/any()"; "Owners ne null"
                //        // requestConfiguration.Headers.Add("Prefer", "outlook.body-content-type=\"text\""); 		+		Owners	null	System.Collections.Generic.List<DirectoryObject>
                //    }); 

                //    var applicationsPage = appRegistrations.Value;
                //    applications.AddRange(applicationsPage);
                //}
                return applications;
            }
            catch (Exception ex) { scope.LogException(ex); }

            return applications;
        }

        protected static void EnsureCacheContext<T>(ref CacheContext cacheContext)
        {
            cacheContext ??= new CacheContext();
            cacheContext.InterfaceType = typeof(T);
        }

        private sealed record GetUserApplicationsAsyncKey(string upn, string tenantId, Guid clientId) : ICacheKey //, IInvalidatable
        {
            //public bool IsInvalidatedBy(IInvalidationRule r, out Func<Task>? ic)
            //{
            //    ic = null;
            //    return r is GroupInvalidationRule gir
            //        && gir.OrganizationId == OrganizationId
            //        && gir.SiteId == SiteId
            //        && gir.GroupId == GroupId;
            //}

            public string ToLogString() => ToString();
        }
    }
}
