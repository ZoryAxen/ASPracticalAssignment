using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ASPracticalAssignment.ViewModels;
using Microsoft.AspNetCore.Identity;
using ASPracticalAssignment.Model;

namespace ASPracticalAssignment.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(LModel.Email);
                if (user != null)
                {
                    if (!await userManager.IsLockedOutAsync(user))
                    {
                        if (signInManager.IsSignedIn(User))
                        {
                            await signInManager.SignOutAsync();
                            HttpContext.Session.Remove("LoggedIn");
                            HttpContext.Session.Remove("AuthToken");
                        }
                        var identityResult = await signInManager.PasswordSignInAsync(user.UserName, LModel.Password, LModel.RememberMe, false);
                        if (identityResult.Succeeded)
                        {
                            string guid = Guid.NewGuid().ToString();

                            HttpContext.Session.SetString("LoggedIn", user.UserName);
                            HttpContext.Session.SetString("AuthToken", guid);

                            Response.Cookies.Append("AuthToken", guid, new CookieOptions
                            {
                                Expires = DateTime.Now.AddMinutes(30)
                            });

                            await userManager.ResetAccessFailedCountAsync(user);

                            return RedirectToPage("Index");
                        }
                        ModelState.AddModelError(string.Empty, "Email or password is incorrect");

                        //Set lockout increment
                        await userManager.AccessFailedAsync(user);
                        if (await userManager.GetAccessFailedCountAsync(user) >= 3)
                        {
                            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                            return Page();
                        }

                    }
                    ModelState.AddModelError(string.Empty, "Account has been locked out due to too many failed login attempts");                    
                }
            }
            return Page();
        }
    }
}
