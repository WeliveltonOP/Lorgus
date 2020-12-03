using Lorgus.UI.Models;
using Lorgus.UI.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Lorgus.UI.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {

        private readonly IWebHostEnvironment _env;

        public AdminController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index([FromServices] LorgusContext context)
        {
            return View(context.Budget.OrderByDescending(b => b.Date).ToList());
        }
        
        public IActionResult SendAnswer(int budgetId, string message, [FromServices] LorgusContext context, [FromServices] IEmailSender emailSender)
        {

            var budget = context.Budget.FirstOrDefault(b => b.BudgetId == budgetId);

            if (budget is null)
                return BadRequest();


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
                    { "{{name}}", budget.FullName },
                    { "{{message}}", message},
                    { "{{date}}", DateTime.UtcNow.AddHours(-3).ToString("dd/MM/yyyy HH:mm:ss") },
                };

            var body = System.IO.File.ReadAllText("Templates/AnswerTemplate.html");

            var to = new MailAddress(budget.Email);

            emailSender.Send(to, body, "Lorgus - resposta de solicitação", replacments, attachments);



            budget.Answered = true;

            context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
