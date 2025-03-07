using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RaadHetWoordApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Woord> Woorden { get; set; }
    }

    public class Woord
    {
        [Key]
        public int Id { get; set; }
        public string Tekst { get; set; } = string.Empty;
    }
}
