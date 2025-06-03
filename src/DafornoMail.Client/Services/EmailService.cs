using System.Net.Http.Json;
using DafornoMail.Client.Models;
using Microsoft.AspNetCore.Components;

namespace DafornoMail.Client.Services;

public interface IEmailService
{
    Task<EmailListResult> GetEmailsAsync(string folder, int page = 1, int pageSize = 20, string search = "");
    Task<EmailMessage> GetEmailByIdAsync(string id, string folder = "inbox");
    Task<bool> SendEmailAsync(ComposeEmailModel email);
    Task<bool> MoveEmailAsync(string id, string fromFolder, string toFolder);
    Task<bool> DeleteEmailAsync(string id, string folder);
    Task<bool> MarkAsReadAsync(string id, string folder, bool read = true);
    Task<bool> MarkAsImportantAsync(string id, string folder, bool important = true);
    Task<IEnumerable<EmailFolder>> GetFoldersAsync();
    Task<EmailStats> GetEmailStatsAsync();
}

public class EmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        HttpClient httpClient,
        NavigationManager navigationManager,
        ILogger<EmailService> logger)
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;
        _logger = logger;
    }

    public async Task<EmailListResult> GetEmailsAsync(string folder, int page = 1, int pageSize = 20, string search = "")
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<EmailListResult>($"api/emails/{folder}?page={page}&pageSize={pageSize}&search={Uri.EscapeDataString(search)}");
            return response ?? new EmailListResult { Items = new List<EmailListItem>(), TotalCount = 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching emails");
            return new EmailListResult { Items = new List<EmailListItem>(), TotalCount = 0 };
        }
    }


    public async Task<EmailMessage> GetEmailByIdAsync(string id, string folder = "inbox")
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<EmailMessage>($"api/emails/{folder}/{id}") 
                   ?? throw new Exception("Email not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching email with ID: {id}");
            throw;
        }
    }

    public async Task<bool> SendEmailAsync(ComposeEmailModel email)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/emails/send", email);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email");
            return false;
        }
    }

    public async Task<bool> MoveEmailAsync(string id, string fromFolder, string toFolder)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/emails/move/{id}", 
                new { FromFolder = fromFolder, ToFolder = toFolder });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error moving email {id} from {fromFolder} to {toFolder}");
            return false;
        }
    }

    public async Task<bool> DeleteEmailAsync(string id, string folder)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/emails/{folder}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting email {id}");
            return false;
        }
    }

    public async Task<bool> MarkAsReadAsync(string id, string folder, bool read = true)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/emails/mark-read/{id}", 
                new { Folder = folder, Read = read });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error marking email {id} as {(read ? "read" : "unread")}");
            return false;
        }
    }

    public async Task<bool> MarkAsImportantAsync(string id, string folder, bool important = true)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/emails/mark-important/{id}", 
                new { Folder = folder, Important = important });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error marking email {id} as {(important ? "important" : "not important")}");
            return false;
        }
    }

    public async Task<IEnumerable<EmailFolder>> GetFoldersAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<EmailFolder>>("api/emails/folders") 
                   ?? new List<EmailFolder>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching folders");
            return new List<EmailFolder>();
        }
    }

    public async Task<EmailStats> GetEmailStatsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<EmailStats>("api/emails/stats") 
                   ?? new EmailStats();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching email stats");
            return new EmailStats();
        }
    }
}

public class EmailListResult
{
    public IEnumerable<EmailListItem> Items { get; set; } = new List<EmailListItem>();
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
}

public class EmailListItem
{
    public string Id { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Preview { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public bool IsRead { get; set; }
    public bool IsStarred { get; set; }
    public bool HasAttachments { get; set; }
    public List<string> Labels { get; set; } = new();
}

public class EmailMessage
{
    public string Id { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Cc { get; set; } = string.Empty;
    public string Bcc { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; }
    public DateTimeOffset Date { get; set; }
    public bool IsRead { get; set; }
    public bool IsImportant { get; set; }
    public List<EmailAttachment> Attachments { get; set; } = new();
    public List<string> Labels { get; set; } = new();
}

public class EmailAttachment
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string? PreviewUrl { get; set; }
}

public class ComposeEmailModel
{
    public string To { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public List<EmailAttachment>? Attachments { get; set; }
}

public class EmailFolder
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int UnreadCount { get; set; }
    public int TotalCount { get; set; }
    public string? ParentId { get; set; }
    public List<EmailFolder> Children { get; set; } = new();
}

public class EmailStats
{
    public int TotalEmails { get; set; }
    public int Unread { get; set; }
    public int Starred { get; set; }
    public int Important { get; set; }
    public int Drafts { get; set; }
    public int Sent { get; set; }
    public int Trash { get; set; }
}
