// DafornoMail.Infrastructure/Email/Providers/GmailProvider.cs
using DafornoMail.Core.Interfaces;
using DafornoMail.Core.Models;
using MailKit;
using MailKit.Security;
using Microsoft.Extensions.Logging;

namespace DafornoMail.Infrastructure.Email.Providers;

public class GmailProvider : BaseEmailProvider
{
    public GmailProvider(ILogger<GmailProvider> logger) : base(logger)
    {
    }

    protected override string GetImapServer() => "imap.gmail.com";
    protected override int GetImapPort() => 993;
    protected override SecureSocketOptions GetImapSslOptions() => SecureSocketOptions.SslOnConnect;
    protected override string GetSmtpServer() => "smtp.gmail.com";
    protected override int GetSmtpPort() => 587;
    protected override SecureSocketOptions GetSmtpSslOptions() => SecureSocketOptions.StartTls;

    public override async Task<string> GetProviderNameAsync() => "Gmail";

    public override async Task<EmailAccount> GetAccountInfoAsync()
    {
        if (_imapClient == null || !_imapClient.IsConnected)
            throw new InvalidOperationException("Not connected to IMAP server");

        var account = new EmailAccount
        {
            Email = _email!,
            DisplayName = _email!.Split('@')[0],
            Provider = "Gmail",
            IsDefault = false
        };

        return account;
    }

    public override async Task<IEnumerable<EmailFolder>> GetFoldersAsync()
    {
        if (_imapClient == null || !_imapClient.IsConnected)
            throw new InvalidOperationException("Not connected to IMAP server");

        var personal = await _imapClient.GetFolderAsync(_imapClient.PersonalNamespaces[0]);
        var folders = new List<EmailFolder>();

        await GetSubFoldersAsync(personal, folders, string.Empty);
        return folders;
    }

    private async Task GetSubFoldersAsync(IMailFolder parent, ICollection<EmailFolder> result, string parentPath)
    {
        var fullName = string.IsNullOrEmpty(parentPath) ? parent.Name : $"{parentPath}/{parent.Name}";

        var folder = new EmailFolder
        {
            Name = parent.Name,
            FullName = fullName,
            OriginalName = parent.FullName,
            IsSystemFolder = IsSystemFolder(parent),
            IsSelectable = parent.Attributes.HasFlag(FolderAttributes.NoSelect) == false,
            HasChildren = parent.Attributes.HasFlag(FolderAttributes.HasChildren),
            ParentFolderId = string.IsNullOrEmpty(parentPath) ? null : parentPath
        };

        result.Add(folder);

        if (parent.Attributes.HasFlag(FolderAttributes.HasChildren))
        {
            var subfolders = await parent.GetSubfoldersAsync();
            foreach (var subfolder in subfolders)
            {
                await GetSubFoldersAsync(subfolder, result, fullName);
            }
        }
    }

    private bool IsSystemFolder(IMailFolder folder)
    {
        var systemFolderNames = new[] { "inbox", "sent", "drafts", "trash", "spam", "junk", "archive" };
        return systemFolderNames.Contains(folder.Name.ToLowerInvariant());
    }

    // Implement other Gmail-specific methods...
}