using WebApplicationData.Data;
using WebApplicationData.Interfaces;

namespace WebApplicationData.Repositories
{
    public interface IWebRepository : IRepository
    {
        Task<WebApplicationUser?> GetUserByEmailAsync(string email);
    }
}
