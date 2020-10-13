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
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using WolfeReiter.Identity.DualStack.Models;
using WolfeReiter.Identity.DualStack.Security;

namespace WolfeReiter.Identity.DualStack.Controllers
{
    [Authorize(/*Policy = Policies.Administration*/)]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IDistributedCache Cache;
        private readonly CryptoService Crypto;
        private readonly SmtpClientService SmtpClient;
        public AccountController(ILogger<AccountController> logger, IDistributedCache cache, CryptoService crypto, SmtpClientService smtpClient)
        {
            _logger    = logger;
            Cache      = cache;
            Crypto     = crypto;
            SmtpClient = smtpClient;
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
            
            //TODO: Implement local database sign-in.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationMethod, "database"),
                new Claim(ClaimTypes.Authentication, "Authenticated"),
                new Claim(ClaimTypes.Name, "Demo"),
                new Claim(ClaimTypes.Email, "brian.reiter@gmail.com"),
                new Claim(ClaimTypes.GivenName, "Demo User"),
                new Claim(ClaimTypes.Surname, "User"),
                new Claim(ClaimTypes.Sid, Guid.NewGuid().ToString("N"))
            };
            //claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x.Name)));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return RedirectFromLogin();
        }

        IActionResult RedirectFromLogin(string? returnUrl = null)
        {
            returnUrl ??= "/";
            return LocalRedirect($"~{returnUrl}");
        }

        public IActionResult SignInMethod(string? returnUrl)
        {
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
