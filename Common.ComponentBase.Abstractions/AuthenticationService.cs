using System;
using Common;
using System.Threading.Tasks;

namespace Common.Abstractions
{
    public interface AuthenticationService
    {
        Task<Identity> LoginSilentAsync();
        Task<Identity> LoginAsync();
        Task<Identity> LogoutAsync();
    }
}
