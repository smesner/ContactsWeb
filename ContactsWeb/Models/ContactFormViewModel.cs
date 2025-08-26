using System.ComponentModel.DataAnnotations;

namespace ContactsWeb.Models;

public class ContactFormViewModel
{
    [Required(ErrorMessage = "Ime je obavezno")]
    [Display(Name = "Ime")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Prezime je obavezno")]
    [Display(Name = "Prezime")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail je obavezan")]
    [EmailAddress(ErrorMessage = "Neispravna E-mail adresa")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}
