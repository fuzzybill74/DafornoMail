using DafornoMail.Core.Models;

namespace DafornoMail.Core.Interfaces.Repositories
{
    public interface IEmailAccountRepository
    {
        Task<EmailAccount> AddAsync(EmailAccount account);
    }
}