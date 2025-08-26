using ContactsWeb.Models;

namespace ContactsWeb.Services;

public interface IEmailService
{
    Task SendNotificationAsync(Contact contact, CancellationToken token);
}
