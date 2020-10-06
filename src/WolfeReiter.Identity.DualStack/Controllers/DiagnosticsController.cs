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
    public class DiagnosticsController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public DiagnosticsController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            //TODO: Implement Diagnostics Index view to list out claims
            throw new NotImplementedException();
            //return View();
        }
    }
}
