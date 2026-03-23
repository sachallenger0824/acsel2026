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

        [BindProperty]
        public string ExpectedCaptcha { get; set; } = string.Empty;

        [BindProperty]
        public string? CaptchaInput { get; set; }

        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public RegisterModel(AcselDbContext db, ILogger<RegisterModel> logger, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        private void GenerateCaptcha()
        {
            var random = new Random();
            int captchaCode = random.Next(1000, 10000);
            
            // Encode the answer so it's not plain text in the HTML source
            ExpectedCaptcha = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(captchaCode.ToString()));

            // Generate an SVG image
            string svg = $@"<svg xmlns='http://www.w3.org/2000/svg' width='120' height='40'>
                <rect width='100%' height='100%' fill='#eaeaea' rx='4' ry='4' />";

            // Add background noise lines
            for (int i = 0; i < 6; i++)
            {
                svg += $"<line x1='{random.Next(0, 120)}' y1='{random.Next(0, 40)}' x2='{random.Next(0, 120)}' y2='{random.Next(0, 40)}' stroke='#{random.Next(0x888888, 0xCCCCCC):X6}' stroke-width='2' />";
            }

            // Draw letters with slight random displacement and rotation
            string codeStr = captchaCode.ToString();
            for (int i = 0; i < codeStr.Length; i++)
            {
                int x = 15 + (i * 22) + random.Next(-2, 3);
                int y = 28 + random.Next(-3, 3);
                int rot = random.Next(-20, 20);
                svg += $"<text x='{x}' y='{y}' font-family='monospace' font-size='26' font-weight='800' fill='#333' transform='rotate({rot} {x} {y})'>{codeStr[i]}</text>";
            }
            svg += "</svg>";

            string base64Svg = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svg));
            ViewData["CaptchaImage"] = $"data:image/svg+xml;base64,{base64Svg}";
        }

        public void OnGet()
        {
            GenerateCaptcha();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Request.Form["agreement"] != "on")
            {
                ModelState.AddModelError("agreement", "You must agree to the terms and conditions.");
            }

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Some fields are missing or incorrect. Please review your entries and try again.";
                GenerateCaptcha();
                return Page();
            }

            string decodedExpected = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(ExpectedCaptcha))
                {
                    decodedExpected = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(ExpectedCaptcha));
                }
            }
            catch { }

            if (string.IsNullOrWhiteSpace(CaptchaInput) || CaptchaInput.Trim() != decodedExpected)
            {
                ModelState.AddModelError("CaptchaInput", "Image verification failed. Please try again.");
                ErrorMessage = "Image verification failed. Please try again.";
                GenerateCaptcha();
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
