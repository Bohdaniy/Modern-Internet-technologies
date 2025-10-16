using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace WebApplicationData.Interfaces
{
    public interface IRepository
    {
       
         IQueryable<T> All<T>() where T : class;

       
         IQueryable<T> ReadAll<T>() where T : class;

        
         IQueryable<T> ReadWhere<T>(Expression<Func<T, bool>> expression) where T : class;

        
         Task<T?> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> expression) where T : class;

         Task<T?> SingleAsync<T>(Expression<Func<T, bool>> expression) where T : class;

         Task<T?> ReadSingleAsync<T>(Expression<Func<T, bool>> expression) where T : class;

         Task<int> AddAsync<T>(T item) where T : class;

         Task<int> UpdateAsync<T>(T item) where T : class;

         Task<int> RemoveAsync<T>(T item) where T : class;


         Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> expression) where T : class;
    }
}
