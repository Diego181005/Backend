namespace BackendSimulacro.Dto;

public class ProductoResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int EmpresaId { get; set; }
}