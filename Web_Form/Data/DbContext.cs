using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Web_Form.Data
{
    public class DbContext:IdentityDbContext
    {
        public DbContext(DbContextOptions<DbContext> options): base(options)
        {
            
        }
    }
}
