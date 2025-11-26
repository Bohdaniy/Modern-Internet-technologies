using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplicationData.Data;       
using WebApplicationData.Interfaces;
using WebApplicationData.Repositories; 

namespace WebApplication1.Controllers
{
    public class BooksController : Controller
    {
        private readonly IWebRepository _repository; 

        public BooksController(IWebRepository repository)
        {
            _repository = repository;
        }

        // 1Відображення форми
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Обробка форми (Серверна валідація та збереження)
        [HttpPost]
        public async Task<IActionResult> Create(BookViewModel model)
        {
            // Перевірка валідації на сервері
            if (ModelState.IsValid)
            {
                // перетворюємо  ViewModel у реальну сутність БД
                var bookEntity = new Book
                {
                    Title = model.Title,
                    AuthorEmail = model.AuthorEmail,
                    Year = model.Year,
                    Price = model.Price,
                    Discount = model.Discount
                };

                // Зберігаємо в базу даних через репозиторій
                await _repository.AddAsync(bookEntity);

                // Повертаємо повідомлення про успіх або перенаправляємо
                return Content($"Книга '{model.Title}' успішно створена та збережена в БД! ID: {bookEntity.Id}");
            }

            // Якщо є помилки, повертаємо ту саму форму з помилками
            return View(model);
        }

        // Remote Validation: Перевірка унікальності назви
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> VerifyTitle(string title)
        {
            // Перевіряємо в реальній БД, чи є така книга
            // Використовуємо ExistsAsync, який ми додали в лабораторній №2
            var exists = await _repository.ExistsAsync<Book>(b => b.Title == title);

            if (exists)
            {
                return Json($"Книга з назвою '{title}' вже існує.");
            }

            return Json(true);
        }

        // Dependent Remote Validation: Перевірка знижки залежно від ціни
        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyDiscount(decimal discount, decimal price)
        {
            if (discount > price)
            {
                return Json("Знижка не може бути більшою за ціну товару.");
            }

            return Json(true);
        }
    }
}