using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Diginsight.Components.Configuration.Tests;

public sealed class DefaultCredentialProviderTests
{
    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public void An_empty_configuration_still_produces_a_chain(string environmentName)
    {
        TokenCredential credential = Provider(environmentName).Get(new ConfigurationBuilder().Build());

        Assert.IsType<ChainedTokenCredential>(credential);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public void An_empty_credential_section_still_produces_a_chain(string environmentName)
    {
        IConfiguration configuration = Configuration(
            new Dictionary<string, string?>
            {
                ["Credential:Common:Retry:MaxRetries"] = null,
            }
        );

        Assert.IsType<ChainedTokenCredential>(Provider(environmentName).Get(configuration));
    }

    [Fact]
    public void An_unparsable_legacy_process_timeout_is_ignored()
    {
        IConfiguration configuration = Configuration(
            new Dictionary<string, string?>
            {
                ["ProcessTimeout"] = "not-a-timespan",
            }
        );

        Assert.IsType<ChainedTokenCredential>(Provider("Development").Get(configuration));
    }

    [Fact]
    public void Null_settings_leave_every_options_type_untouched()
    {
        CredentialSettings? settings = null;

        AzureCliCredentialOptions cliOptions = new();
        ClientSecretCredentialOptions clientSecretOptions = new();
        ClientCertificateCredentialOptions clientCertificateOptions = new();
        ClientAssertionCredentialOptions clientAssertionOptions = new();
        VisualStudioCredentialOptions visualStudioOptions = new();
        VisualStudioCodeCredentialOptions visualStudioCodeOptions = new();
        WorkloadIdentityCredentialOptions workloadIdentityOptions = new();
        TokenCredentialOptions managedIdentityOptions = new();

        settings.Apply(cliOptions);
        settings.Apply(clientSecretOptions);
        settings.Apply(clientCertificateOptions);
        settings.Apply(clientAssertionOptions);
        settings.Apply(visualStudioOptions);
        settings.Apply(visualStudioCodeOptions);
        settings.Apply(workloadIdentityOptions);
        settings.Apply(managedIdentityOptions);

        AzureCliCredentialOptions untouched = new();
        Assert.Null(cliOptions.ProcessTimeout);
        Assert.Equal(untouched.Retry.MaxRetries, cliOptions.Retry.MaxRetries);
        Assert.Equal(untouched.AuthorityHost, cliOptions.AuthorityHost);
        Assert.Empty(workloadIdentityOptions.AdditionallyAllowedTenants);
    }

    private static DefaultCredentialProvider Provider(string environmentName)
    {
        IHostEnvironment environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(environmentName);

        return new DefaultCredentialProvider(environment, NullLogger.Instance);
    }

    private static IConfiguration Configuration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();
}
