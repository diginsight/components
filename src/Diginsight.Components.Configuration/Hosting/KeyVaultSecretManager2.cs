using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using System.Text;

namespace Diginsight.Components.Configuration;

internal sealed class KeyVaultSecretManager2 : KeyVaultSecretManager
{
    private readonly DateTimeOffset now;
    private readonly Func<IDictionary<string, string>, bool>? tagsMatch;

    public KeyVaultSecretManager2(DateTimeOffset now, Func<IDictionary<string, string>, bool>? tagsMatch)
    {
        this.now = now;
        this.tagsMatch = tagsMatch;
    }

    public override string GetKey(KeyVaultSecret secret)
    {
        StringBuilder sb = new();
        ReadOnlySpan<char> name = secret.Name;

        bool lastWasDash = false;
        while (!name.IsEmpty)
        {
            char input = name[0];
            name = name[1..];

            char? maybeOutput = input switch
            {
                '-' => ParseDash(ref name),
                (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or (>= '0' and <= '9') => input,
                _ => throw new ArgumentException("Unexpected character in secret name"),
            };

            if (maybeOutput is not { } output)
                continue;

            if (output == ':')
            {
                if (lastWasDash)
                {
                    return sb.ToString()[..^1];
                }
                else
                {
                    lastWasDash = true;
                }
            }
            else
            {
                lastWasDash = false;
            }

            sb.Append(output);
        }

        string key = sb.ToString();
        return lastWasDash ? key[..^1] : key;

        static char? ParseDash(ref ReadOnlySpan<char> name)
        {
            if (name.IsEmpty)
                return null;

            char c = name[0];
            name = name[1..];
            return c switch
            {
                '-' => ':',
                'x' or 'X' => ParseHex(ref name, 2),
                'u' or 'U' => ParseHex(ref name, 4),
                _ => null,
            };
        }

        static char? ParseHex(ref ReadOnlySpan<char> name, int length)
        {
            if (name.Length < length)
            {
                name = default;
                return null;
            }

            string str = new string(name[..length]);
            name = name[length..];

            try
            {
                return (char)Convert.ToUInt16(str, 16);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }

    public override bool Load(SecretProperties secret)
    {
        //var logger = Program.DeferredLoggerFactory.CreateLogger(T);
        //using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { secret });

        return secret.Enabled != false
            && !(secret.NotBefore > now)
            && !(secret.ExpiresOn < now)
            && (tagsMatch?.Invoke(secret.Tags) ?? true);
    }
}


