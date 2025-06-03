using DafornoMail.Core.Interfaces.Repositories;
using DafornoMail.Core.Models;
using DafornoMail.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DafornoMail.Infrastructure.Repositories;

public class EmailAccountRepository : BaseRepository<EmailAccount>, IEmailAccountRepository
{
    public EmailAccountRepository(ApplicationDbContext context, ILogger<EmailAccountRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<EmailAccount> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.Email == email) 
            ?? throw new KeyNotFoundException($"Email account with email {email} not found");
    }

    public async Task<IEnumerable<EmailAccount>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Email)
            .ToListAsync();
    }

    public async Task SetDefaultAccountAsync(Guid accountId, string userId)
    {
        // Reset all accounts to non-default
        var accounts = await _dbSet
            .Where(a => a.UserId == userId)
            .ToListAsync();

        foreach (var account in accounts)
        {
            account.IsDefault = account.Id == accountId;
        }

        await _context.SaveChangesAsync();
    }


    public async Task<EmailAccount> GetDefaultAccountAsync(string userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault)
            ?? await _dbSet
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.Email)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"No email accounts found for user {userId}");
    }
}
