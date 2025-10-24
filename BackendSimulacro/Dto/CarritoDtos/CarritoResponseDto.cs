namespace BackendSimulacro.Dto.CarritoDtos;

public class CarritoResponseDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; } 
    public string NombreProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Precio { get; set; }
    public decimal Subtotal { get; set; }
}