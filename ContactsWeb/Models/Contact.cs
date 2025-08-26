using System.ComponentModel.DataAnnotations;

namespace ContactsWeb.Models;

public class Contact
{
    public int Id { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100, ErrorMessage = "First name can't exceed 100 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(100, ErrorMessage = "Last name can't exceed 100 characters.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail address is required.")]
    [EmailAddress(ErrorMessage = "Incorrect e-mail address format.")]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? AddressStreet { get; set; }
    public string? AddressSuite { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressZipCode { get; set; }
    public decimal? AddressLatitude { get; set; } = null;
    public decimal? AddressLongitude { get; set; } = null;
    public string? CompanyName { get; set; }
    public string? CompanyBs { get; set; }
    public string? CompanyCatchPhrase { get; set; }

    public DateTime CreatedAt { get; set; }
}
