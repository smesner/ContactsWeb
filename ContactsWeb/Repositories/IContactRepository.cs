using ContactsWeb.Models;

namespace ContactsWeb.Repositories;

public interface IContactRepository
{
    Task InitializeDatabaseAsync();
    Task<bool> CanInsertContactAsync(string email, CancellationToken token);
    Task<int> InsertContactAsync(Contact contact, CancellationToken token);
    Task<IEnumerable<Contact>> GetBizEmailContactsAsync(CancellationToken token);
}
