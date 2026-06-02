using Microsoft.EntityFrameworkCore;

namespace WebApplication1.models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
                
        }
        public DbSet<User> Users { get; set; }
    }
}
