using System.Text.Json;
using System.Net.Http;
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
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public Registration Registration { get; set; } = new();

        public string? SuccessMessage { get; set; }

        public RegisterModel(AcselDbContext db, ILogger<RegisterModel> logger, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
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

            // Determine fee based on TicketType or other criteria
            string feeAmount = "15000"; // Default
            if (Registration.TicketType.Contains("Student", StringComparison.OrdinalIgnoreCase))
                feeAmount = "1500";
            else if (Registration.TicketType.Contains("Early", StringComparison.OrdinalIgnoreCase))
                feeAmount = "4000";
            else
                feeAmount = "4500";

            var client = _httpClientFactory.CreateClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", Registration.FullName),
                new KeyValuePair<string, string>("feeamount", feeAmount),
                new KeyValuePair<string, string>("memo", Registration.Email) // using email as memo for trackability if needed
            });

            try
            {
                var response = await client.PostAsync("https://cecntqb2.sce.pccu.edu.tw/acselRegister/order/add", content);
                
                // If the API returns a 302 redirect, HttpClient follows it by default, 
                // so the final URL will be in response.RequestMessage.RequestUri.
                // If it returns a string with the URL instead, we check the body.
                string responseBody = await response.Content.ReadAsStringAsync();
                string finalUri = response.RequestMessage?.RequestUri?.ToString() ?? "";

                if (finalUri.Contains("scepayment.sce.pccu.edu.tw"))
                {
                    Registration.PaymentLink = finalUri;
                }
                else if (responseBody.Contains("http"))
                {
                    Registration.PaymentLink = responseBody.Trim();
                }
                else if (response.Headers.Location != null)
                {
                    Registration.PaymentLink = response.Headers.Location.ToString();
                }
                else
                {
                    _logger.LogWarning("Unexpected payment API response. URI: {Uri}, Body: {Response}", finalUri, responseBody);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment order.");
            }

            _db.Registrations.Add(Registration);
            await _db.SaveChangesAsync();

            _logger.LogInformation("New registration saved: {Name}, {Email}, {TicketType}",
                Registration.FullName, Registration.Email, Registration.TicketType);

            if (!string.IsNullOrEmpty(Registration.PaymentLink) && Registration.PaymentLink.StartsWith("http"))
            {
                return Redirect(Registration.PaymentLink);
            }

            SuccessMessage = "Registration submitted successfully! We will contact you shortly.";
            
            // Clear form
            Registration = new Registration();
            ModelState.Clear();

            return Page();
        }
    }
}
