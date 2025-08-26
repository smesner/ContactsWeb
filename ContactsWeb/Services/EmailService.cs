using ContactsWeb.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text;

namespace ContactsWeb.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendNotificationAsync(Contact contact, CancellationToken token)
    {
        try
        {
            var subject = $"Novi kontakt: {contact.FirstName} {contact.LastName}";
            var body = BuildEmailBody(contact);

            var message = new MimeMessage();
            var from = new MailboxAddress(_emailSettings.MailFrom.Name, _emailSettings.MailFrom.Address);
            message.From.Add(from);
            var to = new MailboxAddress(_emailSettings.MailTo.Name, _emailSettings.MailTo.Address);
            message.To.Add(to);
            message.Subject = subject;
            message.Body = body;

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, _emailSettings.EnableSsl, token);
            await smtp.SendAsync(message, token);
            await smtp.DisconnectAsync(true, token);
            _logger.LogInformation("E-mail notification for contact:{ContactEmail} sent successfully to {EmailName} <{EmailAddress}>",
                contact.Email, _emailSettings.MailTo.Name, _emailSettings.MailTo.Address);
        }
        catch (SmtpCommandException ex)
        {
            _logger.LogError(ex, "SMTP command error when tryed to send new contact information for {FirstName} {LastName}", contact.FirstName, contact.LastName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when tryed to send new contact information for {FirstName} {LastName}", contact.FirstName, contact.LastName);
        }
    }

    private MimeEntity BuildEmailBody(Contact contact)
    {
        //var createdDate = TimeZoneInfo.ConvertTimeFromUtc(contact.CreatedAt, TimeZoneInfo.Local).ToString("dd.MM.yyyy HH:mm:ss");
        var sb = new StringBuilder();
        sb.AppendLine("<h2>Novi konatakt iz web forme<h2>");
        sb.AppendLine($"<p><strong>Vrijeme unosa:</strong> {contact.CreatedAt.ToLocalTime().ToString()}</p>");
        sb.AppendLine("<h3>Osnovni podaci:</h3>");
        sb.AppendLine("<ul>");
        sb.AppendLine($"<li><strong>Ime:</strong> {contact.FirstName}</li>");
        sb.AppendLine($"<li><strong>Prezime:</strong> {contact.LastName}</li>");
        sb.AppendLine($"<li><strong>E-mail:</strong> {contact.Email}</li>");
        sb.AppendLine("</ul>");

        if (HasAdditionalData(contact))
        {
            sb.AppendLine("<h3>Dodatni podaci s vanjskog servisa:</h3>");
            sb.AppendLine("<ul>");
            if(!string.IsNullOrEmpty(contact.Phone))
                sb.AppendLine($"<li><strong>Telefon:</strong> {contact.Phone}</li>");
            if(!string.IsNullOrEmpty(contact.Website))
                sb.AppendLine($"<li><strong>Website:</strong> {contact.Website}</li>");

            if (HasAddressData(contact))
            {
                sb.AppendLine("<li><strong>Adresa:</strong></li><ul>");
                if (!string.IsNullOrEmpty(contact.AddressStreet))
                    sb.AppendLine($"<li>Ulica: {contact.AddressStreet}</li>");
                if (!string.IsNullOrEmpty(contact.AddressSuite))
                    sb.AppendLine($"<li>Broj: {contact.AddressSuite}</li>");
                if (!string.IsNullOrEmpty(contact.AddressCity))
                    sb.AppendLine($"<li>Grad: {contact.AddressCity}</li>");
                if (!string.IsNullOrEmpty(contact.AddressZipCode))
                    sb.AppendLine($"<li>Poštanski broj: {contact.AddressZipCode}</li>");
                if (contact.AddressLatitude is not null && contact.AddressLongitude is not null)
                    sb.AppendLine($"<li>GPS lokacija (lat,long): {contact.AddressLatitude},{contact.AddressLongitude}</li>");
                sb.AppendLine("</ul>");
            }
            if (!string.IsNullOrEmpty(contact.CompanyName))
            {
                sb.AppendLine("<li><strong>Tvrtka:</strong></li><ul>");
                sb.AppendLine($"<li>Naziv: {contact.CompanyName}</li>");
                if (!string.IsNullOrEmpty(contact.CompanyBs))
                    sb.AppendLine($"<li>Broj: {contact.CompanyBs}</li>");
                if (!string.IsNullOrEmpty(contact.AddressCity))
                    sb.AppendLine($"<li>Djelatnost: {contact.CompanyCatchPhrase}</li>");
                sb.AppendLine("</ul>");
            }
            sb.AppendLine("</ul>");
        }
        else
        {
            sb.AppendLine("<p><em>Nema dodatnih podataka iz vanjskog servisa.</em></p>");
        }

        var bb = new BodyBuilder();
        bb.HtmlBody = sb.ToString();

        return bb.ToMessageBody();
    }

    private bool HasAddressData(Contact contact)
    {
        return !string.IsNullOrEmpty(contact.AddressStreet) ||
            !string.IsNullOrEmpty(contact.AddressSuite) ||
            !string.IsNullOrEmpty(contact.AddressCity) ||
            !string.IsNullOrEmpty(contact.AddressZipCode) ||
            contact.AddressLatitude is not null || 
            contact.AddressCity is not null;
    }

    private bool HasAdditionalData(Contact contact)
    {
        return !string.IsNullOrEmpty(contact.Phone) ||
            !string.IsNullOrEmpty(contact.Website) ||
            !string.IsNullOrEmpty(contact.CompanyName) ||
            HasAddressData(contact);
    }
}
