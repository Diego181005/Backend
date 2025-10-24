using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendSimulacro.Models;

public class Producto
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; } = null!;

    [Required]
    public decimal Precio { get; set; }

    [Required]
    public int Stock { get; set; }

    // Relación con el usuario que es empresa
    [Required]
    public int EmpresaId { get; set; }

    [ForeignKey("EmpresaId")]
    public Usuario Empresa { get; set; } = null!;
}