using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Controllers
{
    [Authorize(Policy = "PremiumContent")] // <-- Захищаємо нашою політикою
    public class PremiumController : Controller
    {
        public IActionResult Index()
        {
            return Content("Вітаємо! Ви маєте достатньо робочих годин (100+), щоб бачити цю преміум-сторінку.");
        }
    }
}