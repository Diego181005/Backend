using System.ComponentModel.DataAnnotations;

namespace BackendSimulacro.Models;
 
public enum RolUsuario
{
    Usuario = 1,
    Empresa = 2,
    Administrador = 3
}

 public class Usuario
 {
     [Key]
     public int Id { get; set; }

     [Required, MaxLength(100)]
     public string Nombre { get; set; } = string.Empty;

     [Required, MinLength(6)]
     public string PasswordHash { get; set; } = string.Empty;

     [Required]
     public RolUsuario Rol { get; set; } = RolUsuario.Usuario;

     // Ejemplo de relaciones futuras:
     // [JsonIgnore]
     // public List<Producto> Productos { get; set; } = [];
     // public List<Carrito> Carritos { get; set; } = [];
 }