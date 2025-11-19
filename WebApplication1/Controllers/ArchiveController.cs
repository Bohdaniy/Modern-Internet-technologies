using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Authorize(Policy = "ArchivePolicy")] 
    public class ArchiveController : Controller
    {
        public IActionResult Index()
        {
            return Content("Ласкаво просимо до Архіву! Тільки для Verified Clients.");
        }
    }
}
