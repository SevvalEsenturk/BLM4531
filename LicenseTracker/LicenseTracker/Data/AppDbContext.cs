using LicenseTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LicenseTracker.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options) { }

        public DbSet<License> Licenses { get; set; }
        public DbSet<Company> Companies { get; set;}
        public DbSet<LoginAudit> LoginAudits { get; set; }
    }
}