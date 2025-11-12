using CheckMachAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CheckMachAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Ejemplo de entidad propia
        public DbSet<Inspection> Inspections { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<InventoryMove> InventoryMoves { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<Maintenance> Maintenances { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TimePeerProject> TimePeerProjects { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole()
                {
                    Id = "admin",
                    Name = "admin",
                    NormalizedName = "ADMIN",
                },
                new IdentityRole()
                {
                    Id = "manager",
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                },
                new IdentityRole()
                {
                    Id = "user",
                    Name = "user",
                    NormalizedName = "user",
                }
                );
        }
    }
}
