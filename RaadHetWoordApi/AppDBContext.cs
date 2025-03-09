using Microsoft.EntityFrameworkCore;
using RaadHetWoordApi.Models;

namespace RaadHetWoordApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Woord> Woorden { get; set; }
    }
}
