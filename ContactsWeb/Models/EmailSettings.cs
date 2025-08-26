namespace ContactsWeb.Models;

public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public EmailFromTo MailFrom { get; set; } = new();
    public EmailFromTo MailTo { get; set; } = new();
    public bool EnableSsl { get; set; } = true;
}

public class EmailFromTo
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
