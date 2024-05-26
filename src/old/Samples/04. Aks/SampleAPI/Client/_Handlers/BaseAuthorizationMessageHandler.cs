using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;

namespace dbo.app.Handlers
{
    public class BaseAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public BaseAuthorizationMessageHandler(
            IAccessTokenProvider provider,
            NavigationManager navigation) : base(provider, navigation)
        {
        }

        public BaseAuthorizationMessageHandler(IAccessTokenProvider provider,
            NavigationManager navigationManager, IConfiguration configuration)
            : base(provider, navigationManager)
        {
        }
    }
}