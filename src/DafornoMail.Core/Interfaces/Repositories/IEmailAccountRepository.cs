using DafornoMail.Core.Models;

namespace DafornoMail.Core.Interfaces.Repositories
{
    public interface IEmailAccountRepository
    {
        // CRUD operations
        Task<EmailAccount> GetByIdAsync(Guid id);
        Task<IEnumerable<EmailAccount>> GetAllAsync();
        Task<EmailAccount> AddAsync(EmailAccount account);


        /// <summary>
        /// Retrieves an account by its identifier.
        /// </summary>
        Task<EmailAccount> GetByIdAsync(Guid id);
        Task UpdateAsync(EmailAccount account);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);

        // Query helpers
        Task<EmailAccount> GetByEmailAsync(string email);
        Task<IEnumerable<EmailAccount>> GetByUserIdAsync(string userId);
        Task SetDefaultAccountAsync(Guid accountId, string userId);
        Task<EmailAccount> GetDefaultAccountAsync(string userId);

    }
}
