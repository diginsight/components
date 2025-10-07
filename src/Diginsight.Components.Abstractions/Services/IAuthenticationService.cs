using System.Security.Principal;

namespace Diginsight.Components;

public interface IAuthenticationService
{
    Task<IIdentity> LoginSilentAsync();
    Task<GenericIdentity> LoginAsync();
    Task LogoutAsync();
}
