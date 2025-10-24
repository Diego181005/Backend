using BackendSimulacro.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendSimulacro.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Tablas
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Producto> Productos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de enum RolUsuario como int en la DB
            modelBuilder.Entity<Usuario>()
                .Property(u => u.Rol)
                .HasConversion<int>();

        }
    }
}