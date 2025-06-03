using DafornoMail.Core.Interfaces.Repositories;
using DafornoMail.Core.Models;
using DafornoMail.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DafornoMail.Infrastructure.Repositories;

public class EmailFolderRepository : BaseRepository<EmailFolder>, IEmailFolderRepository
{
    public EmailFolderRepository(ApplicationDbContext context, ILogger<EmailFolderRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<IEnumerable<EmailFolder>> GetByAccountIdAsync(Guid accountId)
    {
        return await _dbSet
            .Where(f => f.EmailAccountId == accountId)
            .OrderBy(f => f.Name)
            .ToListAsync();
    }

    public async Task<EmailFolder> GetByFullNameAsync(Guid accountId, string fullName)
    {
        return await _dbSet
            .FirstOrDefaultAsync(f => f.EmailAccountId == accountId && f.FullName == fullName)
            ?? throw new KeyNotFoundException($"Folder with full name {fullName} not found for account {accountId}");
    }

    public async Task<IEnumerable<EmailFolder>> GetSystemFoldersAsync(Guid accountId)
    {
        return await _dbSet
            .Where(f => f.EmailAccountId == accountId && f.IsSystemFolder)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmailFolder>> GetSubFoldersAsync(Guid parentFolderId)
    {
        var parentFolder = await GetByIdAsync(parentFolderId);
        return await _dbSet
            .Where(f => f.ParentFolderId == parentFolderId.ToString())
            .ToListAsync();
    }

    public async Task UpdateLastSyncedAsync(Guid folderId, DateTime lastSynced)
    {
        var folder = await GetByIdAsync(folderId);
        folder.LastSynced = lastSynced;
        await UpdateAsync(folder);
    }

    public async Task<int> GetMessageCountAsync(Guid folderId, bool unreadOnly = false)
    {
        var query = _context.EmailMessages
            .Where(m => m.FolderId == folderId);

        if (unreadOnly)
        {
            query = query.Where(m => !m.IsRead);
        }

        
        return await query.CountAsync();
    }
}
