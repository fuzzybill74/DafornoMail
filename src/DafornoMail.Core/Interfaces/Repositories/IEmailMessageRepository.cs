using DafornoMail.Core.Models;

namespace DafornoMail.Core.Interfaces.Repositories
{
    public interface IEmailMessageRepository
    {
        // CRUD operations
        Task<EmailMessage> GetByIdAsync(Guid id);
        Task<IEnumerable<EmailMessage>> GetAllAsync();
        Task<EmailMessage> AddAsync(EmailMessage message);
        Task UpdateAsync(EmailMessage message);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);

        // Query helpers
        Task<EmailMessage> GetByProviderIdAsync(string providerMessageId);
        Task<IEnumerable<EmailMessage>> GetByAccountIdAsync(Guid accountId);
        Task<IEnumerable<EmailMessage>> GetByFolderAsync(Guid folderId, int page = 1, int pageSize = 50, string sortBy = "Date", bool sortDescending = true);
        Task<IEnumerable<EmailMessage>> SearchAsync(Guid accountId, string query, int page = 1, int pageSize = 50, Guid? folderId = null);
        Task<int> MoveMessagesAsync(IEnumerable<Guid> messageIds, Guid targetFolderId);
        Task<int> MarkAsReadAsync(IEnumerable<Guid> messageIds, bool read = true);
        Task<int> MarkAsFlaggedAsync(IEnumerable<Guid> messageIds, bool flagged = true);
        Task<IEnumerable<EmailMessage>> GetThreadAsync(string threadId, Guid accountId);
        Task<int> GetCountByFolderAsync(Guid folderId, bool unreadOnly = false);
        Task<int> GetTotalCountByAccountAsync(Guid accountId);
        Task<int> DeleteOldMessagesAsync(DateTime cutoffDate);
        Task<bool> AddLabelToMessageAsync(Guid messageId, string labelName);
        Task<bool> RemoveLabelFromMessageAsync(Guid messageId, string labelName);
        Task<IEnumerable<string>> GetExistingProviderIdsAsync(Guid folderId, IEnumerable<string> providerIds);
        Task<IEnumerable<EmailMessage>> GetMessagesForSyncAsync(Guid folderId, DateTime since);
        Task DeleteAsync(Guid id, bool permanent);
    }
}
