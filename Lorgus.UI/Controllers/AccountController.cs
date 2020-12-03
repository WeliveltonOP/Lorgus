using Lorgus.UI.Models;
using Lorgus.UI.Services.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Lorgus.UI.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public AccountController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Admin");

            return View();
        }

        [HttpPost]
        public IActionResult Login([Bind] LoginViewModel model, [FromServices] LorgusContext context)
        {
            if (ModelState.IsValid)
            {
                var user = context.User.Include(u => u.AccessLevel).FirstOrDefault(u => u.Email.ToLower() == model.Email.ToLower());

                if (user is null)
                {
                    ViewData["messageError"] = "Usuário não cadastrado!";

                    return View(model);
                }

                if (!VerifyPasswordHash(model.Password, user.PasswordHash, user.PasswordSalt))
                {
                    ViewData["messageError"] = "E-mail ou senha incorretos!";

                    return View(model);
                }

                
                var authProperties = new AuthenticationProperties
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(4),
                    RedirectUri = Url.Action("Index", "Admin")
                };

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, model.Email),
                    new Claim(ClaimTypes.Role,  user.AccessLevel.Description.ToLower()),
                    
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties).Wait();
               
                return RedirectToAction("Index", "Admin");
            }

            return View(model);
        }

        public IActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RecoverPassword(string email, [FromServices] LorgusContext context, [FromServices] IEmailSender emailSender)
        {
            var user = context.User.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

            var emailTitle = "Recuperação de Senha";

            if (user is null)
            {
                ViewData["messageError"] = "E-mail não encontrado!";

                return View();
            }

            var request = new RequestChangePassword
            {
                Date = DateTime.Now,
                UserId = user.UserId,
                RequestChangePasswordId = Guid.NewGuid().ToString()
            };

            var requestPassword = context.RequestChangePassword.Add(request);

            context.SaveChanges();

            var attachments = new List<Attachment>
            {
                new Attachment($"{_env.WebRootPath}/images/logo.png")
                {
                    ContentId = "Logo"
                }
            };

           var replacements = new Dictionary<string, string>
                {
                    { "{{name}}", user.FullName },
                    { "{{action_url}}", Url.Action("ResetPassword", "Account", new { id = request.RequestChangePasswordId }, Request.Scheme) },
                    { "{{logo}}", "Logo" },
                    { "{{date}}", DateTime.UtcNow.AddHours(-3).ToString("dd/MM/yyyy HH:mm:ss") },
                };

            var to = new MailAddress(user.Email, user.FullName);

            var body = System.IO.File.ReadAllText($"Templates/EmailRecoveryPasswordTemplate.html");

            try
            {                
                emailSender.Send(to, body, emailTitle, replacements, attachments);

                TempData["messageSuccess"] = "Recuperação enviada com sucesso!";

            }
            catch (Exception)
            {
                TempData["messageError"] = "Não foi possível enviar a Recuperação!";
            }

            return RedirectToAction("Login");
        }

        public IActionResult ResetPassword(Guid id, [FromServices] LorgusContext context)
        {
            ViewData["messageError"] = TempData["messageError"];

            var requestParams = context.RequestChangePassword.FirstOrDefault(r => r.RequestChangePasswordId == id.ToString());

            var hours = default(int);

            if (requestParams != null)
                hours = (DateTime.Now - requestParams.Date).Value.Hours;

            if (hours >= 24 || requestParams == null)
            {
                return Json("Desculpe, este link já expirou !!!");
            }

            var values = new ResetPasswordViewModel
            {
                RequestChangePasswordId = requestParams.RequestChangePasswordId,
                Date = requestParams.Date,
                UserId = requestParams.UserId
            };

            return View(values);
        }

        [HttpPost]
        public IActionResult ResetPassword(Guid id, [Bind] ResetPasswordViewModel model, [FromServices] LorgusContext context)
        {
            try
            {
                if (model.Password1 != model.Password2)
                {
                    ViewData["messageError"] = "Senhas não conferem.";

                    return View(model);
                }

                if (id == default)
                    return NotFound();

                var password = model.Password1;

                var request = context.RequestChangePassword.Include(u => u.User).FirstOrDefault(r => r.RequestChangePasswordId == id.ToString());

                var (hash, salt) = GenerateHMACSHA512Hash(model.Password1);

                request.User.PasswordHash = hash;

                request.User.PasswordSalt = salt;

                context.Update(request.User);

                context.Remove(request);

                context.SaveChanges();

                return RedirectToAction("Login", "Account");

            }
            catch (Exception)
            {
                TempData["messageError"] = "Desculpe, ocorreu um erro ao alterar sua senha!";

                return RedirectToAction("ResetPassword", "Account", new { id });
            }
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();

            return RedirectToAction("Login");
        }

        public static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                return computedHash.SequenceEqual(storedHash);
            }
        }

        public static (byte[] hash, byte[] salt) GenerateHMACSHA512Hash(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                var salt = hmac.Key;

                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                return (hash, salt);
            }
        }
    }
}
