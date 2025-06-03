using DafornoMail.Core.Models;

namespace DafornoMail.Core.Interfaces;

public interface IEmailService
{
    // Account Management
    Task<bool> TestConnectionAsync(EmailAccount account);
    Task<EmailAccount> AddAccountAsync(EmailAccount account, string password);
    Task<bool> RemoveAccountAsync(Guid accountId);
    Task<EmailAccount> GetAccountAsync(Guid accountId);
    Task<IEnumerable<EmailAccount>> GetUserAccountsAsync(string userId);
    
    // Folder Operations
    Task<IEnumerable<EmailFolder>> GetFoldersAsync(Guid accountId, bool forceRefresh = false);
    Task<EmailFolder> GetFolderAsync(Guid folderId);
    Task<EmailFolder> CreateFolderAsync(EmailFolder folder);
    Task<bool> DeleteFolderAsync(Guid folderId);
    Task<bool> RenameFolderAsync(Guid folderId, string newName);
    Task<bool> MoveFolderAsync(Guid folderId, string newParentFolderId);
    
    // Message Operations
    Task<IEnumerable<EmailMessage>> GetMessagesAsync(
        Guid accountId, 
        Guid? folderId = null, 
        int page = 1, 
        int pageSize = 50, 
        string? searchQuery = null, 
        bool unreadOnly = false,
        string? sortBy = "Date", 
        bool sortDescending = true);
        
    Task<EmailMessage> GetMessageAsync(Guid messageId);
    Task<EmailMessage> GetMessageByProviderIdAsync(string providerMessageId);
    Task<bool> MoveMessageAsync(Guid messageId, Guid targetFolderId);
    Task<bool> MoveMessagesAsync(IEnumerable<Guid> messageIds, Guid targetFolderId);
    Task<bool> MarkAsReadAsync(Guid messageId, bool read = true);
    Task<bool> MarkAsFlaggedAsync(Guid messageId, bool flagged = true);
    Task<bool> DeleteMessageAsync(Guid messageId, bool permanentDelete = false);
    
    // Message Composition
    Task<EmailMessage> SendMessageAsync(EmailMessage message, IEnumerable<string> attachmentPaths = null);
    Task<EmailMessage> SaveDraftAsync(EmailMessage draft);
    Task<EmailMessage> ReplyToMessageAsync(Guid originalMessageId, string replyBody, bool replyAll = false);
    Task<EmailMessage> ForwardMessageAsync(Guid originalMessageId, string forwardBody, IEnumerable<string> recipients);
    
    // Sync Operations
    Task<bool> SyncAccountAsync(Guid accountId, bool fullSync = false);
    Task<bool> SyncFolderAsync(Guid folderId, bool fullSync = false);
    
    // Attachments
    Task<Stream> GetAttachmentAsync(Guid attachmentId);
    Task<string> SaveAttachmentAsync(Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteAttachmentAsync(Guid attachmentId);
    
    // Search
    Task<IEnumerable<EmailMessage>> SearchMessagesAsync(
        string userId, 
        string query, 
        int page = 1, 
        int pageSize = 50, 
        string? folderId = null);
        
    // Labels (Gmail specific)
    Task<bool> AddLabelToMessageAsync(Guid messageId, string labelName);
    Task<bool> RemoveLabelFromMessageAsync(Guid messageId, string labelName);
    Task<IEnumerable<string>> GetLabelsAsync(Guid accountId);
    
    // Cleanup
    Task CleanupOldMessagesAsync(TimeSpan olderThan);
}
