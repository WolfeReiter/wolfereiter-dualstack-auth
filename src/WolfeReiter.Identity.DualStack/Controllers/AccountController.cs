using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using WolfeReiter.Identity.Data;
using WolfeReiter.Identity.DualStack.Models;
using WolfeReiter.Identity.DualStack.Security;

namespace WolfeReiter.Identity.DualStack.Controllers
{
    [Authorize(/*Policy = Policies.Administration*/)]
    public class AccountController : Controller
    {
        private readonly IConfiguration Configuration;
        private readonly ILogger<AccountController> _logger;
        private readonly IDistributedCache Cache;
        private readonly CryptoService Crypto;
        private readonly SmtpClientService SmtpClient;

        private readonly SharedDbContext DbContext;

        int AccountLockoutSeconds => Configuration.GetValue<int>("Account:LockoutSeconds", 300);
        int AccountLockoutMaxFailedAttempts => Configuration.GetValue<int>("Account:LockoutMaxFailedAttempts", 5);
        int TokenValidMinutes => Configuration.GetValue<int>("Account:TokenValidMinutes", 60);
        bool EnableSelfEnrollment => Configuration.GetValue<bool>("Account:EnableSelfEnrollment", false);
        string ValidSelfEnrollmentEmailPattern => Configuration.GetValue<string>("Account:ValidSelfEnrollmentEmailPattern");

        public AccountController(IConfiguration configuration, ILogger<AccountController> logger, IDistributedCache cache, 
            CryptoService crypto, SmtpClientService smtpClient, SharedDbContext dbContext)
        {
            Configuration = configuration;
            _logger       = logger;
            Cache         = cache;
            DbContext     = dbContext;
            Crypto        = crypto;
            SmtpClient    = smtpClient;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity.IsAuthenticated) return  RedirectFromLogin(returnUrl);

            return View(new LoginViewModel { RedirectUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            ViewBag.EnableSelfEnrollment = EnableSelfEnrollment;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await DbContext.Users.Where(x => x.Name == model.Username).FirstOrDefaultAsync();
            if (user == null)
            {
                ModelState.AddModelError("", "Please register before attempting to log in.");
                ModelState.AddModelError("Username", $"\"{model.Username}\" is not registered.");
                return View(model);
            }

            if (!user.Active)
            {
                ModelState.AddModelError("", $"\"{model.Username}\" is disabled. Please contact the administrator.");
                ModelState.AddModelError("Username", $"\"{ model.Username}\" is disabled.");
                return View(model);
            }

            DateTime lastLoginAttempt, currentLoginAttempt;
            currentLoginAttempt   = DateTime.UtcNow;
            lastLoginAttempt      = user.LastLoginAttempt ?? DateTime.UtcNow;
            user.LastLoginAttempt = currentLoginAttempt;

            var lockoutErrorMsg = $"Too many failed login attempts. \"{model.Username}\" is temporarily locked.";
            if (user.Locked && (currentLoginAttempt - lastLoginAttempt) < TimeSpan.FromSeconds(AccountLockoutSeconds)) 
            {
                    ModelState.AddModelError("",lockoutErrorMsg);
                    //don't save user last login attempt time update or it will extend the lockout
                    return View(model);
            }

            if (model.Password != null && !PwdUtil.Verify(pwd: model.Password, hash: user.Hash, salt: user.Salt))
            {
                ModelState.AddModelError("Password", "Incorrect password.");
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= AccountLockoutMaxFailedAttempts) 
                {
                    ModelState.AddModelError("", lockoutErrorMsg);
                    user.Locked = true;
                }

                await DbContext.SaveChangesAsync();
                return View(model);
            }

            await SignInAsync(user);

            user.FailedLoginAttempts = 0;
            user.Locked              = false;

            await DbContext.SaveChangesAsync();
            return RedirectFromLogin(model.RedirectUrl);

        }

        async Task SignInAsync(Data.Models.User user)
        {
            var roles = await DbContext.UserRoles
                .Where(x => x.UserId == user.UserId)
                .Join(DbContext.Roles, ur => ur.RoleId, r => r.RoleId, (ur, r) => new
                {
                    r.RoleId,
                    r.Name
                })
                .ToListAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationMethod, "username/password database"),
                new Claim(ClaimTypes.Name, user.Name!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.GivenName, user.GivenName ?? string.Empty),
                new Claim(ClaimTypes.Surname, user.Surname ?? string.Empty),
                new Claim("UserId", user.UserId.ToString("N"))
            };
            claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x.Name!)));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Enroll()
        {
            if (!EnableSelfEnrollment) throw new NotSupportedException("Self-enrollment is disabled.");
            //TODO: Enroll()
            throw new NotImplementedException();
            //return View(new EnrollViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(EnrollViewModel model)
        {
            await Task.FromResult(0);
            throw new NotImplementedException();
        }

        IActionResult RedirectFromLogin(string? returnUrl = null)
        {
            returnUrl ??= "/";
            return LocalRedirect($"~{returnUrl}");
        }

        [AllowAnonymous]
        public IActionResult SignInMethod(string? returnUrl)
        {
            ViewBag.EnableSelfEnrollment = EnableSelfEnrollment;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// Replacement for Microsoft.Identity.Web.UI Account/Signout that clears groups from cache for User
        /// </summary>
        /// <param name="scheme"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("/Account/SignOut/")]
        public async Task<IActionResult> SignOut(string? scheme)
        {
            if (User != null && User.GetObjectId() != null)
            {
                await Cache.RemoveGroupClaimsAsync(User);
                scheme ??= OpenIdConnectDefaults.AuthenticationScheme;
                var callbackUrl = Url.ActionLink("SignedOut");
                return SignOut(
                    new AuthenticationProperties
                    {
                        RedirectUri = callbackUrl
                    },
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    scheme);
            }
            else
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return LocalRedirect("~/");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult SignedOut()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                return LocalRedirect("~/");
            }

            return View();
        }

    }
}
