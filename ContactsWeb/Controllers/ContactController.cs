using ContactsWeb.Models;
using ContactsWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContactsWeb.Controllers;
public class ContactController : Controller
{
    private readonly IContactService _contactService;
    private readonly ILogger<ContactController> _logger;

    public ContactController(IContactService contactService, ILogger<ContactController> logger)
    {
        _contactService = contactService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        _logger.LogInformation("Contact from page accessed from IP: {RemoteIpAddress}",
            HttpContext.Connection.RemoteIpAddress);

        return View(new ContactFormViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Index(ContactFormViewModel model, CancellationToken token)
    {
        _logger.LogInformation("Contact form submitted for e-mail: {Email}", model.Email);

        if (!ModelState.IsValid)
        {
            _logger.LogInformation("Invalid model state for contact form submission: {Email}", model.Email);
            
            return View(model);
        }

        var result = await _contactService.ProccessContactAsync(model, token);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            _logger.LogInformation("Contact form processed successufully: {Email}", model.Email);

            return RedirectToAction(nameof(Success));
        }
        else
        {
            ModelState.AddModelError(string.Empty, result.Message);
            _logger.LogWarning("Contact form processing failed: {Email}, Message: {Message}", model.Email, result.Message);
            
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Success()
    {
        var message = TempData["SuccessMessage"] as string ?? "Kontakt je uspješno poslan!";
        ViewBag.Message = message;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> BizContacts(CancellationToken token)
    {
        try
        {
            _logger.LogInformation("BizContacts view accessed from IP: {RemoteIpAddress}",
                HttpContext.Connection.RemoteIpAddress);

            var contacts = await _contactService.GetBizContactAsync(token);

            return View(contacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving .biz email contacts");
            TempData["ErrorMessage"] = "Greška pri dohvaćanju kontakata.";
            return View(new List<Contact>());
        }
    }
}
