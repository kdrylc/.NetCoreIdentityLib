using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetCoreIdentity.DTO;
using NetCoreIdentity.EmailServices;
using NetCoreIdentity.Models.Identity;

namespace NetCoreIdentity.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _usermanager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<User> usermanager, SignInManager<User> signInManager, IEmailSender emailSender)
        {
            _usermanager = usermanager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModelDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // var user = await _userManager.FindByNameAsync(model.UserName);
            var user = await _usermanager.FindByNameAsync(model.UserName);

            if (user == null)
            {
                ModelState.AddModelError("", "Bu kullanıcı adı ile daha önce hesap oluşturulmamış");
                return View(model);
            }

            if (!await _usermanager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError("", "Lütfen email hesabınıza gelen link ile üyeliğinizi onaylayınız.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Girilen kullanıcı adı veya parola yanlış");
            return View(model);
        }
        
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        
        [HttpPost]
       
        public async  Task<IActionResult> Register(RegisterModelDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _usermanager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _usermanager.AddToRoleAsync(user, "User");
                // generate token
                var code = await _usermanager.GenerateEmailConfirmationTokenAsync(user);
                var url = Url.Action("ConfirmEmail", "Account", new
                {
                    userId = user.Id,
                    token = code
                });

                // email
                await _emailSender.SendEmailAsync(model.Email, "Hesabınızı onaylayınız.", $"Lütfen email hesabınızı onaylamak için linke <a href='https://localhost:7097{url}'>tıklayınız.</a>");
                return RedirectToAction("Login", "Account");
            }

            ModelState.AddModelError("", "Bilinmeyen hata oldu lütfen tekrar deneyiniz.");
            return View(model);
        }
        
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
        
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId==null || token==null)
            {
                TempData["message"] = "Geçersiz token.";
                return View();
            }
            var user = await _usermanager.FindByIdAsync(userId);
            if (user !=null)
            {
                var result = await _usermanager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    TempData["message"] = "Hesabınız onaylandı.";

                }
            }
            TempData["message"] = "Hesabınız onaylanmadı.";

            return View();
        }
        
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                return View();
            }
            var user = await _usermanager.FindByEmailAsync(Email);
            if (user == null)
            {
                return View();

            }
            var code = await _usermanager.GeneratePasswordResetTokenAsync(user);
            // generate token
            var url = Url.Action("ResetPassword", "Account", new
            {
                userId = user.Id,
                token = code
            });

            // email
            await _emailSender.SendEmailAsync(Email, "Reset Password.", $"Lütfen parolanızı yenilemek için linke <a href='https://localhost:7097{url}'>tıklayınız.</a>");
            return View();
        }
        
        public IActionResult ResetPassword(string userId, string token)
        {
            if (userId ==null || token==null)
            {
                return RedirectToAction("Index", "Home");
            }
            var model = new ResetPasswordModelDTO {Token = token};

            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModelDTO rpm)
        {
            if(!ModelState.IsValid)
            {
                return View(rpm);
            }
            var user =await _usermanager.FindByEmailAsync(rpm.Email);
            if (user==null)
            {
                return RedirectToAction("Index", "Home");
            }
            var result = await _usermanager.ResetPasswordAsync(user,rpm.Token,rpm.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("Login", "Account");
            }
            return View(rpm);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
