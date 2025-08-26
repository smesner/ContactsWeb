using ContactsWeb.Models;

namespace ContactsWeb.Services;

public interface IContactService
{
    Task<ContactSubmissionResult> ProccessContactAsync(ContactFormViewModel form, CancellationToken token);
    Task<IEnumerable<Contact>> GetBizContactAsync(CancellationToken token);
}
