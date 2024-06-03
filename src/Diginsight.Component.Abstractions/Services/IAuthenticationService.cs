using System;
using Common;
using System.Threading.Tasks;
using System.Security.Principal;

namespace Common
{
    public interface IAuthenticationService
    {
        Task<IIdentity> LoginSilentAsync();
        Task<GenericIdentity> LoginAsync();
        Task LogoutAsync();
    }
}
