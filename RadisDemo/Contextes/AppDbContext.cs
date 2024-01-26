using Microsoft.EntityFrameworkCore;
using RadisDemo.Models;

namespace RadisDemo.Contextes
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {
            
        }

        public DbSet<Driver> Drivers { get; set; }
    }
}
