using Azure.Core;
using Azure.ResourceManager.Resources;
using Common.SmartCache;
using System.Threading.Tasks;

namespace Common.PresentationBase
{
    public interface IArmAPIClient
    {
        void Attach(TokenCredential credential);
        Task<TenantCollection> GetTenantsAsync(CacheContext cacheContext);
    }
}