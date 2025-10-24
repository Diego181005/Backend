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
        
        public DbSet<Carrito> Carritos { get; set; }
        
        public DbSet<CarritoItem> CarritoItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CarritoItem>()
                .HasOne(ci => ci.Producto)
                .WithMany()
                .HasForeignKey(ci => ci.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}