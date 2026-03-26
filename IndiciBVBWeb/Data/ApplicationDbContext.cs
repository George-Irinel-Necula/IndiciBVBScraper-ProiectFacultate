using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IndiciBVBWeb.Models;

namespace IndiciBVBWeb.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<IndiciBVBWeb.Models.IndiciBVB> IndiciBVB { get; set; } = default!;
    }
}
