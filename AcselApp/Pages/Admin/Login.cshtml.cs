using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcselApp.Pages.Admin
{
    public class LoginModel : PageModel
    {
        private const string AdminPassword = "LHbwukeuk4C65f1y";
        private const string SessionKey = "AdminLoggedIn";

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // If already logged in, redirect to admin dashboard
            if (HttpContext.Session.GetString(SessionKey) == "true")
            {
                return RedirectToPage("/Admin/Index");
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            if (Password == AdminPassword)
            {
                HttpContext.Session.SetString(SessionKey, "true");
                return RedirectToPage("/Admin/Index");
            }

            ErrorMessage = "密碼錯誤，請重新輸入。";
            return Page();
        }
    }
}
