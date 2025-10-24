using BackendSimulacro.Models;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Password { get; set; } = null!;
    public RolUsuario Rol { get; set; } // <- usar el mismo enum que el modelo
}