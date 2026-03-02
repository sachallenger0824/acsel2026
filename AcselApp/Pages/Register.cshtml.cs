using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AcselApp.Data;
using AcselApp.Models;

namespace AcselApp.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly AcselDbContext _db;
        private readonly ILogger<RegisterModel> _logger;

        [BindProperty]
        public Registration Registration { get; set; } = new();

        public string? SuccessMessage { get; set; }

        public RegisterModel(AcselDbContext db, ILogger<RegisterModel> logger)
        {
            _db = db;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Registration.RegistrationDate = DateTime.Now;
            Registration.PaymentStatus = "Pending";

            _db.Registrations.Add(Registration);
            await _db.SaveChangesAsync();

            _logger.LogInformation("New registration saved: {Name}, {Email}, {TicketType}",
                Registration.FullName, Registration.Email, Registration.TicketType);

            SuccessMessage = "Registration submitted successfully! We will contact you shortly.";
            
            // Clear form
            Registration = new Registration();
            ModelState.Clear();

            return Page();
        }
    }
}
