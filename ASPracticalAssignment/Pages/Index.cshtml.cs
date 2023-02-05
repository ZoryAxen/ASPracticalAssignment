using ASPracticalAssignment.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ASPracticalAssignment.ViewModels;
using Microsoft.AspNetCore.DataProtection;

namespace ASPracticalAssignment.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public User MyUser = new();
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        public IndexModel(ILogger<IndexModel> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if(signInManager.IsSignedIn(User) && HttpContext.Session.GetString("LoggedIn") != null && HttpContext.Session.GetString("AuthToken") != null && Request.Cookies["AuthToken"] != null)
            {
                if (!HttpContext.Session.GetString("AuthToken").ToString().Equals(Request.Cookies["AuthToken"]))
                {
                    await signInManager.SignOutAsync();

                    HttpContext.Session.Remove("LoggedIn");
                    HttpContext.Session.Remove("AuthToken");

                    Response.Cookies.Delete("AuthToken");
                    return RedirectToPage("Login");
                }
                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("MySecretKey");
                var user = await userManager.GetUserAsync(User);
                if (user != null) 
                {
                    
                    MyUser.Username = user.UserName;
                    MyUser.FirstName = protector.Unprotect(user.FirstName);
                    MyUser.LastName = protector.Unprotect(user.LastName);
                    MyUser.Gender= protector.Unprotect(user.Gender);
                    MyUser.NRIC = protector.Unprotect(user.NRIC);
                    MyUser.DateOfBirth= user.DateOfBirth;
                    MyUser.Email = user.Email;
                    MyUser.WhoamI = protector.Unprotect(user.WhoamI);
                    MyUser.ResumeURL = user.ResumeURL;
                }
                return Page(); 
            }
            else
            {
                await signInManager.SignOutAsync();
                return RedirectToPage("Login");
            }
        }
    }
}