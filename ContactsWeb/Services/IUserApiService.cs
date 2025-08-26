using ContactsWeb.Models.External;

namespace ContactsWeb.Services;

public interface IUserApiService
{
    Task<User?> FetchUserByEmailAsync(string email, CancellationToken token);
}
