using DafornoMail.Core.Models;

namespace DafornoMail.Core.Interfaces.Repositories
{
    public interface IEmailFolderRepository
    {
        // CRUD operations
        Task<EmailFolder> GetByIdAsync(Guid id);
        Task<IEnumerable<EmailFolder>> GetAllAsync();
        Task<EmailFolder> AddAsync(EmailFolder folder);
        Task UpdateAsync(EmailFolder folder);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);

        // Query helpers
        Task<IEnumerable<EmailFolder>> GetByAccountIdAsync(Guid accountId);
        Task<EmailFolder> GetByFullNameAsync(Guid accountId, string fullName);
        Task<IEnumerable<EmailFolder>> GetSystemFoldersAsync(Guid accountId);
        Task<IEnumerable<EmailFolder>> GetSubFoldersAsync(Guid parentFolderId);
        Task UpdateLastSyncedAsync(Guid folderId, DateTime lastSynced);
        Task<int> GetMessageCountAsync(Guid folderId, bool unreadOnly = false);
    }
}
