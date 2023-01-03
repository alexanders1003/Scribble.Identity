using Microsoft.AspNetCore.Mvc;

namespace Scribble.Identity.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}