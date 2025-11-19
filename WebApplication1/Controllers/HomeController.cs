using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication1.Models;
using WebApplicationData.Data;
using WebApplicationData.Interfaces;
using WebApplicationData.Models.Configurations; //  додано для доступу до MyConfiguration
using WebApplicationData.Repositories;
using Microsoft.AspNetCore.Authorization;


namespace WebApplication1.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebRepository _repository;
        private readonly MyConfiguration _config; //  додано для роботи з конфігурацією

        //  додано параметр MyConfiguration у конструктор
        public HomeController(ILogger<HomeController> logger, IWebRepository repository, MyConfiguration config)
        {
            _logger = logger;
            _repository = repository;
            _config = config;
        }
        [AllowAnonymous]
        public async Task<IActionResult> IndexAsync()
        {
            var users = await _repository.ReadAll<WebApplicationUser>().ToListAsync();



            return View(users);
        }
        [AllowAnonymous]
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
