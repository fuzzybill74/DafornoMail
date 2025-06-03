using System.Net.Http.Json;
using DafornoMail.Client.Models;

namespace DafornoMail.Client.Services;

public interface IAccountService
{
    Task<UserProfile> GetUserProfileAsync();
    Task<bool> UpdateProfileAsync(UpdateProfileModel model);
    Task<bool> ChangePasswordAsync(ChangePasswordModel model);
    Task<IEnumerable<ConnectedAccount>> GetConnectedAccountsAsync();
    Task<bool> AddAccountAsync(AddAccountModel model);
    Task<bool> UpdateAccountAsync(string accountId, UpdateAccountModel model);
    Task<bool> RemoveAccountAsync(string accountId);
    Task<bool> SetDefaultAccountAsync(string accountId);
    Task<AccountSettings> GetAccountSettingsAsync();
    Task<bool> UpdateAccountSettingsAsync(AccountSettings settings);
    Task<IEnumerable<EmailSignature>> GetEmailSignaturesAsync();
    Task<EmailSignature> GetEmailSignatureAsync(string id);
    Task<EmailSignature> CreateEmailSignatureAsync(EmailSignature signature);
    Task<bool> UpdateEmailSignatureAsync(EmailSignature signature);
    Task<bool> DeleteEmailSignatureAsync(string id);
}

public class AccountService : IAccountService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccountService> _logger;

    public AccountService(HttpClient httpClient, ILogger<AccountService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserProfile> GetUserProfileAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<UserProfile>("api/account/profile") 
                   ?? throw new Exception("Failed to load user profile");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user profile");
            throw;
        }
    }

    public async Task<bool> UpdateProfileAsync(UpdateProfileModel model)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync("api/account/profile", model);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordModel model)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/change-password", model);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return false;
        }
    }

    public async Task<IEnumerable<ConnectedAccount>> GetConnectedAccountsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ConnectedAccount>>("api/account/accounts") 
                   ?? new List<ConnectedAccount>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching connected accounts");
            return new List<ConnectedAccount>();
        }
    }

    public async Task<bool> AddAccountAsync(AddAccountModel model)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/accounts", model);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding account");
            return false;
        }
    }

    public async Task<bool> UpdateAccountAsync(string accountId, UpdateAccountModel model)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/account/accounts/{accountId}", model);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating account {accountId}");
            return false;
        }
    }

    public async Task<bool> RemoveAccountAsync(string accountId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/account/accounts/{accountId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing account {accountId}");
            return false;
        }
    }

    public async Task<bool> SetDefaultAccountAsync(string accountId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/account/accounts/{accountId}/set-default", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting default account {accountId}");
            return false;
        }
    }

    public async Task<AccountSettings> GetAccountSettingsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AccountSettings>("api/account/settings") 
                   ?? new AccountSettings();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching account settings");
            return new AccountSettings();
        }
    }


    public async Task<bool> UpdateAccountSettingsAsync(AccountSettings settings)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync("api/account/settings", settings);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account settings");
            return false;
        }
    }


    public async Task<IEnumerable<EmailSignature>> GetEmailSignaturesAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<EmailSignature>>("api/account/signatures") 
                   ?? new List<EmailSignature>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching email signatures");
            return new List<EmailSignature>();
        }
    }

    public async Task<EmailSignature> GetEmailSignatureAsync(string id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<EmailSignature>($"api/account/signatures/{id}") 
                   ?? throw new Exception("Signature not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching email signature {id}");
            throw;
        }
    }

    public async Task<EmailSignature> CreateEmailSignatureAsync(EmailSignature signature)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/signatures", signature);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to create signature");
            }
            return await response.Content.ReadFromJsonAsync<EmailSignature>() ?? throw new Exception("Invalid response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating email signature");
            throw;
        }
    }

    public async Task<bool> UpdateEmailSignatureAsync(EmailSignature signature)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/account/signatures/{signature.Id}", signature);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating email signature {signature.Id}");
            return false;
        }
    }

    public async Task<bool> DeleteEmailSignatureAsync(string id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/account/signatures/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting email signature {id}");
            return false;
        }
    }
}

public class UserProfile
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? TimeZone { get; set; } = "UTC";
    public string? Language { get; set; } = "en-US";
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class UpdateProfileModel
{
    public string? DisplayName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? TimeZone { get; set; }
    public string? Language { get; set; }
}

public class ChangePasswordModel
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ConnectedAccount
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty; // "gmail", "outlook", "imap", etc.
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsDefault { get; set; }
    public bool IsConnected { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public Dictionary<string, string>? ProviderData { get; set; }
}

public class AddAccountModel
{
    public string Email { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? Password { get; set; }
    public Dictionary<string, string>? ProviderData { get; set; }
}

public class UpdateAccountModel
{
    public string? DisplayName { get; set; }
    public string? Password { get; set; }
    public Dictionary<string, string>? ProviderData { get; set; }
}

public class AccountSettings
{
    public EmailDisplaySettings EmailDisplay { get; set; } = new();
    public EmailComposeSettings EmailCompose { get; set; } = new();
    public NotificationSettings Notifications { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
    public AutoReplySettings AutoReply { get; set; } = new();
    public ForwardingSettings Forwarding { get; set; } = new();
    public string? DefaultSignatureId { get; set; }
}

public class EmailDisplaySettings
{
    public int EmailsPerPage { get; set; } = 25;
    public bool ShowImages { get; set; } = true;
    public bool ShowExternalImages { get; set; } = false;
    public bool ShowNotification { get; set; } = true;
    public bool ShowPreviewText { get; set; } = true;
    public bool ShowFullDate { get; set; } = false;
    public string DefaultView { get; set; } = "conversation"; // "conversation" or "individual"
    public string Theme { get; set; } = "light"; // "light", "dark", or "system"
}

public class EmailComposeSettings
{
    public bool AutoSaveDrafts { get; set; } = true;
    public int AutoSaveInterval { get; set; } = 5; // in seconds
    public bool SpellCheck { get; set; } = true;
    public bool AutoCorrect { get; set; } = true;
    public bool RichText { get; set; } = true;
    public bool RequestReadReceipt { get; set; } = false;
    public bool IncludeOriginalMessage { get; set; } = true;
    public string DefaultFormat { get; set; } = "html"; // "html" or "plain"
}

public class NotificationSettings
{
    public bool EnableDesktopNotifications { get; set; } = true;
    public bool EnableSound { get; set; } = true;
    public bool NotifyForAllEmails { get; set; } = false;
    public bool NotifyOnlyForImportant { get; set; } = true;
    public bool NotifyForMentions { get; set; } = true;
    public string[] NotifyEmailAddresses { get; set; } = Array.Empty<string>();
}

public class SecuritySettings
{
    public bool TwoFactorEnabled { get; set; } = false;
    public string[] RecoveryEmailAddresses { get; set; } = Array.Empty<string>();
    public string[] RecoveryPhoneNumbers { get; set; } = Array.Empty<string>();
    public bool ShowUnreadMessageCount { get; set; } = true;
    public bool AutoLogout { get; set; } = true;
    public int AutoLogoutMinutes { get; set; } = 30;
    public bool ShowExternalContentWarning { get; set; } = true;
}

public class AutoReplySettings
{
    public bool Enabled { get; set; } = false;
    public string? Subject { get; set; }
    public string? Message { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool OnlySendOncePerSender { get; set; } = true;
    public bool OnlySendToContacts { get; set; } = false;
}

public class ForwardingSettings
{
    public bool Enabled { get; set; } = false;
    public string? ForwardToEmail { get; set; }
    public bool KeepCopy { get; set; } = true;
    public string? Filter { get; set; }
}

public class EmailSignature
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string PlainTextContent { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
