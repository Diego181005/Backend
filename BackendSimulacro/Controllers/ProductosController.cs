using BackendSimulacro.Data;
using BackendSimulacro.Dto;
using BackendSimulacro.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BackendSimulacro.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/productos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoResponseDto>>> GetProductos(int? empresaId)
    {
        var query = _context.Productos.AsQueryable();

        if (empresaId.HasValue)
            query = query.Where(p => p.EmpresaId == empresaId.Value);

        var productos = await query
            .Select(p => new ProductoResponseDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Precio = p.Precio,
                Stock = p.Stock,
                EmpresaId = p.EmpresaId
            })
            .ToListAsync();

        return Ok(productos);
    }

    // POST: api/productos
    [HttpPost]
    [Authorize(Roles = "2")] // Solo empresas
    public async Task<ActionResult<ProductoResponseDto>> CrearProducto(ProductoDto dto)
    {
        var empresaId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Precio = dto.Precio,
            Stock = dto.Stock,
            EmpresaId = empresaId
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProductos), new { id = producto.Id }, new ProductoResponseDto
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Precio = producto.Precio,
            Stock = producto.Stock,
            EmpresaId = producto.EmpresaId
        });
    }
}