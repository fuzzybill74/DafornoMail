using DafornoMail.Core.Models;

namespace DafornoMail.Core.Interfaces.Repositories
{
    public interface IEmailAccountRepository
    {
        Task<EmailAccount> AddAsync(EmailAccount account);

        /// <summary>
        /// Retrieves an account by its identifier.
        /// </summary>
        Task<EmailAccount> GetByIdAsync(Guid id);
    }
}