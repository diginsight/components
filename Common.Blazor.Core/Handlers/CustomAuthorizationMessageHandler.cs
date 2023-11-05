using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;

namespace Common.Core.Blazor
{
    public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public CustomAuthorizationMessageHandler(IAccessTokenProvider provider,
            NavigationManager navigationManager, IConfiguration configuration)
            : base(provider, navigationManager)
        {
            ConfigureHandler(
               authorizedUrls: configuration["authorizedUrls"].Split(","),
               scopes: new[] { "https://graph.microsoft.com/User.Read", "api://0acaf19a-be71-4605-aff3-982c02a449f1/access_as_user" });
               //scopes: new[] { "https://graph.microsoft.com/User.Read", "api://09595640-a576-48e9-9c2b-7e745d8885ab/access_as_user" });

        }
    }
}