using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using WolfeReiter.Identity.DualStack.Models;

namespace WolfeReiter.Identity.DualStack.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IDistributedCache Cache;
        public AccountController(ILogger<AccountController> logger, IDistributedCache cache)
        {
            _logger = logger;
            Cache   = cache;
        }

        public IActionResult SignIn()
        {
            //TODO: Implement local database sign-in.
            throw new NotImplementedException();
        }

        public IActionResult SignInMethod()
        {
            return View();
        }

        /// <summary>
        /// Replacement for Microsoft.Identity.Web.UI Account/Signout that clears groups from cache for User
        /// </summary>
        /// <param name="scheme"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/Account/SignOut/")]
        public async Task<IActionResult> SignOut(string scheme)
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

        [HttpGet]
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
