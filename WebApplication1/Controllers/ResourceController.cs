using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationData.Data;
using WebApplicationData.Interfaces;
using WebApplicationData.Repositories;

namespace WebApplication1.Controllers
{
    [Authorize] // Тільки для залогінених користувачів
    public class ResourceController : Controller
    {
        private readonly IWebRepository _repository;
        private readonly IAuthorizationService _authorizationService;
        private readonly UserManager<WebApplicationUser> _userManager;

        public ResourceController(
            IWebRepository repository,
            IAuthorizationService authorizationService,
            UserManager<WebApplicationUser> userManager)
        {
            _repository = repository;
            _authorizationService = authorizationService;
            _userManager = userManager;
        }

        // 1. Список усіх ресурсів
        public async Task<IActionResult> Index()
        {
            // Отримуємо всі ресурси з бази
            var resources = await _repository.ReadAll<AppResource>().Include(r=>r.Author).ToListAsync();
            return View(resources);
        }

        // 2. Сторінка створення (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. Логіка створення (POST)
        [HttpPost]
        public async Task<IActionResult> Create(AppResource model)
        {
            // Отримуємо поточного користувача
            var user = await _userManager.GetUserAsync(User);

            // Присвоюємо автора ресурсу
            model.AuthorId = user.Id;

            // Зберігаємо в БД
            await _repository.AddAsync(model);

            return RedirectToAction("Index");
        }

        // 4. Сторінка редагування (GET)    
        public async Task<IActionResult> Edit(int id)
        {
            var resource = await _repository.ReadSingleAsync<AppResource>(r => r.Id == id);

            if (resource == null)
            {
                return NotFound();
            }


            var authorizationResult = await _authorizationService
                .AuthorizeAsync(User, resource, "ResourceOwner");

            if (!authorizationResult.Succeeded)
            {
                // Якщо ні - повертаємо сторінку "Заборонено"
                return Forbid();
            }

            return View(resource);
        }

        // 5. Логіка редагування (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(AppResource model)
        {
            // Зчитуємо оригінальний ресурс з БД, щоб не втратити AuthorId
            var resource = await _repository.SingleAsync<AppResource>(r => r.Id == model.Id);

            if (resource == null) return NotFound();

            // Повторна перевірка авторизації (безпека понад усе)
            var authorizationResult = await _authorizationService
                .AuthorizeAsync(User, resource, "ResourceOwner");

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            // Оновлюємо дані
            resource.Title = model.Title;

            await _repository.UpdateAsync(resource);

            return RedirectToAction("Index");
        }
    }
}