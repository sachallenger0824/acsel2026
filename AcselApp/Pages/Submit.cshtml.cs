using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AcselApp.Data;
using AcselApp.Models;
using System.Net.Mail;

namespace AcselApp.Pages
{
    public class SubmitModel : PageModel
    {
        private readonly AcselDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<SubmitModel> _logger;

        [BindProperty]
        public AbstractSubmission Submission { get; set; } = new();

        [BindProperty]
        public string ExpectedCaptcha { get; set; } = string.Empty;

        [BindProperty]
        public string? CaptchaInput { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public SubmitModel(AcselDbContext db, IConfiguration config, ILogger<SubmitModel> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
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

            Submission.SubmittedAt = DateTime.Now;
            Submission.Status = "Pending";

            _db.AbstractSubmissions.Add(Submission);
            await _db.SaveChangesAsync();

            try
            {
                var smtpHost = _config["Smtp:Host"] ?? "mta.pccu.edu.tw";
                var smtpPort = int.Parse(_config["Smtp:Port"] ?? "25");
                var fromAddr = _config["Smtp:From"] ?? "acsel2026@ulive.pccu.edu.tw";
                //測試用改信箱
                var toAddr = _config["Smtp:To"] ?? "acsel2026@office.pccu.edu.tw";

                _logger.LogInformation("Attempting to send abstract submission email to {ToAddr} via {SmtpHost}:{SmtpPort}...", toAddr, smtpHost, smtpPort);
   
                using var mail = new MailMessage();
                mail.From = new MailAddress(fromAddr, "ACSEL 2026 Abstract System");
                mail.To.Add(toAddr);
                mail.Subject = $"[ACSEL 2026] New Abstract Submission – {Submission.Title}";
                mail.IsBodyHtml = true;
                mail.Body = BuildEmailBody(Submission);
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.SubjectEncoding = System.Text.Encoding.UTF8;

                using var smtp = new SmtpClient(smtpHost, smtpPort);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.EnableSsl = false;
                await smtp.SendMailAsync(mail);

                _logger.LogInformation("Abstract submission email sent successfully for ID {Id}.", Submission.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send abstract submission notification email (ID={Id}). Exception Message: {Message}", Submission.Id, ex.Message);
            }

            SuccessMessage = "Your abstract has been submitted successfully! We will contact you at the provided email address.";
            Submission = new AbstractSubmission();
            ModelState.Clear();
            GenerateCaptcha();
            return Page();
        }

        private static string BuildEmailBody(AbstractSubmission s) => $@"
<html>
<body style=""font-family:'Helvetica Neue',Arial,sans-serif;max-width:720px;margin:0 auto;color:#1a1a2e;"">
  <div style=""background:#1a1a2e;padding:1.5rem 2rem;border-radius:8px 8px 0 0;"">
    <h1 style=""color:#fff;font-size:1.3rem;margin:0;"">ACSEL 2026 — New Abstract Submission</h1>
  </div>
  <div style=""background:#f8f9fc;padding:2rem;border-radius:0 0 8px 8px;border:1px solid #e0e0e0;"">
    <table style=""width:100%;border-collapse:collapse;font-size:0.93rem;"">
      <tr><td style=""padding:8px 12px;background:#eef0f8;font-weight:600;width:200px;border-bottom:1px solid #d0d4e8;"">Submission #</td><td style=""padding:8px 12px;border-bottom:1px solid #e8e8e8;"">{s.Id}</td></tr>
      <tr><td style=""padding:8px 12px;background:#eef0f8;font-weight:600;border-bottom:1px solid #d0d4e8;"">Presentation Type</td><td style=""padding:8px 12px;border-bottom:1px solid #e8e8e8;"">{System.Net.WebUtility.HtmlEncode(s.PresentationType)}</td></tr>
      <tr><td style=""padding:8px 12px;background:#eef0f8;font-weight:600;border-bottom:1px solid #d0d4e8;"">Title</td><td style=""padding:8px 12px;border-bottom:1px solid #e8e8e8;""><strong>{System.Net.WebUtility.HtmlEncode(s.Title)}</strong></td></tr>
      <tr><td style=""padding:8px 12px;background:#eef0f8;font-weight:600;border-bottom:1px solid #d0d4e8;"">Authors</td><td style=""padding:8px 12px;border-bottom:1px solid #e8e8e8;"">{System.Net.WebUtility.HtmlEncode(s.Authors)}</td></tr>
      <tr><td style=""padding:8px 12px;background:#eef0f8;font-weight:600;border-bottom:1px solid #d0d4e8;"">Affiliations</td><td style=""padding:8px 12px;border-bottom:1px solid #e8e8e8;white-space:pre-wrap;"">{System.Net.WebUtility.HtmlEncode(s.Affiliations ?? "—")}</td></tr>
      <tr><td style=""padding:8px 12px;background:#eef0f8;font-weight:600;border-bottom:1px solid #d0d4e8;"">Corresponding Author</td><td style=""padding:8px 12px;border-bottom:1px solid #e8e8e8;"">{System.Net.WebUtility.HtmlEncode(s.CorrespondingAuthor)}</td></tr>
      <tr><td style=""padding:8px 12px;background:#eef0f8;font-weight:600;border-bottom:1px solid #d0d4e8;"">Corresponding Email</td><td style=""padding:8px 12px;border-bottom:1px solid #e8e8e8;""><a href=""mailto:{System.Net.WebUtility.HtmlEncode(s.CorrespondingEmail)}"">{System.Net.WebUtility.HtmlEncode(s.CorrespondingEmail)}</a></td></tr>
      <tr><td style=""padding:8px 12px;background:#eef0f8;font-weight:600;border-bottom:1px solid #d0d4e8;"">Keywords</td><td style=""padding:8px 12px;border-bottom:1px solid #e8e8e8;"">{System.Net.WebUtility.HtmlEncode(s.Keywords ?? "—")}</td></tr>
      <tr><td style=""padding:8px 12px;background:#eef0f8;font-weight:600;"">Submitted At</td><td style=""padding:8px 12px;"">{s.SubmittedAt:yyyy-MM-dd HH:mm} (UTC+8)</td></tr>
    </table>
    <h3 style=""margin:1.5rem 0 0.5rem;font-size:1rem;"">Abstract</h3>
    <div style=""background:#fff;padding:1rem 1.25rem;border-radius:6px;border:1px solid #d8dae0;white-space:pre-wrap;font-size:0.93rem;line-height:1.7;"">{System.Net.WebUtility.HtmlEncode(s.AbstractText)}</div>
  </div>
</body>
</html>";
    }
}
