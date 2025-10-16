using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebApplicationData.Interfaces;

namespace WebApplicationData.Repositories
{
    public class BaseSqlServerRepository<TDbContext> : IRepository where TDbContext : DbContext
    {
        protected TDbContext Db { get; set; }

        public BaseSqlServerRepository(TDbContext db)
        {
            Db = db;
        }

        public async Task<int> AddAsync<T>(T item) where T : class
        {
            await Db.AddAsync(item);
            return await Db.SaveChangesAsync();
        }

        public IQueryable<T> All<T>() where T : class
        {
            return Db.Set<T>().AsQueryable();
        }

        public async Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return await All<T>().AnyAsync(expression);
        }

        public async Task<T?> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return await All<T>().FirstOrDefaultAsync(expression);
        }

        public IQueryable<T> ReadAll<T>() where T : class
        {
            return All<T>().AsNoTracking();
        }

        public async Task<T?> ReadSingleAsync<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return await ReadAll<T>().SingleOrDefaultAsync(expression);
        }

        public IQueryable<T> ReadWhere<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return ReadAll<T>().Where(expression);
        }

        public async Task<int> RemoveAsync<T>(T item) where T : class
        {
            Db.Remove(item);
            return await Db.SaveChangesAsync();
        }

        public async Task<T?> SingleAsync<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return await All<T>().SingleOrDefaultAsync(expression);
        }

        public async Task<int> UpdateAsync<T>(T item) where T : class
        {
       
            var local = Db.Set<T>()
                .Local
                .FirstOrDefault(entry => entry.Equals(item));

            if (local != null)
            {
               Db.Entry(local).State = EntityState.Detached; 
            }

            Db.Entry(item).State = EntityState.Modified; 
            return await Db.SaveChangesAsync();
        }
    }
}