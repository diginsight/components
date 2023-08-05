using System;
using Common;
using System.Threading.Tasks;

namespace Common.Abstractions
{
    public interface IAuthenticationService
    {
        Task<Identity> LoginSilentAsync();
        Task<Identity> LoginAsync();
        Task LogoutAsync();
    }
}
