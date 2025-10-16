using WebApplicationData.Data;
using WebApplicationData.Interfaces;

namespace WebApplicationData.Repositories
{
    public class WebRepository : BaseSqlServerRepository<WebApplicationDbContext>, IWebRepository
    {
        public WebRepository(WebApplicationDbContext db) : base(db)
        {
        }

        public async Task<WebApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await FirstOrDefaultAsync<WebApplicationUser>(u => u.Email == email);
        }
    }
}