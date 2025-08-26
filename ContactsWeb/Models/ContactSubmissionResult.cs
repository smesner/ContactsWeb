namespace ContactsWeb.Models;

public class ContactSubmissionResult
{
    public Contact? Contact { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
