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
            if(ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(LModel.Email);
                if (user != null)
                {
                    var identityResult = await signInManager.PasswordSignInAsync(user.UserName, LModel.Password, LModel.RememberMe, false);
                    if (identityResult.Succeeded)
                    {
                        HttpContext.Session.SetString("LoggedIn", "Success");
                        return RedirectToPage("Index");
                    }
                    ModelState.AddModelError(string.Empty, "Email or password is incorrect");
                }           
            }
            return Page();
        }
    }
}
