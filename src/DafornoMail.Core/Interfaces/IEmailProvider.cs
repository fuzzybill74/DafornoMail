using DafornoMail.Core.Models;

namespace DafornoMail.Core.Interfaces;

public interface IEmailProvider : IDisposable
{
    // Connection and authentication
    Task<bool> ConnectAsync(string email, string password, bool useOAuth = false, string? accessToken = null);
    Task DisconnectAsync();
    Task<bool> IsConnectedAsync();
    
    // Account information
    Task<string> GetProviderNameAsync();
    Task<EmailAccount> GetAccountInfoAsync();
    
    // Folder operations
    Task<IEnumerable<EmailFolder>> GetFoldersAsync();
    Task<EmailFolder> CreateFolderAsync(string name, string? parentFolderId = null);
    Task<bool> DeleteFolderAsync(string folderId);
    Task<bool> RenameFolderAsync(string folderId, string newName);
    Task<bool> MoveFolderAsync(string folderId, string newParentFolderId);
    
    // Message operations
    Task<IEnumerable<EmailMessage>> GetMessagesAsync(
        string folderId, 
        int skip = 0, 
        int take = 50, 
        bool skipAttachments = true, 
        bool headersOnly = false);
        
    Task<EmailMessage> GetMessageAsync(string messageId, bool downloadAttachments = false);
    Task<EmailMessage> SendMessageAsync(EmailMessage message, IEnumerable<string> attachmentPaths = null);
    Task<bool> MoveMessageAsync(string messageId, string targetFolderId);
    Task<bool> MoveMessagesAsync(IEnumerable<string> messageIds, string targetFolderId);
    Task<bool> MarkAsReadAsync(string messageId, bool read = true);
    Task<bool> MarkAsFlaggedAsync(string messageId, bool flagged = true);
    Task<bool> DeleteMessageAsync(string messageId, bool permanentDelete = false);
    
    // Thread operations (Gmail specific)
    Task<IEnumerable<EmailMessage>> GetThreadAsync(string threadId);
    
    // Label operations (Gmail specific)
    Task<bool> AddLabelToMessageAsync(string messageId, string labelName);
    Task<bool> RemoveLabelFromMessageAsync(string messageId, string labelName);
    Task<IEnumerable<string>> GetLabelsAsync();
    
    // Search
    Task<IEnumerable<EmailMessage>> SearchMessagesAsync(
        string query, 
        string? folderId = null, 
        int skip = 0, 
        int take = 50);
    
    // Sync operations
    Task<IEnumerable<string>> GetChangedMessageIdsSinceAsync(DateTime since, string folderId);
    Task<IEnumerable<string>> GetDeletedMessageIdsSinceAsync(DateTime since, string folderId);
    
    // Capabilities
    bool SupportsFolders { get; }
    bool SupportsLabels { get; }
    bool SupportsThreads { get; }
    bool SupportsSearch { get; }
    bool SupportsSync { get; }
    
    // Events
    event EventHandler<EmailMessage>? NewMessageReceived;
    event EventHandler<string>? MessageDeleted;
    event EventHandler<(string MessageId, string?[] ChangedFlags)>? MessageFlagsChanged;
    
    // Start/Stop listening for real-time updates
    Task StartListeningAsync();
    Task StopListeningAsync();
    void Disconnect();
}
