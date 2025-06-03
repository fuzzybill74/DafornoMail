// DafornoMail.Infrastructure/Email/BaseEmailProvider.cs
using DafornoMail.Core.Interfaces;
using DafornoMail.Core.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace DafornoMail.Infrastructure.Email;

public abstract class BaseEmailProvider : IEmailProvider
{
    protected readonly ILogger _logger;
    protected ImapClient? _imapClient;
    protected SmtpClient? _smtpClient;
    protected string? _email;
    protected string? _password;
    protected string? _accessToken;
    protected bool _useOAuth;
    protected bool _isListening;
    protected CancellationTokenSource? _cancellationTokenSource;

    // Events
    public event EventHandler<EmailMessage>? NewMessageReceived;
    public event EventHandler<string>? MessageDeleted;
    public event EventHandler<(string MessageId, string?[] ChangedFlags)>? MessageFlagsChanged;

    // Capabilities
    public virtual bool SupportsFolders => true;
    public virtual bool SupportsLabels => true;
    public virtual bool SupportsThreads => true;
    public virtual bool SupportsSearch => true;
    public virtual bool SupportsSync => true;

    protected BaseEmailProvider(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public virtual async Task<bool> ConnectAsync(string email, string password, bool useOAuth = false, string? accessToken = null)
    {
        _email = email ?? throw new ArgumentNullException(nameof(email));
        _password = password;
        _useOAuth = useOAuth;
        _accessToken = accessToken;

        try
        {
            await ConnectImapAsync();
            await ConnectSmtpAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to email provider");
            Disconnect();
            throw;
        }
    }

    protected virtual async Task ConnectImapAsync()
    {
        _imapClient = new ImapClient();
        await _imapClient.ConnectAsync(GetImapServer(), GetImapPort(), GetImapSslOptions());

        if (_useOAuth && !string.IsNullOrEmpty(_accessToken))
        {
            var oauth2 = new SaslMechanismOAuth2(_email, _accessToken);
            await _imapClient.AuthenticateAsync(oauth2);
        }
        else
        {
            await _imapClient.AuthenticateAsync(new NetworkCredential(_email, _password));
        }
    }

    protected virtual async Task ConnectSmtpAsync()
    {
        _smtpClient = new SmtpClient();
        await _smtpClient.ConnectAsync(GetSmtpServer(), GetSmtpPort(), GetSmtpSslOptions());

        if (_useOAuth && !string.IsNullOrEmpty(_accessToken))
        {
            var oauth2 = new SaslMechanismOAuth2(_email, _accessToken);
            await _smtpClient.AuthenticateAsync(oauth2);
        }
        else
        {
            await _smtpClient.AuthenticateAsync(new NetworkCredential(_email, _password));
        }
    }

    public virtual void Disconnect()
    {
        _imapClient?.Disconnect(true);
        _smtpClient?.Disconnect(true);
        _imapClient?.Dispose();
        _smtpClient?.Dispose();
    }

    public virtual async Task<bool> IsConnectedAsync()
    {
        return _imapClient?.IsConnected == true && _smtpClient?.IsConnected == true;
    }

    public virtual async Task<EmailFolder> CreateFolderAsync(string name, string? parentFolderId = null)
    {
        if (_imapClient == null || !_imapClient.IsConnected)
            throw new InvalidOperationException("Not connected to IMAP server");

        var parent = parentFolderId != null 
            ? await _imapClient.GetFolderAsync(parentFolderId) 
            : _imapClient.Inbox.ParentFolder;
            
        var newFolder = await parent.CreateAsync(name, true);
        return new EmailFolder
        {
            Id = newFolder.Id,
            Name = newFolder.Name,
            FullName = newFolder.FullName,
            ParentFolderId = parent.FullName != newFolder.FullName ? parent.FullName : null,
            Attributes = newFolder.Attributes.ToString()
        };
    }

    public virtual async Task<bool> DeleteFolderAsync(string folderId)
    {
        if (_imapClient == null || !_imapClient.IsConnected)
            throw new InvalidOperationException("Not connected to IMAP server");

        var folder = await _imapClient.GetFolderAsync(folderId);
        await folder.DeleteAsync();
        return true;
    }

    public virtual async Task<EmailMessage> GetMessageAsync(string messageId, string? folderId = null)
    {
        if (_imapClient == null || !_imapClient.IsConnected)
            throw new InvalidOperationException("Not connected to IMAP server");

        var folder = folderId != null 
            ? await _imapClient.GetFolderAsync(folderId) 
            : _imapClient.Inbox;
            
        await folder.OpenAsync(FolderAccess.ReadOnly);
        var message = await folder.GetMessageAsync(new UniqueId(uint.Parse(messageId)));
        return ConvertToEmailMessage(message);
    }

    public virtual async Task<IEnumerable<EmailMessage>> GetMessagesAsync(string folderId, int skip = 0, int take = 50, bool unreadOnly = false)
    {
        if (_imapClient == null || !_imapClient.IsConnected)
            throw new InvalidOperationException("Not connected to IMAP server");

        var folder = await _imapClient.GetFolderAsync(folderId);
        await folder.OpenAsync(FolderAccess.ReadOnly);

        var query = unreadOnly 
            ? SearchQuery.Not(SearchQuery.Seen) 
            : SearchQuery.All;
            
        var uids = await folder.SearchAsync(query);
        var messages = new List<EmailMessage>();
        
        foreach (var uid in uids.Skip(skip).Take(take))
        {
            var message = await folder.GetMessageAsync(uid);
            messages.Add(ConvertToEmailMessage(message));
        }
        
        return messages;
    }

    public virtual async Task SendMessageAsync(EmailMessage message)
    {
        if (_smtpClient == null || !_smtpClient.IsConnected)
            throw new InvalidOperationException("Not connected to SMTP server");

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(message.From, message.SenderEmail));
        mimeMessage.To.AddRange(message.To.Select(x => new MailboxAddress(x.From, x.Address)));
        mimeMessage.Cc.AddRange(message.Cc?.Select(x => new MailboxAddress(x.From, x.Address)) ?? Enumerable.Empty<MailboxAddress>());
        mimeMessage.Bcc.AddRange(message.Bcc?.Select(x => new MailboxAddress(x.Name, x.Address)) ?? Enumerable.Empty<MailboxAddress>());
        mimeMessage.Subject = message.Subject;
        
        var builder = new BodyBuilder();
        if (!string.IsNullOrEmpty(message.HtmlBody))
            builder.HtmlBody = message.HtmlBody;
        if (!string.IsNullOrEmpty(message.HtmlBody))
            builder.TextBody = message.HtmlBody;
            
        mimeMessage.Body = builder.ToMessageBody();
        
        await _smtpClient.SendAsync(mimeMessage);
    }

    public virtual async Task DeleteMessageAsync(string messageId, string? folderId = null)
    {
        if (_imapClient == null || !_imapClient.IsConnected)
            throw new InvalidOperationException("Not connected to IMAP server");

        var folder = folderId != null 
            ? await _imapClient.GetFolderAsync(folderId) 
            : _imapClient.Inbox;
            
        await folder.OpenAsync(FolderAccess.ReadWrite);
        await folder.StoreAsync(new UniqueId(uint.Parse(messageId)), new StoreFlagsRequest(StoreAction.Add, MessageFlags.Deleted) { Silent = true });
        await folder.ExpungeAsync();
    }

    public virtual async Task MoveMessageAsync(string messageId, string sourceFolderId, string destinationFolderId)
    {
        if (_imapClient == null || !_imapClient.IsConnected)
            throw new InvalidOperationException("Not connected to IMAP server");

        var sourceFolder = await _imapClient.GetFolderAsync(sourceFolderId);
        var destinationFolder = await _imapClient.GetFolderAsync(destinationFolderId);
        
        await sourceFolder.OpenAsync(FolderAccess.ReadWrite);
        await sourceFolder.MoveToAsync(new UniqueId(uint.Parse(messageId)), destinationFolder);
    }

    public virtual async Task MarkAsReadAsync(string messageId, bool read = true, string? folderId = null)
    {
        if (_imapClient == null || !_imapClient.IsConnected)
            throw new InvalidOperationException("Not connected to IMAP server");

        var folder = folderId != null 
            ? await _imapClient.GetFolderAsync(folderId) 
            : _imapClient.Inbox;
            
        await folder.OpenAsync(FolderAccess.ReadWrite);
        await folder.StoreAsync(
            new UniqueId(uint.Parse(messageId)), 
            new StoreFlagsRequest(read ? MessageAction.Add : MessageAction.Remove, MessageFlags.Seen) { Silent = true });
    }

    public virtual async Task<IEnumerable<EmailMessage>> SearchMessagesAsync(string query, string? folderId = null, bool unreadOnly = false)
    {
        if (_imapClient == null || !_imapClient.IsConnected)
            throw new InvalidOperationException("Not connected to IMAP server");

        var folder = folderId != null 
            ? await _imapClient.GetFolderAsync(folderId) 
            : _imapClient.Inbox;
            
        await folder.OpenAsync(FolderAccess.ReadOnly);
        
        var searchQuery = new List<SearchQuery> { SearchQuery.SubjectContains(query) };
        if (unreadOnly)
            searchQuery.Add(SearchQuery.NotSeen);
            
        var uids = await folder.SearchAsync(SearchQuery.And(searchQuery));
        var messages = new List<EmailMessage>();
        
        foreach (var uid in uids)
        {
            var message = await folder.GetMessageAsync(uid);
            messages.Add(ConvertToEmailMessage(message));
        }
        
        return messages;
    }

    public virtual async Task StartListeningAsync()
    {
        if (_imapClient == null || !_imapClient.IsConnected)
            throw new InvalidOperationException("Not connected to IMAP server");

        if (_isListening)
            return;
            
        _cancellationTokenSource = new CancellationTokenSource();
        _isListening = true;
        
        _ = Task.Run(async () =>
        {
            while (_isListening && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await _imapClient.Inbox.IdleAsync(_cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // Expected when stopping
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in IMAP IDLE loop");
                    await Task.Delay(5000, _cancellationTokenSource.Token);
                }
            }
        }, _cancellationTokenSource.Token);
        
        _imapClient.Inbox.CountChanged += OnMessageCountChanged;
        _imapClient.Inbox.MessageFlagsChanged += OnMessageFlagsChanged;
    }

    public virtual async Task StopListeningAsync()
    {
        if (!_isListening || _cancellationTokenSource == null)
            return;
            
        _isListening = false;
        _cancellationTokenSource.Cancel();
        
        if (_imapClient != null)
        {
            _imapClient.Inbox.CountChanged -= OnMessageCountChanged;
            _imapClient.Inbox.MessageFlagsChanged -= OnMessageFlagsChanged;
        }
        
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = null;
    }

    protected virtual void OnMessageCountChanged(object? sender, EventArgs e)
    {
        if (sender is IMailFolder folder && NewMessageReceived != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var message = await folder.GetMessageAsync(folder.Count - 1);
                    NewMessageReceived?.Invoke(this, ConvertToEmailMessage(message));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing new message");
                }
            });
        }
    }

    protected virtual void OnMessageFlagsChanged(object? sender, MessageFlagsChangedEventArgs e)
    {
        MessageFlagsChanged?.Invoke(this, (e.Index.ToString(), e.Flags.Select(f => f.ToString()).ToArray()));
    }

    protected virtual EmailMessage ConvertToEmailMessage(MimeMessage message)
    {
        return new EmailMessage
        {
            Id = message.MessageId,
            Subject = message.Subject,
            TextBody= message.TextBody,
            HtmlBody = message.HtmlBody,
            Date = message.Date.UtcDateTime,
            From = new EmailAddress { Name = message.From.Mailboxes.First().Name, Address = message.From.Mailboxes.First().Address },
            To = message.To.Mailboxes.Select(x => new EmailAddress { Name = x.Name, Address = x.Address }).ToList(),
            Cc = message.Cc.Mailboxes.Select(x => new EmailAddress { Name = x.Name, Address = x.Address }).ToList(),
            Bcc = message.Bcc.Mailboxes.Select(x => new EmailAddress { Name = x.Name, Address = x.Address }).ToList(),
            IsRead = !message.Flags.HasValue || !message.Flags.Value.HasFlag(MessageFlags.Seen),
            HasAttachments = message.Attachments.Any(),
            Headers = message.Headers.ToDictionary(h => h.Field, h => h.Value)
        };
    }

    public async Task DisconnectAsync()
    {
        await StopListeningAsync();
        Disconnect();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cancellationTokenSource?.Dispose();
            _imapClient?.Dispose();
            _smtpClient?.Dispose();
        }
    }

    // Abstract methods that must be implemented by derived classes
    protected abstract string GetImapServer();
    protected abstract int GetImapPort();
    protected abstract SecureSocketOptions GetImapSslOptions();
    protected abstract string GetSmtpServer();
    protected abstract int GetSmtpPort();
    protected abstract SecureSocketOptions GetSmtpSslOptions();

    // Abstract methods from IEmailProvider
    public abstract Task<string> GetProviderNameAsync();
    public abstract Task<EmailAccount> GetAccountInfoAsync();
    public abstract Task<IEnumerable<EmailFolder>> GetFoldersAsync();

    public Task<bool> RenameFolderAsync(string folderId, string newName)
    {
        throw new NotImplementedException();
    }

    public Task<bool> MoveFolderAsync(string folderId, string newParentFolderId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<EmailMessage>> GetMessagesAsync(string folderId, int skip = 0, int take = 50, bool skipAttachments = true, bool headersOnly = false)
    {
        throw new NotImplementedException();
    }

    public Task<EmailMessage> GetMessageAsync(string messageId, bool downloadAttachments = false)
    {
        throw new NotImplementedException();
    }

    public Task<EmailMessage> SendMessageAsync(EmailMessage message, IEnumerable<string> attachmentPaths = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> MoveMessageAsync(string messageId, string targetFolderId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> MoveMessagesAsync(IEnumerable<string> messageIds, string targetFolderId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> MarkAsReadAsync(string messageId, bool read = true)
    {
        throw new NotImplementedException();
    }

    public Task<bool> MarkAsFlaggedAsync(string messageId, bool flagged = true)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteMessageAsync(string messageId, bool permanentDelete = false)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<EmailMessage>> GetThreadAsync(string threadId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> AddLabelToMessageAsync(string messageId, string labelName)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemoveLabelFromMessageAsync(string messageId, string labelName)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetLabelsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<EmailMessage>> SearchMessagesAsync(string query, string? folderId = null, int skip = 0, int take = 50)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetChangedMessageIdsSinceAsync(DateTime since, string folderId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetDeletedMessageIdsSinceAsync(DateTime since, string folderId)
    {
        throw new NotImplementedException();
    }
}