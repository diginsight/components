using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Diginsight.Components.Configuration.Tests;

public sealed class RetryingTokenCredentialTests
{
    [Fact]
    public void No_settings_returns_the_inner_credential_unwrapped()
    {
        CountingTokenCredential inner = new(_ => null);

        Assert.Same(inner, RetryingTokenCredential.Create(inner, null, NullLogger.Instance));
        Assert.Same(inner, RetryingTokenCredential.Create(inner, new GetTokenRetrySettings(), NullLogger.Instance));
        Assert.Same(inner, RetryingTokenCredential.Create(inner, new GetTokenRetrySettings { MaxAttempts = 1 }, NullLogger.Instance));
    }

    [Fact]
    public async Task An_unavailable_credential_is_not_retried()
    {
        CountingTokenCredential inner = new(_ => new CredentialUnavailableException("unavailable"));
        TokenCredential credential = Wrap(inner, maxAttempts: 4);

        await Assert.ThrowsAsync<CredentialUnavailableException>(
            async () => await credential.GetTokenAsync(Context, CancellationToken.None)
        );

        Assert.Equal(1, inner.Attempts);
    }

    [Fact]
    public async Task A_failing_credential_is_retried_up_to_the_attempt_limit()
    {
        CountingTokenCredential inner = new(_ => new AuthenticationFailedException("Azure CLI authentication timed out."));
        TokenCredential credential = Wrap(inner, maxAttempts: 3);

        await Assert.ThrowsAsync<AuthenticationFailedException>(
            async () => await credential.GetTokenAsync(Context, CancellationToken.None)
        );

        Assert.Equal(3, inner.Attempts);
    }

    [Fact]
    public async Task A_later_attempt_can_succeed()
    {
        CountingTokenCredential inner = new(attempt => attempt < 3 ? new AuthenticationFailedException("timed out") : null);
        TokenCredential credential = Wrap(inner, maxAttempts: 4);

        AccessToken token = await credential.GetTokenAsync(Context, CancellationToken.None);

        Assert.Equal(CountingTokenCredential.TokenValue, token.Token);
        Assert.Equal(3, inner.Attempts);
    }

    [Fact]
    public void The_synchronous_path_retries_the_same_way()
    {
        CountingTokenCredential inner = new(attempt => attempt < 2 ? new AuthenticationFailedException("timed out") : null);
        TokenCredential credential = Wrap(inner, maxAttempts: 3);

        AccessToken token = credential.GetToken(Context, CancellationToken.None);

        Assert.Equal(CountingTokenCredential.TokenValue, token.Token);
        Assert.Equal(2, inner.Attempts);
    }

    private static TokenRequestContext Context => new(["https://storage.azure.com/.default"]);

    private static TokenCredential Wrap(TokenCredential inner, int maxAttempts) =>
        RetryingTokenCredential.Create(
            inner,
            new GetTokenRetrySettings
            {
                MaxAttempts = maxAttempts,
                Mode = RetryMode.Fixed,
                Delay = TimeSpan.FromMilliseconds(1),
                MaxDelay = TimeSpan.FromMilliseconds(1),
            },
            NullLogger.Instance
        );

    private sealed class CountingTokenCredential : TokenCredential
    {
        public const string TokenValue = "token";

        private readonly Func<int, Exception?> outcome;

        public CountingTokenCredential(Func<int, Exception?> outcome)
        {
            this.outcome = outcome;
        }

        public int Attempts { get; private set; }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            Attempts++;

            return outcome(Attempts) is { } exception
                ? throw exception
                : new AccessToken(TokenValue, DateTimeOffset.UtcNow.AddHours(1));
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken) =>
            new(GetToken(requestContext, cancellationToken));
    }
}
