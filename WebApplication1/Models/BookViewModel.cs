using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class BookViewModel
    {
        // [Required] - Обов'язкове поле
        // [StringLength] - Обмеження довжини
        // [Remote] - Валідація через запит на сервер (перевірка унікальності)

        [Required(ErrorMessage = "Назва книги є обов'язковою.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Довжина назви має бути від 3 до 100 символів.")]
        [Remote(action: "VerifyTitle", controller: "Books", ErrorMessage = "Така назва книги вже існує.")]
        [Display(Name = "Назва книги")]
        public string Title { get; set; }

        // [EmailAddress] - Перевірка формату email
        [Required(ErrorMessage = "Email автора обов'язковий.")]
        [EmailAddress(ErrorMessage = "Некоректний формат Email.")]
        [Display(Name = "Email автора")]
        public string AuthorEmail { get; set; }

        // [Compare] - Порівняння з іншим полем
        [Required(ErrorMessage = "Підтвердження Email є обов'язковим.")]
        [Compare("AuthorEmail", ErrorMessage = "Email адреси не співпадають.")]
        [Display(Name = "Підтвердження Email")]
        public string ConfirmAuthorEmail { get; set; }

        // [Range] - Діапазон значень
        [Required]
        [Range(1900, 2025, ErrorMessage = "Рік має бути між 1900 та 2025.")]
        [Display(Name = "Рік видання")]
        public int Year { get; set; }

        [Required]
        [Range(0, 10000, ErrorMessage = "Ціна має бути більше 0.")]
        [Display(Name = "Ціна")]
        public decimal Price { get; set; }

        // Залежна Remote валідація (перевірка залежить від ціни)
        [Remote(action: "VerifyDiscount", controller: "Books", AdditionalFields = "Price", ErrorMessage = "Знижка не може бути більшою за ціну.")]
        [Display(Name = "Знижка")]
        public decimal Discount { get; set; }
    }
}