using System;
using System.Threading.Tasks;

namespace DafornoMail.Core.Interfaces.Services;

/// <summary>
/// Service for interacting with Azure Key Vault to securely store and retrieve secrets.
/// </summary>
public interface IKeyVaultService
{
    /// <summary>
    /// Retrieves a secret from the Key Vault.
    /// </summary>
    /// <param name="secretName">The name of the secret to retrieve.</param>
    /// <returns>The secret value as a string.</returns>
    /// <exception cref="ArgumentException">Thrown when secretName is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the secret cannot be retrieved.</exception>
    Task<string> GetSecretAsync(string secretName);

    /// <summary>
    /// Stores a secret in the Key Vault.
    /// </summary>
    /// <param name="secretName">The name to give the secret.</param>
    /// <param name="secretValue">The value of the secret to store.</param>
    /// <returns>The name of the stored secret.</returns>
    /// <exception cref="ArgumentException">Thrown when secretName or secretValue is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the secret cannot be stored.</exception>
    Task<string> SetSecretAsync(string secretName, string secretValue);

    /// <summary>
    /// Deletes a secret from the Key Vault.
    /// </summary>
    /// <param name="secretName">The name of the secret to delete.</param>
    /// <exception cref="ArgumentException">Thrown when secretName is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the secret cannot be deleted.</exception>
    Task DeleteSecretAsync(string secretName);

    /// <summary>
    /// Checks if a secret exists in the Key Vault.
    /// </summary>
    /// <param name="secretName">The name of the secret to check.</param>
    /// <returns>True if the secret exists, false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown when secretName is null or empty.</exception>
    Task<bool> SecretExistsAsync(string secretName);

    /// <summary>
    /// Updates an existing secret in the Key Vault.
    /// </summary>
    /// <param name="secretName">The name of the secret to update.</param>
    /// <param name="secretValue">The new value for the secret.</param>
    /// <exception cref="ArgumentException">Thrown when secretName or secretValue is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the secret cannot be updated.</exception>
    Task UpdateSecretAsync(string secretName, string secretValue);
}
