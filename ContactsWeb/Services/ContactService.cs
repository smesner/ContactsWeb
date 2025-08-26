using ContactsWeb.Models;
using ContactsWeb.Models.External;
using ContactsWeb.Repositories;
using System.Globalization;

namespace ContactsWeb.Services;

public class ContactService : IContactService
{
    private readonly IContactRepository _repository;
    private readonly IUserApiService _userApiService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ContactService> _logger;

    public ContactService(
        IContactRepository repository, 
        IUserApiService userApiService, 
        IEmailService emailService,
        ILogger<ContactService> logger)
    {
        _repository = repository;
        _userApiService = userApiService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ContactSubmissionResult> ProccessContactAsync(ContactFormViewModel form, CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Processing contact submision for e-mail {Email}", form.Email);

            if (!await _repository.CanInsertContactAsync(form.Email, token))
            {
                var message = "Zahtjev odbačen zbog sigurnosnih razloga. Pokušajte ponovo za minutu.";
                _logger.LogWarning("Contact submission blocked by anti-spam for e-mail {Email}", form.Email);

                return new ContactSubmissionResult
                {
                    Success = false,
                    Message = message
                };
            }

            var contact = new Contact
            {
                FirstName = form.FirstName,
                LastName = form.LastName,
                Email = form.Email,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                var apiUser = await _userApiService.FetchUserByEmailAsync(form.Email, token);
                if (apiUser is not null)
                {
                    EnrichContactWithApiData(contact, apiUser);
                    _logger.LogInformation("Contact enriched with API data for e-mail {Email}", form.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to enrich contact with API data for e-mail {Email}", contact.Email);
            }

            var contactId = await _repository.InsertContactAsync(contact, token);
            contact.Id = contactId;

            try
            {
                await _emailService.SendNotificationAsync(contact, token);
                _logger.LogInformation("E-mail notification sent for contact {ContactId}", contactId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send e-mail notification for contact {ContactId}", contactId);
            }

            return new ContactSubmissionResult
            {
                Success = true,
                Message = "Kontakt je uspješno unesen."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing contact submission for e-mail {Email}", form.Email);

            return new ContactSubmissionResult
            {
                Success = false,
                Message = "Došlo je do greške prilikom unosa kontakta. Molimo pokušajte ponovo."
            };
        }
    }

    public async Task<IEnumerable<Contact>> GetBizContactAsync(CancellationToken token)
    {
        return await _repository.GetBizEmailContactsAsync(token);
    }

    private void EnrichContactWithApiData(Contact contact, User apiUser)
    {
        contact.Phone = apiUser.Phone;
        contact.Website = apiUser.Website;

        if (apiUser.Address is not null)
        {
            contact.AddressStreet = apiUser.Address.Street;
            contact.AddressSuite = apiUser.Address.Suite;
            contact.AddressCity = apiUser.Address.City;
            contact.AddressZipCode = apiUser.Address.Zipcode;
            if(apiUser.Address.Geo is not null)
            {
                var style = NumberStyles.Number;
                var culture = CultureInfo.InvariantCulture;
                var converted = Decimal
                    .TryParse(apiUser.Address.Geo.Lat, style, culture, out decimal latitude);
                contact.AddressLatitude = converted ? latitude : null;
                converted = Decimal
                    .TryParse(apiUser.Address.Geo.Lng, style, culture, out decimal longitude);
                contact.AddressLongitude = converted ? longitude : null;
            }
        }

        if (apiUser.Company is not null)
        {
            contact.CompanyName = apiUser.Company.Name;
            contact.CompanyBs = apiUser.Company.Bs;
            contact.CompanyCatchPhrase = apiUser.Company.CatchPhrase;
        }
    }


}
