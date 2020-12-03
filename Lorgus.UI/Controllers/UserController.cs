using Lorgus.UI.Models;
using Lorgus.UI.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lorgus.UI.Controllers
{
    [Authorize(Roles = "admin")]
    public class UserController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public UserController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index([FromServices]LorgusContext context)
        {
            return View(context.User.Include(u => u.AccessLevel).ToList());
        }

        [HttpPost]
        public IActionResult Create([FromForm]User model, [FromServices] LorgusContext context, [FromServices]IEmailSender emailSender )
        {
            model.Phone = Regex.Replace(model.Phone, @"\D+", string.Empty);

            model.AccessLevelId = 2;

            context.User.Add(model);

            context.SaveChanges();

            var request = new RequestChangePassword
            {
                Date = DateTime.Now,
                UserId = model.UserId,
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
                    { "{{name}}", model.FullName },
                    { "{{action_url}}", Url.Action("ResetPassword", "Account", new { id = request.RequestChangePasswordId }, Request.Scheme) },
                    { "{{logo}}", "Logo" },
                    { "{{date}}", DateTime.UtcNow.AddHours(-3).ToString("dd/MM/yyyy HH:mm:ss") },
                };

            var to = new MailAddress(model.Email, model.FullName);

            var body = System.IO.File.ReadAllText($"Templates/EmailNewUserTemplate.html");

            try
            {
                emailSender.Send(to, body, "Bem vindo a Lorgus", replacements, attachments);

            }
            catch
            {

            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Edit([FromForm] User model, [FromServices] LorgusContext context)
        {
            var user = context.User.FirstOrDefault(u => u.UserId == model.UserId);

            if (user is null)
                return BadRequest();

            user.FullName = model.FullName;
            
            user.Phone = model.Phone;
            
            user.Email = model.Email;

            context.SaveChanges();

            return RedirectToAction(nameof(Index));
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
