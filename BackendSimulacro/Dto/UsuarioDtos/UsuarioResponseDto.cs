namespace BackendSimulacro.Dto
{
    public class UsuarioResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public int Rol { get; set; }
        public string? Token { get; set; } // para login
    }
}