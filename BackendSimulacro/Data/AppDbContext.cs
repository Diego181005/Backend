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

    public DbSet<Usuario> Usuarios { get; set; } = null!;
    // Aquí luego agregaremos:
    // public DbSet<Producto> Productos { get; set; } = null!;
    // public DbSet<Carrito> Carritos { get; set; } = null!;
}
}