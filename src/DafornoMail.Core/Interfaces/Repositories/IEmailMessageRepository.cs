namespace DafornoMail.Core.Interfaces.Repositories
{
    public interface IEmailMessageRepository
    {
        Task<IEnumerable<EmailMessage>> GetByFolderAsync(
            Guid folderId,
            int page = 1,
            int pageSize = 50,
            string sortBy = "Date",
            bool sortDescending = true);

        Task<IEnumerable<EmailMessage>> SearchAsync(
            Guid accountId,
            string query,
            int page = 1,
            int pageSize = 50,
            Guid? folderId = null);

        Task<IEnumerable<EmailMessage>> GetByAccountIdAsync(
            Guid accountId,
            int page = 1,
            int pageSize = 50,
            string sortBy = "Date",
            bool sortDescending = true);
    }
}