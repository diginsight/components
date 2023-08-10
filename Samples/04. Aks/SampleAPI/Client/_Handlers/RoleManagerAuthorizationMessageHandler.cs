using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;

namespace dbo.app.Handlers
{
    public class RoleManagerAuthorizationMessageHandler : BaseAuthorizationMessageHandler
    {
        public RoleManagerAuthorizationMessageHandler(IAccessTokenProvider provider,
            NavigationManager navigationManager, IConfiguration configuration)
            : base(provider, navigationManager)
        {
            ConfigureHandler(
               authorizedUrls: configuration["RoleManagerApi:AuthorizedUrls"]?.Split(","),
               scopes: configuration["RoleManagerApi:Scopes"]?.Split(",")
            );
        }
    }
}