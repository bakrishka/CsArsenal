using System.Diagnostics;
using CsArsenal.Models;
using Microsoft.AspNetCore.Mvc;

namespace CsArsenal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ItemRepository _repo;

        public HomeController(IConfiguration config)
        {
            _repo = new ItemRepository(config.GetConnectionString("Default")!);
        }

        public IActionResult Index()
        {
            var items = _repo.GetAll().Take(4).ToList();
            return View(items);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contacts()
        {
            return View(new ContactModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contacts(ContactModel model)
        {
            if (!ModelState.IsValid) return View(model);

            TempData["Message"] = "Спасибо! Ваше сообщение отправлено.";
            return RedirectToAction("Contacts");
        }

        public IActionResult Faq()
        {
            return View();
        }

        public IActionResult Error404(int code = 404)
        {
            Response.StatusCode = code;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
