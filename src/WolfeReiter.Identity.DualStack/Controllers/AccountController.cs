using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
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
using MimeKit;
using WolfeReiter.Identity.Data;
using WolfeReiter.Identity.DualStack.Models;
using WolfeReiter.Identity.DualStack.Security;

namespace WolfeReiter.Identity.DualStack.Controllers
{
    [Authorize(/*Policy = Policies.Administration*/)]
    public class AccountController : Controller
    {
        private readonly IConfiguration Configuration;
        private readonly ILogger<AccountController> Logger;
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
            CryptoService crypto, SmtpClientService smtpClient, PgSqlContext pgContext, SqlServerContext sqlContext)
        {
            Configuration = configuration;
            Logger        = logger;
            Cache         = cache;
            Crypto        = crypto;
            SmtpClient    = smtpClient;

            DbContext = (configuration.GetValue<string>("EntityFramework:Driver")) switch
            {
                "PostgreSql" => pgContext,
                "SqlServer" => sqlContext,
                _ => throw new InvalidOperationException("The EntityFramework:Driver configuration value must be set to \"PostgreSql\" or \"SqlServer\"."),
            };
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Forgot()
        {
            return View(new ForgotViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Forgot(ForgotViewModel model)
        {
            //validate username exists
            //validate user is active
            //encode reset token
            //mail reset token

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

            var token = EncodeResetToken(user);
            var cryptoken = EncryptToken(token);

            var resetLink = Url.ActionLink("Reset", "Account", new { id = cryptoken });

            var message = new MimeMessage();
            message.From.Add(SmtpClient.SystemFromAddress);
            message.To.Add(new MailboxAddress((string?)null, user.Email));
            message.Subject = "Password Reset Requested";

            var builder = new BodyBuilder
            {
                //TODO: message tempate as txt and html part resource
                TextBody = "Follow the link below to reset your password in the TBD system. " +
                $"This reset link will expire {TokenValidMinutes} minutes after you requested it. " +
                "If you do not wish to reset your password, you can safely ignore this message.\n\n" +
                $"<{resetLink}>",

                HtmlBody = "<p>The link below will allow you to reset your password in the TBD system.</p>" +
                $"<p><a href={resetLink}>Follow this link to reset your password. It will " +
                $"expire {TokenValidMinutes} minutes after you requested it.</a></p>" +
                "<p>If you do not wish to reset your password, you can safely ignore this message.</p>" 
            };

            message.Body = builder.ToMessageBody();

            try
            {
                await SmtpClient.SendMessageAsync(message);
                ViewBag.Message = $"We have sent you an email from {SmtpClient.SystemFromAddress.Address} that contains a link to reset your email. " +
                    $"The link will expire in {TokenValidMinutes} minutes.";
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error sending email.");
                ModelState.AddModelError("", e.Message);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Reset(string id)
        {
            //id is an encrypted token
            //token not useable, start over
            if (!(await ValidateResetTokenAsync(id)).success) return View("Forgot", new ForgotViewModel());

            //otherwise the token is OK.
            //user is allowed to reset password.
            return View(new ResetViewModel() { Token = id });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset(ResetViewModel model)
        {
            //token not useable, start over
            var result = await ValidateResetTokenAsync(model.Token);
            if (!result.success) return View("Forgot", new ForgotViewModel());
            if (!ModelState.IsValid) return View(model);

            var user = result.user!; //result.user cannot be null when result.success == true
            var salt = PwdUtil.NewSalt();
            var hash = PwdUtil.Hash(model.Password!, salt); //model.Password cannot be null if ModelState.IsValid == true
            user.Hash = hash;
            user.Salt = salt;

            await DbContext.SaveChangesAsync();
            await SignInAsync(user);

            return RedirectFromLogin();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Enroll()
        {
            if (!EnableSelfEnrollment) throw new NotSupportedException("Self-enrollment is disabled.");

            return View(new EnrollViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(EnrollViewModel model)
        {
            if (!EnableSelfEnrollment) throw new NotSupportedException("Self-enrollment is disabled.");

            if (!ModelState.IsValid) return View(model);
            if (!string.IsNullOrEmpty(ValidSelfEnrollmentEmailPattern) && !Regex.IsMatch(model.Email, ValidSelfEnrollmentEmailPattern))
            {
                ModelState.AddModelError("Email", $"Not permitted.");
                ModelState.AddModelError("", $"\"{model.Email}\" is not permitted to register. You must use a pre-authorized address. Please contact the system administrator.");
                return View(model);
            }

            //create token from email, timestamp
            //encrypt token
            var token = EncodeEnrollmentToken(model.Email!); //model.Email cannot be null if ModelState.IsValid == true
            var cryptoken = EncryptToken(token);

            //send mail loop confirmation
            var enrollmentLink = Url.ActionLink("Confirm", "Account", new { id = cryptoken });

            var message = new MimeMessage();
            message.From.Add(SmtpClient.SystemFromAddress);
            message.To.Add(new MailboxAddress((string?)null, model.Email));
            message.Subject = "Account Enrollment Requested";

            var builder = new BodyBuilder
            {
                //TODO: message tempate as txt and html part resource
                TextBody = "Follow the link below to create your new account in the  TBD system. " +
                $"This link will expire {TokenValidMinutes} minutes after you requested it. " +
                "If you do not wish to reset your password, you can safely ignore this message.\n\n" +
                $"<{enrollmentLink}>",

                HtmlBody = "<p>Follow the link below to  create your new account in the TBD system.</p>" +
                $"<p><a href={enrollmentLink}>This link will allow you to finish creating your account. It will " +
                $"expire {TokenValidMinutes} minutes after you requested it.</a></p>" +
                "<p>If you do not wish to reset your password, you can safely ignore this message.</p>"
            };

            message.Body = builder.ToMessageBody();

            try
            {
                await SmtpClient.SendMessageAsync(message);
                ViewBag.Message = $"We have sent you an email from {SmtpClient.SystemFromAddress.Address} that contains a link to create your new account. " +
                    $"The link will expire in {TokenValidMinutes} minutes.";
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error sending email.");
                ModelState.AddModelError("", e.Message);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Confirm(string id)
        {
            if (!EnableSelfEnrollment) throw new NotSupportedException("Self-enrollment is disabled.");

            //id is an encrypted token
            //valid token must: 
            //1. decrypt successfully
            //2. has not expired
            //
            //then allow loading the view with the token in the model
            //id is an encrypted token
            //token not useable, start over
            if (!ValidateEnrollmentToken(id, out string? email)) return View("Enroll", new EnrollViewModel());

            //otherwise the token is OK.
            //user is allowed to create an account.
            //default username is email.
            return View(new ConfirmEnrollViewModel() { Token = id, Email = email, Username = email });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(ConfirmEnrollViewModel model)
        {
            if (!EnableSelfEnrollment) throw new NotSupportedException("Self-enrollment is disabled.");

            //Model.Token is an encrypted token
            //valid token must: 
            //1. decrypt successfully
            //2. has not expired
            //
            //then allow creating a new Data.Models.User entity.

            //token not useable, start over
            if (string.IsNullOrEmpty(model.Token) || !ValidateEnrollmentToken(model.Token, out string? email)) 
            { return View("Enroll", new EnrollViewModel()); }
            
            if (!ModelState.IsValid) return View(model);

            //test if requested username exists
            if (DbContext.Users.Where(x => x.Name == model.Username).Any())
            {
                ModelState.AddModelError("", $"\"{model.Username}\" is already in use.");
                ModelState.AddModelError("", $"\"{model.Username}\" is already in use. Please select something else. Your username does not have to match your email address.");
                return View(model);
            }
            //create user and log in
            var salt = PwdUtil.NewSalt();
            var hash = PwdUtil.Hash(model.Password!, salt); //model.Password can't be null if ModelState.IsValid == true
            var user = new Data.Models.User()
            {
                Name = model.Username,
                Email = email,
                Hash = hash,
                Salt = salt
            };

            try
            {
                DbContext.Add(user);
                await DbContext.SaveChangesAsync();
                await SignInAsync(user);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to create user: {model.Username}.");
                ModelState.AddModelError("", $"Unable to create user: {model.Username}.");
                ModelState.AddModelError("", e.Message);
                return View(model);
            }
            return RedirectFromLogin(null);
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
                new Claim(ClaimTypes.AuthenticationMethod, "database"),
                new Claim(ClaimTypes.Name, user.Name!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.GivenName, user.GivenName ?? string.Empty),
                new Claim(ClaimTypes.Surname, user.Surname ?? string.Empty),
                new Claim("UserId", user.UserId.ToString("N")),
                new Claim("UserNumber", user.UserNumber.ToString())
            };
            claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x.Name!)));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }

        async Task<(bool success, Data.Models.User? user)> ValidateResetTokenAsync(string? encrypted)
        {
            //valid token must: 
            //1. decrypt successfully
            //2. valid userId
            //3. truncated hash of User.Hash must match the database version
            //4. has not expired
            (bool success, Data.Models.User? user) result = (false, null);
            
            if (string.IsNullOrEmpty(encrypted)) 
            {
                return result;
            }
            else if (!DecryptToken(encrypted, out string? tok) || !TryDecodeResetToken(tok, out (Guid userId, string? hash, DateTime timestamp) token))
            {
                //token cannot be read
                ModelState.AddModelError("", "The token in the password reset link was bad. Please send yourself a new reset email and try again.");
            }
            else if (DateTime.UtcNow > token.timestamp.AddMinutes(TokenValidMinutes))
            {
                //check token timeout
                ModelState.AddModelError("", "Your reset link has expired. Please send yourself a new reset email and try again.");
            }
            else
            {
                //check token vs. user record
                result.user = await DbContext.Users.Where(x => x.UserId == token.userId).FirstOrDefaultAsync();
                if (result.user == null)
                {
                    ModelState.AddModelError("", $"\"{token.userId} has been deleted. Please contact the system administrator.");
                }
                else if (!result.user.Active)
                {
                    ModelState.AddModelError("", $"\"{result.user.Name} is disabled. Please contact the system administrator.");
                }
                else if (TruncateToBase64(result.user.Hash) != token.hash)
                {
                    ModelState.AddModelError("", "Your password has already been reset. Please send yourself a new reset email and try again.");
                }
                result.success = true;
            }

            return result;
        }

        bool ValidateEnrollmentToken(string encrypted, out string? email)
        {
            //valid token must: 
            //1. decrypt successfully
            //2. has not expired
            bool valid = true;
            email = null;

            if (!DecryptToken(encrypted, out string? tok) || !TryDecodeEnrollmentToken(tok, out (string? email, DateTime timestamp) token))
            {
                //token cannot be read
                ModelState.AddModelError("", "The token in the account enrollment link was bad. Please send yourself another account confirmation email and try again.");
                valid = false;
            }
            else if (DateTime.UtcNow > token.timestamp.AddMinutes(TokenValidMinutes))
            {
                //check token timeout
                ModelState.AddModelError("", "Your account enrollment link has expired. Please send yourself another account confirmation email and try again.");
                valid = false;
            }
            else
            {
                email = token.email;
            }

            return valid;
        }

        string EncodeResetToken(Data.Models.User user)
        {
            //token format: "userId:hash:timestamp"
            //userId for reliable account identification
            //truncated password hash string for anti-replay
            //timestamp UTC for expiration
            var hash = TruncateToBase64(user.Hash);
            return $"{user.UserId:N}:{hash}:{DateTime.UtcNow}";
        }

        string EncodeEnrollmentToken(string email)
        {
            //token format: "email:timestamp"
            //timestamp UTC for expiration

            return $"{email}:{DateTime.UtcNow}";
        }

        string TruncateToBase64(byte[] hash)
        {
            return Convert.ToBase64String(hash).Substring(0, 10);
        }

        bool TryDecodeResetToken(string? token, out (Guid userId, string? hash, DateTime timestamp) result)
        {
            result = (Guid.Empty, null, DateTime.MinValue);
            if (string.IsNullOrEmpty(token)) return false;
            var split = token.Split(":", 3);
            if (split.Length != 3) return false;
            if (!Guid.TryParse(split[0], out Guid userId)) return false;
            string hash = split[1];
            if (!DateTime.TryParse(split[2], out DateTime timestamp)) return false;
            result = (userId, hash, timestamp);
            return true;
        }

        bool TryDecodeEnrollmentToken(string? token, out (string? email, DateTime timestamp) result)
        {
            result = (null, DateTime.MinValue);
            if (string.IsNullOrEmpty(token)) return false;
            var split = token.Split(":", 2);
            if (split.Length != 2) return false;
            if (!DateTime.TryParse(split[1], out DateTime timestamp)) return false;
            result = (split[0], timestamp);
            return true;
        }

        string EncryptToken(string token)
        {
            var encrypted = Crypto.Encrypt(token);
            var escaped = Uri.EscapeDataString(encrypted);
            return escaped;
        }

        bool DecryptToken(string escaped, out string? plaintext)
        {
            var encrypted = Uri.UnescapeDataString(escaped);
            try
            {
                return Crypto.DecryptAndVerify(encrypted, out plaintext);
            }
            catch
            {
                plaintext = null;
                return false;
            }
        }

    }
}
