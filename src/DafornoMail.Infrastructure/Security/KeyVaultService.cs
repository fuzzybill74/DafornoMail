// DafornoMail.Infrastructure/Security/KeyVaultService.cs
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace DafornoMail.Infrastructure.Security;

public class KeyVaultService
{
    private readonly SecretClient _secretClient;
    private readonly IConfiguration _configuration;

    public KeyVaultService(IConfiguration configuration)
    {
        _configuration = configuration;
        var keyVaultUri = _configuration["KeyVault:VaultUri"]
            ?? throw new InvalidOperationException("KeyVault:VaultUri is not configured");

        _secretClient = new SecretClient(
            new Uri(keyVaultUri),
            new DefaultAzureCredential());
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        try
        {
            var secret = await _secretClient.GetSecretAsync(secretName);
            return secret.Value.Value;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve secret {secretName} from Key Vault", ex);
        }
    }

    public async Task<string> SetSecretAsync(string secretName, string secretValue)
    {
        try
        {
            var secret = await _secretClient.SetSecretAsync(secretName, secretValue);
            return secret.Value.Name;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to store secret {secretName} in Key Vault", ex);
        }
    }

    public async Task DeleteSecretAsync(string secretName)
    {
        try
        {
            await _secretClient.StartDeleteSecretAsync(secretName);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete secret {secretName} from Key Vault", ex);
        }
    }
}