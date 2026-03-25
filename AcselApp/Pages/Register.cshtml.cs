using System.Text.Json;
using System.Net.Http;
using System.Net.Mail;
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
        private readonly IConfiguration _config;

        [BindProperty]
        public Registration Registration { get; set; } = new();

        [BindProperty]
        public string ExpectedCaptcha { get; set; } = string.Empty;

        [BindProperty]
        public string? CaptchaInput { get; set; }

        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public RegisterModel(AcselDbContext db, ILogger<RegisterModel> logger, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _db = db;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _config = config;
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

            try
            {
                var smtpHost = _config["Smtp:Host"] ?? "mta.pccu.edu.tw";
                var smtpPort = int.Parse(_config["Smtp:Port"] ?? "25");
                var fromAddr = _config["Smtp:From"] ?? "acsel2026@ulive.pccu.edu.tw";
                var toAddr = _config["Smtp:To"] ?? "acsel2026@office.pccu.edu.tw";

                _logger.LogInformation("Attempting to send registration email notification to {ToAddr} via {SmtpHost}:{SmtpPort}...", toAddr, smtpHost, smtpPort);

                using var mail = new MailMessage();
                mail.From = new MailAddress(fromAddr, "ACSEL 2026 Registration");
                mail.To.Add(toAddr);
                mail.Subject = $"[ACSEL 2026] New Registration – {Registration.FullName}";
                mail.IsBodyHtml = true;
                mail.Body = $@"
<html>
<body style='font-family:""Helvetica Neue"",Arial,sans-serif;max-width:720px;margin:0 auto;color:#1a1a2e;'>
  <div style='background:#1a1a2e;padding:1.5rem 2rem;border-radius:8px 8px 0 0;'>
    <h1 style='color:#fff;font-size:1.3rem;margin:0;'>ACSEL 2026 — New Registration</h1>
  </div>
  <div style='background:#f8f9fc;padding:2rem;border-radius:0 0 8px 8px;border:1px solid #e0e0e0;'>
    <p>A new registration has been received:</p>
    <ul>
      <li><strong>Name:</strong> {System.Net.WebUtility.HtmlEncode(Registration.FullName)}</li>
      <li><strong>Email:</strong> {System.Net.WebUtility.HtmlEncode(Registration.Email)}</li>
      <li><strong>Institution:</strong> {System.Net.WebUtility.HtmlEncode(Registration.Institution)}</li>
      <li><strong>Ticket Type:</strong> {System.Net.WebUtility.HtmlEncode(Registration.TicketType)}</li>
      <li><strong>Sightseeing Tour:</strong> {System.Net.WebUtility.HtmlEncode(Registration.SightseeingTour)}</li>
      <li><strong>Technical Tour:</strong> {System.Net.WebUtility.HtmlEncode(Registration.TechnicalTour)}</li>
    </ul>
  </div>
</body>
</html>";
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.SubjectEncoding = System.Text.Encoding.UTF8;

                using var smtp = new SmtpClient(smtpHost, smtpPort);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.EnableSsl = false;
                await smtp.SendMailAsync(mail);
                
                _logger.LogInformation("Registration email notification sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send registration email notification (ID={Id}). Exception Message: {Message}", Registration.Id, ex.Message);
            }

            if (!string.IsNullOrEmpty(Registration.PaymentLink) && Registration.PaymentLink.StartsWith("http"))
            {
                return Redirect(Registration.PaymentLink);
            }

            SuccessMessage = "Registration submitted successfully! We will contact you shortly.";
            
            // Clear form
            Registration = new Registration();
            ModelState.Clear();
            GenerateCaptcha();

            return Page();
        }
    }
}
