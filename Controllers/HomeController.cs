using Microsoft.AspNetCore.Mvc;
using NuJournalPro.Models;
using NuJournalPro.Models.ViewModels;
using NuJournalPro.Services.Interfaces;
using System.Diagnostics;

namespace NuJournalPro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IContactEmailSender _contactEmailSender;

        public HomeController(ILogger<HomeController> logger, IContactEmailSender contactEmailSender)
        {
            _logger = logger;
            _contactEmailSender = contactEmailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact(string? swalMessage = null)
        {
            ViewData["SwalMessage"] = swalMessage;
            ContactUs contactUsModel = new();
            return View(contactUsModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactUs model)
        {
            // This is where we would send the email to the site owner
            if (ModelState.IsValid)
            {
                try
                {
                    model.Message = $"{model.Message}<br /><hr /><br />{model.Name} Phone: {model.Phone}";
                    await _contactEmailSender.SendContactEmailAsync(model.Email, model.Name, model.Subject, model.Message);
                    return RedirectToAction("Contact", "Home", new { swalMessage = "Success: Your message has been sent!" });
                }
                catch
                {
                    return RedirectToAction("Contact", "Home", new { swalMessage = "Error: Your message was not sent." });
                    throw;
                }
            }   
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}