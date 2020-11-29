using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Lorgus.UI.Models;
using Lorgus.UI.Services.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Lorgus.UI.Controllers
{
    public class HomeController : Controller
    {
        private IWebHostEnvironment _env;

        private LorgusConfig _config;

        public HomeController(IWebHostEnvironment env, IOptions<LorgusConfig> options)
        {
            _env = env;

            _config = options.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SendContactEmail(ContactModel model, [FromServices] IEmailSender emailSender, [FromServices]LorgusContext lorgusContext)
        {
            try
            {
                try
                {
                    lorgusContext.Budget.Add(new Budget
                    {
                        FullName = model.FullName,
                        Email=model.Email,
                        Message = model.Message,
                        Phone = model.Phone,
                        Date = DateTime.UtcNow.AddHours(-3)
                    });

                    lorgusContext.SaveChanges();
                }
                catch
                {
                    // ignore
                }

                var attachments = new List<Attachment>
                {
                    new Attachment($"{_env.WebRootPath}/images/logo.png")
                    {
                        ContentId = "Logo"
                    }
                };

                var replacments = new Dictionary<string, string>
                {
                    { "{{logo}}", "Logo" },
                    { "{{fullname}}", model.FullName },
                    { "{{phone}}", model.Phone },
                    { "{{email}}", model.Email },
                    { "{{message}}", model.Message },
                    { "{{date}}", DateTime.UtcNow.AddHours(-3).ToString("dd/MM/yyyy HH:mm:ss") },
                };

                var body = System.IO.File.ReadAllText("Templates/ContactTemplate.html");

                var to = new MailAddress(_config.ContactEmail);

                emailSender.Send(to, body, "Solicitação de orçamento", replacments, attachments);

                return Json(new
                {
                    success = true,
                    message = "E-mail enviado, por favor aguarde o nosso contato.",
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Desculpe, ocorreu um erro, não foi possível enviar o e-mail.",
                    ex = ex.Message
                });
            }
        }
    }
}
