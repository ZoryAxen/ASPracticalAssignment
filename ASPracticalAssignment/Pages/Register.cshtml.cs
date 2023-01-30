using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ASPracticalAssignment.ViewModels;
using ASPracticalAssignment.Model;
using Microsoft.AspNetCore.DataProtection;

namespace ASPracticalAssignment.Pages
{
    public class RegisterModel : PageModel
    {
        private UserManager<ApplicationUser> userManager { get; }
        private SignInManager<ApplicationUser> signInManager { get; }
        private IWebHostEnvironment _environment;
        [BindProperty]
        public Register RModel { get; set; } = new();
        [BindProperty]
        public IFormFile? Resume { get; set; }

        public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IWebHostEnvironment _environment)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this._environment = _environment;
        }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync() 
        {
            if(ModelState.IsValid)
            {
                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("MySecretKey");


                var user = new ApplicationUser()
                {
                    UserName = RModel.Username,
                    Email = RModel.Email,
                    FirstName = protector.Protect(RModel.FirstName),
                    LastName = protector.Protect(RModel.LastName),
                    Gender = protector.Protect(RModel.Gender),
                    NRIC = protector.Protect(RModel.NRIC),
                    PasswordHash = RModel.Password,
                    DateOfBirth = RModel.DateOfBirth,
                    WhoamI = protector.Protect(RModel.WhoamI)
                };
                if (Resume != null)
                {
                    if (Resume.Length > 10 * 1024 * 1024)
                    {
                        ModelState.AddModelError("Resume", "File size cannot exceed 10mb");
                        return Page();
                    }
                    var uploadsFolder = "uploads";
                    var resumeFile = Guid.NewGuid() + Path.GetExtension(Resume.FileName);
                    var resumePath = Path.Combine(_environment.ContentRootPath, "wwwroot", uploadsFolder, resumeFile);
                    Directory.CreateDirectory(Path.GetDirectoryName(resumePath));
                    using var fileStream = new FileStream(resumePath, FileMode.Create);
                    await Resume.CopyToAsync(fileStream);
                    user.ResumeURL = string.Format("/{0}/{1}", uploadsFolder, resumeFile);
                }
                try
                {
                    var result = await userManager.CreateAsync(user, RModel.Password);
                    if (result.Succeeded)
                    {
                        await signInManager.SignInAsync(user, false);
                        return RedirectToPage("Index");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    if(ex.InnerException.Message.Contains("IX_"))
                    {
                        ModelState.AddModelError("Email", "Email Address ia already taken");
                    }
                    else
                    {
                        throw;
                    }
                }

            }
            return Page();
        }
    }
}
