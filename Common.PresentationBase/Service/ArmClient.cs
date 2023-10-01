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
using Azure.ResourceManager.Resources;
using Azure.Core;
using Azure.ResourceManager;

namespace Common.PresentationBase
{
    public class ArmAPIClient : IArmAPIClient
    {
        private ILogger<ArmAPIClient> logger;
        private readonly ICacheService cacheService;
        private TokenCredential credential;

        public ArmAPIClient(ILogger<ArmAPIClient> logger, ICacheService cacheService)
        {
            this.logger = logger;
            this.cacheService = cacheService;
        }

        public void Attach(TokenCredential credential)
        {
            this.credential = credential;
        }

        public async Task<TenantCollection> GetTenantsAsync(CacheContext cacheContext) // pageSize, $count
        {
            using var scope = logger.BeginMethodScope(new { cacheContext = cacheContext.GetLogString() }, properties: PROPS.Get(new[] { ("Tags", "Event,Call" as object), ("Identity", ApplicationBase.Current?.Properties?["Identity"]), ("User", ApplicationBase.GetUser()), ("MaxMessageLen", 0) }));

            EnsureCacheContext<IArmAPIClient>(ref cacheContext);

            if (credential == null)
            {
                var identity = ApplicationBase.Current.Properties["Identity"] as Identity;
                var credentialOptions = new DefaultAzureCredentialOptions { SharedTokenCacheUsername = identity.Upn, ExcludeInteractiveBrowserCredential = false, ExcludeSharedTokenCacheCredential = false, ExcludeAzureCliCredential = false, ExcludeEnvironmentCredential = true, ExcludeManagedIdentityCredential = true, ExcludeVisualStudioCodeCredential = true, ExcludeVisualStudioCredential = true };
                credential = new DefaultAzureCredential(credentialOptions);
            }

            var cacheKey = new GetTenantsAsyncKey();

            var result = await cacheService.GetAsync(
                cacheKey, async () =>
                {
                    var armClient = new ArmClient(credential);
                    var tenantObjects = armClient.GetTenants();
                    return tenantObjects;
                },
                cacheContext);

            scope.Result = result;
            return result;
        }

        protected static void EnsureCacheContext<T>(ref CacheContext cacheContext)
        {
            cacheContext ??= new CacheContext();
            cacheContext.InterfaceType = typeof(T);
        }

        private sealed record GetTenantsAsyncKey() : ICacheKey //, IInvalidatable
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
