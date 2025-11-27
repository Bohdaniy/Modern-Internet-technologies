using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{

    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; } // Номер поточної сторінки
        public int TotalPages { get; private set; } // Загальна кількість сторінок

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items); // Додаємо елементи в цей список
        }

        // Властивість: чи є попередня сторінка?
        public bool HasPreviousPage => PageIndex > 1;

        // Властивість: чи є наступна сторінка?
        public bool HasNextPage => PageIndex < TotalPages;

        // Головний метод створення сторінки
        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync(); // Рахуємо загальну кількість записів у БД

            
            // Skip - пропускаємо (номер сторінки - 1) * розмір сторінки
            // Take - беремо тільки потрібну кількість
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}