using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MimeKit;
using WolfeReiter.Identity.DualStack.Models;

namespace WolfeReiter.Identity.DualStack.Controllers
{

    [Authorize]
    public class DiagnosticsController : Controller
    {
        private readonly ILogger<DiagnosticsController> _logger;

        readonly SmtpClientService SmtpClient;

        public DiagnosticsController(ILogger<DiagnosticsController> logger, SmtpClientService smtpClient)
        {
            _logger = logger;
            SmtpClient = smtpClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        //test that SMTP is configured
        //https://localhost:5001/Diagnostics/Mail/youremail@domain.com
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Mail(string id)
        {
            string email = id;

            var message = new MimeMessage();
            var builder = new BodyBuilder
            {
                TextBody = "Hello. It works."
            };
            message.Body = builder.ToMessageBody();
            message.Subject = "Test email";
            message.To.Add(new MailboxAddress(name: null, address: email));
            message.From.Add(SmtpClient.SystemFromAddress);
            try
            {
                await SmtpClient.SendMessageAsync(message);
                return Content("OK. Check your inbox.");
            }
            catch (Exception ex)
            {
                return Content($"{ex.Message}\r\n\r\n{ex.StackTrace}");
            }
        }

        [Authorize(Roles = "85de0d3fb8f54ebb94e9cc6747545271")] //fake role
        public IActionResult Denied()
        {
            return Content("You should not see this message.");
        }
    }
}
