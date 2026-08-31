using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;

namespace Diginsight.Components.Configuration;

/// <summary>
/// Retries <c>GetToken</c> on a wrapped credential.
/// <para>
/// The Azure Core <c>Retry</c> options govern the credential's HTTP pipeline only, so they cannot recover
/// from a failure raised before any HTTP call — a timed-out Azure CLI subprocess, for instance. This
/// decorator sits around token acquisition itself, which is the layer that can.
/// </para>
/// </summary>
public sealed class RetryingTokenCredential : TokenCredential
{
    private readonly TokenCredential inner;
    private readonly int maxAttempts;
    private readonly TimeSpan delay;
    private readonly TimeSpan maxDelay;
    private readonly RetryMode mode;
    private readonly ILogger logger;

    private RetryingTokenCredential(TokenCredential inner, GetTokenRetrySettings settings, int maxAttempts, ILogger logger)
    {
        this.inner = inner;
        this.maxAttempts = maxAttempts;
        this.logger = logger;

        delay = settings.Delay ?? TimeSpan.FromSeconds(1);
        maxDelay = settings.MaxDelay ?? TimeSpan.FromSeconds(30);
        mode = settings.Mode ?? RetryMode.Exponential;
    }

    /// <summary>
    /// Wraps <paramref name="inner"/> when retries are configured, and returns it untouched otherwise, so the
    /// decorator is invisible unless a caller asks for it.
    /// </summary>
    public static TokenCredential Create(TokenCredential inner, GetTokenRetrySettings? settings, ILogger logger)
    {
        if (settings?.MaxAttempts is not { } maxAttempts || maxAttempts <= 1)
        {
            return inner;
        }

        return new RetryingTokenCredential(inner, settings, maxAttempts, logger);
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return inner.GetToken(requestContext, cancellationToken);
            }
            catch (AuthenticationFailedException exception) when (ShouldRetry(exception, attempt))
            {
                TimeSpan currentDelay = GetDelay(attempt);
                LogRetry(exception, attempt, currentDelay);
                cancellationToken.WaitHandle.WaitOne(currentDelay);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }

    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return await inner.GetTokenAsync(requestContext, cancellationToken).ConfigureAwait(false);
            }
            catch (AuthenticationFailedException exception) when (ShouldRetry(exception, attempt))
            {
                TimeSpan currentDelay = GetDelay(attempt);
                LogRetry(exception, attempt, currentDelay);
                await Task.Delay(currentDelay, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    // CredentialUnavailableException derives from AuthenticationFailedException, and it is how a credential
    // tells ChainedTokenCredential to move on. Retrying it would stall the chain.
    private bool ShouldRetry(AuthenticationFailedException exception, int attempt) =>
        exception is not CredentialUnavailableException && attempt < maxAttempts;

    private TimeSpan GetDelay(int attempt)
    {
        if (mode == RetryMode.Fixed)
        {
            return delay;
        }

        double factor = Math.Pow(2, attempt - 1);
        double milliseconds = delay.TotalMilliseconds * factor;

        return milliseconds >= maxDelay.TotalMilliseconds
            ? maxDelay
            : TimeSpan.FromMilliseconds(milliseconds);
    }

    private void LogRetry(AuthenticationFailedException exception, int attempt, TimeSpan currentDelay)
    {
        logger.LogWarning(
            exception,
            "Token acquisition attempt {Attempt} of {MaxAttempts} failed for {Credential}; retrying in {Delay}",
            attempt,
            maxAttempts,
            inner.GetType().Name,
            currentDelay);
    }
}
