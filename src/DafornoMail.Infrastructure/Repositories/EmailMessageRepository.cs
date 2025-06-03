using System.Text.Json;
using DafornoMail.Core.Interfaces.Repositories;
using DafornoMail.Core.Models;
using DafornoMail.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace DafornoMail.Infrastructure.Repositories;

public class EmailMessageRepository : BaseRepository<EmailMessage>, IEmailMessageRepository
{
    public EmailMessageRepository(
        ApplicationDbContext context,
        ILogger<EmailMessageRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<IEnumerable<EmailMessage>> GetByAccountIdAsync(
        Guid accountId,
        int page = 1,
        int pageSize = 50,
        string sortBy = "Date",
        bool sortDescending = true)
    {
        var query = _dbSet.Where(m => m.EmailAccountId == accountId && !m.IsDeleted);

        query = sortBy.ToLower() switch
        {
            "subject" => sortDescending
                ? query.OrderByDescending(m => m.Subject)
                : query.OrderBy(m => m.Subject),
            "from" => sortDescending
                ? query.OrderByDescending(m => m.SenderName ?? m.SenderEmail ?? m.From)
                : query.OrderBy(m => m.SenderName ?? m.SenderEmail ?? m.From),
            _ => sortDescending
                ? query.OrderByDescending(m => m.Date)
                : query.OrderBy(m => m.Date)
        };

        return await ApplyPaging(query, page, pageSize).ToListAsync();
    }


    public async Task<EmailMessage> GetByProviderIdAsync(string providerMessageId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(m => m.MessageId == providerMessageId)
            ?? throw new KeyNotFoundException($"Message with provider ID {providerMessageId} not found");
    }

    public async Task<IEnumerable<EmailMessage>> GetByFolderAsync(
        Guid folderId, 
        int page = 1, 
        int pageSize = 50, 
        string sortBy = "Date", 
        bool sortDescending = true)
    {
        var query = _dbSet
            .Where(m => m.FolderId == folderId && !m.IsDeleted);

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "subject" => sortDescending 
                ? query.OrderByDescending(m => m.Subject) 
                : query.OrderBy(m => m.Subject),
            "from" => sortDescending 
                ? query.OrderByDescending(m => m.SenderName ?? m.SenderEmail ?? m.From) 
                : query.OrderBy(m => m.SenderName ?? m.SenderEmail ?? m.From),
            _ => sortDescending 
                ? query.OrderByDescending(m => m.Date) 
                : query.OrderBy(m => m.Date)
        };

        return await ApplyPaging(query, page, pageSize).ToListAsync();
    }


    public async Task<IEnumerable<EmailMessage>> SearchAsync(
        Guid accountId,
        string query,
        int page = 1,
        int pageSize = 50,
        Guid? folderId = null)
    {
        var searchQuery = query.ToLower();
        
        var messages = _dbSet
            .Where(m => m.EmailAccountId == accountId && 
                       !m.IsDeleted && 
                       (folderId == null || m.FolderId == folderId) &&
                       (m.Subject.ToLower().Contains(searchQuery) ||
                        m.PreviewText.ToLower().Contains(searchQuery) ||
                        m.TextBody.ToLower().Contains(searchQuery) ||
                        m.HtmlBody.ToLower().Contains(searchQuery) ||
                        m.From.ToLower().Contains(searchQuery) ||
                        m.SenderEmail.ToLower().Contains(searchQuery) ||
                        m.SenderName.ToLower().Contains(searchQuery)));

        return await ApplyPaging(messages, page, pageSize).ToListAsync();
    }


    public async Task<int> MoveMessagesAsync(IEnumerable<Guid> messageIds, Guid targetFolderId)
    {
        var messages = await _dbSet
            .Where(m => messageIds.Contains(m.Id))
            .ToListAsync();

        foreach (var message in messages)
        {
            message.FolderId = targetFolderId;
        }

        return await _context.SaveChangesAsync();
    }


    public async Task<int> MarkAsReadAsync(IEnumerable<Guid> messageIds, bool read = true)
    {
        var messages = await _dbSet
            .Where(m => messageIds.Contains(m.Id))
            .ToListAsync();

        foreach (var message in messages)
        {
            message.IsRead = read;
        }


        return await _context.SaveChangesAsync();
    }


    public async Task<int> MarkAsFlaggedAsync(IEnumerable<Guid> messageIds, bool flagged = true)
    {
        var messages = await _dbSet
            .Where(m => messageIds.Contains(m.Id))
            .ToListAsync();

        foreach (var message in messages)
        {
            message.IsFlagged = flagged;
        }


        return await _context.SaveChangesAsync();
    }


    public async Task<IEnumerable<EmailMessage>> GetThreadAsync(string threadId, Guid accountId)
    {
        return await _dbSet
            .Where(m => m.ThreadId == threadId && m.EmailAccountId == accountId)
            .OrderBy(m => m.Date)
            .ToListAsync();
    }


    public async Task<int> GetCountByFolderAsync(Guid folderId, bool unreadOnly = false)
    {
        var query = _dbSet.Where(m => m.FolderId == folderId && !m.IsDeleted);
        
        if (unreadOnly)
        {
            query = query.Where(m => !m.IsRead);
        }
        
        return await query.CountAsync();
    }


    public async Task<int> GetTotalCountByAccountAsync(Guid accountId)
    {
        return await _dbSet
            .Where(m => m.EmailAccountId == accountId && !m.IsDeleted)
            .CountAsync();
    }


    public async Task<int> DeleteOldMessagesAsync(DateTime cutoffDate)
    {
        var oldMessages = await _dbSet
            .Where(m => m.Date < cutoffDate && m.IsDeleted)
            .ToListAsync();

        _dbSet.RemoveRange(oldMessages);
        return await _context.SaveChangesAsync();
    }


    public async Task<bool> AddLabelToMessageAsync(Guid messageId, string labelName)
    {
        var message = await GetByIdAsync(messageId);
        var labels = string.IsNullOrEmpty(message.Labels) 
            ? new List<string>() 
            : JsonSerializer.Deserialize<List<string>>(message.Labels) ?? new List<string>();

        if (!labels.Contains(labelName))
        {
            labels.Add(labelName);
            message.Labels = JsonSerializer.Serialize(labels);
            await UpdateAsync(message);
            return true;
        }
        
        return false;
    }


    public async Task<bool> RemoveLabelFromMessageAsync(Guid messageId, string labelName)
    {
        var message = await GetByIdAsync(messageId);
        if (string.IsNullOrEmpty(message.Labels)) 
            return false;
            
        var labels = JsonSerializer.Deserialize<List<string>>(message.Labels);
        if (labels == null || !labels.Contains(labelName)) 
            return false;
            
        labels.Remove(labelName);
        message.Labels = JsonSerializer.Serialize(labels);
        await UpdateAsync(message);
        return true;
    }


    public async Task<IEnumerable<string>> GetExistingProviderIdsAsync(Guid folderId, IEnumerable<string> providerIds)
    {
        return await _dbSet
            .Where(m => m.FolderId == folderId && providerIds.Contains(m.MessageId))
            .Select(m => m.MessageId)
            .ToListAsync();
    }


    public async Task<IEnumerable<EmailMessage>> GetMessagesForSyncAsync(Guid folderId, DateTime since)
    {
        return await _dbSet
            .Where(m => m.FolderId == folderId && m.LastModified >= since)
            .ToListAsync();
    }


    public override async Task DeleteAsync(Guid id)
    {
        var message = await GetByIdAsync(id);
        message.IsDeleted = true;
        message.LastModified = DateTime.UtcNow;
        await UpdateAsync(message);
    }


    public async Task DeleteAsync(Guid id, bool permanent = false)
    {
        if (permanent)
        {
            await base.DeleteAsync(id);
        }
        else
        {
            await DeleteAsync(id);
        }
    }
}
