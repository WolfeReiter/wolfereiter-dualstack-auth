using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WolfeReiter.Identity.DualStack.Models;

namespace WolfeReiter.Identity.DualStack.Controllers
{

    [Authorize]
    public class DiagnosticsController : Controller
    {
        private readonly ILogger<DiagnosticsController> _logger;

        public DiagnosticsController(ILogger<DiagnosticsController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
