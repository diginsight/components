using Azure.Core;
using Microsoft.Extensions.Configuration;

namespace Diginsight.Components.Configuration;

public interface ICredentialProvider
{
    TokenCredential Get(IConfiguration configuration);
}



