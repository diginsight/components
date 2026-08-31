using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Diginsight.Components.Configuration.Tests;

public sealed class CredentialSettingsApplierTests
{
    [Fact]
    public void Absent_settings_leave_the_azure_sdk_defaults_untouched()
    {
        AzureCliCredentialOptions expected = new();
        AzureCliCredentialOptions actual = new();

        CredentialChainSettings chain = Bind([]);
        chain.Common.Apply(actual);
        chain.AzureCli.Apply(actual);

        Assert.Null(actual.ProcessTimeout);
        Assert.Equal(expected.ProcessTimeout, actual.ProcessTimeout);
        Assert.Equal(expected.Subscription, actual.Subscription);
        Assert.Equal(expected.TenantId, actual.TenantId);
        Assert.Equal(expected.Retry.MaxRetries, actual.Retry.MaxRetries);
        Assert.Equal(expected.AuthorityHost, actual.AuthorityHost);
        Assert.Empty(actual.AdditionallyAllowedTenants);
    }

    [Fact]
    public void Common_settings_reach_every_credential()
    {
        CredentialChainSettings chain = Bind(
            new Dictionary<string, string?>
            {
                ["Credential:Common:Retry:MaxRetries"] = "7",
                ["Credential:Common:Diagnostics:IsLoggingEnabled"] = "false",
                ["Credential:Common:IsUnsafeSupportLoggingEnabled"] = "true",
            }
        );

        AzureCliCredentialOptions cliOptions = new();
        chain.Common.Apply(cliOptions);

        WorkloadIdentityCredentialOptions workloadOptions = new();
        chain.Common.Apply(workloadOptions);

        Assert.Equal(7, cliOptions.Retry.MaxRetries);
        Assert.False(cliOptions.Diagnostics.IsLoggingEnabled);
        Assert.True(cliOptions.IsUnsafeSupportLoggingEnabled);
        Assert.Equal(7, workloadOptions.Retry.MaxRetries);
    }

    [Fact]
    public void A_named_section_overrides_common()
    {
        CredentialChainSettings chain = Bind(
            new Dictionary<string, string?>
            {
                ["Credential:Common:Retry:MaxRetries"] = "7",
                ["Credential:AzureCli:Retry:MaxRetries"] = "2",
            }
        );

        AzureCliCredentialOptions cliOptions = new();
        chain.Common.Apply(cliOptions);
        chain.AzureCli.Apply(cliOptions);

        WorkloadIdentityCredentialOptions workloadOptions = new();
        chain.Common.Apply(workloadOptions);
        chain.WorkloadIdentity.Apply(workloadOptions);

        Assert.Equal(2, cliOptions.Retry.MaxRetries);
        Assert.Equal(7, workloadOptions.Retry.MaxRetries);
    }

    [Fact]
    public void Azure_cli_process_settings_reach_the_options()
    {
        CredentialChainSettings chain = Bind(
            new Dictionary<string, string?>
            {
                ["Credential:AzureCli:ProcessTimeout"] = "00:05:00",
                ["Credential:AzureCli:Subscription"] = "contoso-subscription",
                ["Credential:AzureCli:TenantId"] = "11111111-1111-1111-1111-111111111111",
            }
        );

        AzureCliCredentialOptions cliOptions = new();
        chain.Common.Apply(cliOptions);
        chain.AzureCli.Apply(cliOptions);

        Assert.Equal(TimeSpan.FromMinutes(5), cliOptions.ProcessTimeout);
        Assert.Equal("contoso-subscription", cliOptions.Subscription);
        Assert.Equal("11111111-1111-1111-1111-111111111111", cliOptions.TenantId);
    }

    [Fact]
    public void Additionally_allowed_tenants_replaces_rather_than_appends()
    {
        CredentialChainSettings chain = Bind(
            new Dictionary<string, string?>
            {
                ["Credential:Common:AdditionallyAllowedTenants:0"] = "aaaa",
                ["Credential:AzureCli:AdditionallyAllowedTenants:0"] = "*",
            }
        );

        AzureCliCredentialOptions cliOptions = new();
        chain.Common.Apply(cliOptions);
        chain.AzureCli.Apply(cliOptions);

        Assert.Equal(["*"], cliOptions.AdditionallyAllowedTenants);
    }

    [Fact]
    public void Workload_identity_specific_settings_reach_the_options()
    {
        CredentialChainSettings chain = Bind(
            new Dictionary<string, string?>
            {
                ["Credential:WorkloadIdentity:ClientId"] = "22222222-2222-2222-2222-222222222222",
                ["Credential:WorkloadIdentity:TokenFilePath"] = "/var/run/secrets/token",
                ["Credential:WorkloadIdentity:DisableInstanceDiscovery"] = "true",
            }
        );

        WorkloadIdentityCredentialOptions workloadOptions = new();
        chain.Common.Apply(workloadOptions);
        chain.WorkloadIdentity.Apply(workloadOptions);

        Assert.Equal("22222222-2222-2222-2222-222222222222", workloadOptions.ClientId);
        Assert.Equal("/var/run/secrets/token", workloadOptions.TokenFilePath);
        Assert.True(workloadOptions.DisableInstanceDiscovery);
    }

    [Fact]
    public void Retry_mode_and_delays_are_bound()
    {
        CredentialChainSettings chain = Bind(
            new Dictionary<string, string?>
            {
                ["Credential:Common:Retry:Mode"] = "Fixed",
                ["Credential:Common:Retry:Delay"] = "00:00:03",
                ["Credential:Common:Retry:MaxDelay"] = "00:00:20",
                ["Credential:Common:Retry:NetworkTimeout"] = "00:01:40",
            }
        );

        AzureCliCredentialOptions cliOptions = new();
        chain.Common.Apply(cliOptions);

        Assert.Equal(RetryMode.Fixed, cliOptions.Retry.Mode);
        Assert.Equal(TimeSpan.FromSeconds(3), cliOptions.Retry.Delay);
        Assert.Equal(TimeSpan.FromSeconds(20), cliOptions.Retry.MaxDelay);
        Assert.Equal(TimeSpan.FromSeconds(100), cliOptions.Retry.NetworkTimeout);
    }

    private static CredentialChainSettings Bind(Dictionary<string, string?> values)
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();

        return configuration.GetSection(CredentialChainSettings.SectionName).Get<CredentialChainSettings>()
            ?? new CredentialChainSettings();
    }
}
