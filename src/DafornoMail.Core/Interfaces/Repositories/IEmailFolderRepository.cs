
namespace DafornoMail.Core.Interfaces.Repositories
{
    public interface IEmailFolderRepository
    {
        /// <summary>
        /// Returns folders belonging to the specified account.
        /// </summary>
        Task<IEnumerable<EmailFolder>> GetByAccountIdAsync(Guid accountId);
    }
}