using Microsoft.AspNetCore.Mvc;
using StajEvrak.web.Models;
using System.Diagnostics;

namespace StajEvrak.web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
 
            if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");
            if (User.IsInRole("Student")) return RedirectToAction("Index", "Student");
            return RedirectToAction("Login", "Account");
        }
        

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
