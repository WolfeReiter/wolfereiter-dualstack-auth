using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WolfeReiter.Identity.DualStack.Models;

namespace WolfeReiter.Identity.DualStack.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public AccountController(ILogger<HomeController> logger)
        {
            _logger = logger;
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

    }
}
