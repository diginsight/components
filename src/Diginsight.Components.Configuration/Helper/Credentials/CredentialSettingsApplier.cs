using Azure.Core;
using Azure.Identity;

namespace Diginsight.Components.Configuration;

/// <summary>
/// Applies <see cref="CredentialSettings"/> onto the Azure Identity options objects.
/// <para>
/// Every overload takes a nullable <c>settings</c> and returns without touching <c>options</c> when it is
/// <see langword="null"/>, so an absent configuration section leaves the Azure SDK defaults in place and a
/// caller never has to null-check before applying.
/// </para>
/// <para>
/// One overload per options type is unavoidable: <c>TenantId</c> and <c>AdditionallyAllowedTenants</c> are
/// declared independently on each derived options type rather than on <see cref="TokenCredentialOptions"/>.
/// </para>
/// </summary>
public static class CredentialSettingsApplier
{
    /// <summary>
    /// Applies the members declared on <see cref="TokenCredentialOptions"/> and <see cref="ClientOptions"/>.
    /// Also the whole surface for credentials configured through a bare <see cref="TokenCredentialOptions"/>.
    /// </summary>
    /// <param name="settings">Settings to apply. When <see langword="null"/> the call is a no-op.</param>
    /// <param name="options">Options instance to mutate.</param>
    public static void Apply(this CredentialSettings? settings, TokenCredentialOptions options)
    {
        if (settings is null)
        {
            return;
        }

        if (settings.AuthorityHost is { } authorityHost)
        {
            options.AuthorityHost = authorityHost;
        }

        if (settings.IsUnsafeSupportLoggingEnabled is { } isUnsafeSupportLoggingEnabled)
        {
            options.IsUnsafeSupportLoggingEnabled = isUnsafeSupportLoggingEnabled;
        }

        settings.Retry?.ApplyTo(options.Retry);
        settings.Diagnostics?.ApplyTo(options.Diagnostics);
    }

    public static void Apply(this CredentialSettings? settings, ClientSecretCredentialOptions options)
    {
        settings.Apply((TokenCredentialOptions)options);

        if (settings is null)
        {
            return;
        }

        ReplaceAll(settings.AdditionallyAllowedTenants, options.AdditionallyAllowedTenants);

        if (settings.DisableInstanceDiscovery is { } disableInstanceDiscovery)
        {
            options.DisableInstanceDiscovery = disableInstanceDiscovery;
        }
    }

    public static void Apply(this CredentialSettings? settings, ClientCertificateCredentialOptions options)
    {
        settings.Apply((TokenCredentialOptions)options);

        if (settings is null)
        {
            return;
        }

        ReplaceAll(settings.AdditionallyAllowedTenants, options.AdditionallyAllowedTenants);

        if (settings.DisableInstanceDiscovery is { } disableInstanceDiscovery)
        {
            options.DisableInstanceDiscovery = disableInstanceDiscovery;
        }

        if (settings is ClientCertificateCredentialSettings { SendCertificateChain: { } sendCertificateChain })
        {
            options.SendCertificateChain = sendCertificateChain;
        }
    }

    public static void Apply(this CredentialSettings? settings, ClientAssertionCredentialOptions options)
    {
        settings.Apply((TokenCredentialOptions)options);

        if (settings is null)
        {
            return;
        }

        ReplaceAll(settings.AdditionallyAllowedTenants, options.AdditionallyAllowedTenants);
    }

    public static void Apply(this CredentialSettings? settings, AzureCliCredentialOptions options)
    {
        settings.Apply((TokenCredentialOptions)options);

        if (settings is null)
        {
            return;
        }

        if (settings.TenantId is { } tenantId)
        {
            options.TenantId = tenantId;
        }

        ReplaceAll(settings.AdditionallyAllowedTenants, options.AdditionallyAllowedTenants);

        if (settings is not ProcessCredentialSettings processSettings)
        {
            return;
        }

        if (processSettings.ProcessTimeout is { } processTimeout)
        {
            options.ProcessTimeout = processTimeout;
        }

        if (processSettings.Subscription is { } subscription)
        {
            options.Subscription = subscription;
        }
    }

    public static void Apply(this CredentialSettings? settings, VisualStudioCodeCredentialOptions options)
    {
        settings.Apply((TokenCredentialOptions)options);

        if (settings is null)
        {
            return;
        }

        if (settings.TenantId is { } tenantId)
        {
            options.TenantId = tenantId;
        }

        ReplaceAll(settings.AdditionallyAllowedTenants, options.AdditionallyAllowedTenants);
    }

    public static void Apply(this CredentialSettings? settings, VisualStudioCredentialOptions options)
    {
        settings.Apply((TokenCredentialOptions)options);

        if (settings is null)
        {
            return;
        }

        if (settings.TenantId is { } tenantId)
        {
            options.TenantId = tenantId;
        }

        ReplaceAll(settings.AdditionallyAllowedTenants, options.AdditionallyAllowedTenants);
    }

    public static void Apply(this CredentialSettings? settings, WorkloadIdentityCredentialOptions options)
    {
        settings.Apply((TokenCredentialOptions)options);

        if (settings is null)
        {
            return;
        }

        if (settings.TenantId is { } tenantId)
        {
            options.TenantId = tenantId;
        }

        ReplaceAll(settings.AdditionallyAllowedTenants, options.AdditionallyAllowedTenants);

        if (settings.DisableInstanceDiscovery is { } disableInstanceDiscovery)
        {
            options.DisableInstanceDiscovery = disableInstanceDiscovery;
        }

        if (settings is not WorkloadIdentityCredentialSettings workloadSettings)
        {
            return;
        }

        if (workloadSettings.ClientId is { } clientId)
        {
            options.ClientId = clientId;
        }

        if (workloadSettings.TokenFilePath is { } tokenFilePath)
        {
            options.TokenFilePath = tokenFilePath;
        }
    }

    private static void ApplyTo(this CredentialRetrySettings settings, RetryOptions options)
    {
        if (settings.MaxRetries is { } maxRetries)
        {
            options.MaxRetries = maxRetries;
        }

        if (settings.Delay is { } delay)
        {
            options.Delay = delay;
        }

        if (settings.MaxDelay is { } maxDelay)
        {
            options.MaxDelay = maxDelay;
        }

        if (settings.Mode is { } mode)
        {
            options.Mode = mode;
        }

        if (settings.NetworkTimeout is { } networkTimeout)
        {
            options.NetworkTimeout = networkTimeout;
        }
    }

    private static void ApplyTo(this CredentialDiagnosticsSettings settings, TokenCredentialDiagnosticsOptions options)
    {
        if (settings.ApplicationId is { } applicationId)
        {
            options.ApplicationId = applicationId;
        }

        if (settings.IsLoggingEnabled is { } isLoggingEnabled)
        {
            options.IsLoggingEnabled = isLoggingEnabled;
        }

        if (settings.IsLoggingContentEnabled is { } isLoggingContentEnabled)
        {
            options.IsLoggingContentEnabled = isLoggingContentEnabled;
        }

        if (settings.IsTelemetryEnabled is { } isTelemetryEnabled)
        {
            options.IsTelemetryEnabled = isTelemetryEnabled;
        }

        if (settings.IsDistributedTracingEnabled is { } isDistributedTracingEnabled)
        {
            options.IsDistributedTracingEnabled = isDistributedTracingEnabled;
        }

        if (settings.IsAccountIdentifierLoggingEnabled is { } isAccountIdentifierLoggingEnabled)
        {
            options.IsAccountIdentifierLoggingEnabled = isAccountIdentifierLoggingEnabled;
        }

        if (settings.LoggedContentSizeLimit is { } loggedContentSizeLimit)
        {
            options.LoggedContentSizeLimit = loggedContentSizeLimit;
        }
    }

    // AdditionallyAllowedTenants is get-only on the Azure options types, so a configured list replaces
    // the contents rather than the instance.
    private static void ReplaceAll(IList<string>? source, IList<string> target)
    {
        if (source is null)
        {
            return;
        }

        target.Clear();
        foreach (string item in source)
        {
            target.Add(item);
        }
    }
}
