using BackendSimulacro.Data;
using BackendSimulacro.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BackendSimulacro.Dto.CarritoDtos;

namespace BackendSimulacro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Usuario")] // Solo usuarios normales
    public class CarritosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CarritosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<List<CarritoResponseDto>> GetCarrito()
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Suponiendo que obtienes el carrito del usuario logueado
            var carrito = _context.Carritos
                .Include(c => c.Items)
                .ThenInclude(i => i.Producto)
                .FirstOrDefault(c => c.UsuarioId == usuarioId); // usuarioId viene de tu token o sesión

            if (carrito == null)
                return NotFound();

            var carritoResponse = carrito.Items.Select(i => new CarritoResponseDto
            {
                Id = i.Id,
                NombreProducto = i.Producto.Nombre,  // Accedemos al nombre desde Producto
                Cantidad = i.Cantidad,
                Precio = i.Producto.Precio,
                Subtotal = i.Cantidad * i.Producto.Precio
            }).ToList();

            return Ok(carritoResponse);
        }
        
        [HttpPost]
        [Authorize(Roles = "Usuario")]
        public async Task<ActionResult<IEnumerable<CarritoResponseDto>>> AgregarProducto(CarritoItemDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // 1) Validaciones básicas
            if (dto.Cantidad <= 0)
                return BadRequest("La cantidad debe ser mayor a cero.");

            var producto = await _context.Productos.FindAsync(dto.ProductoId);
            if (producto == null)
                return NotFound("El producto no existe.");

            // 2) Traer el carrito con Items + Producto (evita NullReference)
            var carrito = await _context.Carritos
                .Include(c => c.Items)
                .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (carrito == null)
            {
                carrito = new Carrito { UsuarioId = userId, Items = new List<CarritoItem>() };
                _context.Carritos.Add(carrito);
            }

            // 3) Agregar/sumar item
            var item = carrito.Items.FirstOrDefault(i => i.ProductoId == dto.ProductoId);
            if (item == null)
            {
                item = new CarritoItem
                {
                    ProductoId = producto.Id,
                    Cantidad = dto.Cantidad,
                    Precio = producto.Precio, // decimal en tu modelo
                    Producto = producto       // asegura que no sea null en esta request
                };
                carrito.Items.Add(item);
            }
            else
            {
                item.Cantidad += dto.Cantidad;
            }

            await _context.SaveChangesAsync();

            // 4) Proyección a TU CarritoResponseDto (sin clases nuevas)
            var result = carrito.Items.Select(i => new CarritoResponseDto
            {
                Id = i.Id,
                ProductoId = i.ProductoId,
                NombreProducto = i.Producto?.Nombre ?? string.Empty,
                Cantidad = i.Cantidad,
                Precio = i.Precio, // decimal
                Subtotal = i.Precio * i.Cantidad
            }).ToList();

            return Ok(result);
        }

        [HttpPut("{itemId:int}")]
        [Authorize(Roles = "Usuario")]
        public async Task<ActionResult<IEnumerable<CarritoResponseDto>>> ActualizarCantidad(int itemId, [FromBody] int cantidad)
        {
            if (cantidad <= 0) return BadRequest("La cantidad debe ser mayor a cero.");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var carrito = await _context.Carritos
                .Include(c => c.Items).ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (carrito == null) return NotFound("No tienes carrito.");

            var item = carrito.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return NotFound("Item no encontrado.");

            item.Cantidad = cantidad;
            await _context.SaveChangesAsync();

            var result = carrito.Items.Select(i => new CarritoResponseDto
            {
                Id = i.Id,
                ProductoId = i.ProductoId,
                NombreProducto = i.Producto?.Nombre ?? string.Empty,
                Cantidad = i.Cantidad,
                Precio = i.Precio,
                Subtotal = i.Precio * i.Cantidad
            }).ToList();

            return Ok(result);
        }


        // DELETE: api/carrito/{itemId}
        [HttpDelete("{itemId}")]
        public async Task<IActionResult> QuitarProducto(int itemId)
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var item = await _context.CarritoItems
                .Include(i => i.Carrito)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.Carrito.UsuarioId == usuarioId);

            if (item == null) return NotFound("Producto no encontrado en tu carrito");

            _context.CarritoItems.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        // POST: api/carrito/checkout
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout()
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var carrito = await _context.Carritos
                .Include(c => c.Items)
                .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (carrito == null || !carrito.Items.Any())
                return BadRequest("Tu carrito está vacío");

            // Verificar stock disponible antes de descontar
            foreach (var item in carrito.Items)
            {
                if (item.Producto.Stock < item.Cantidad)
                    return BadRequest($"No hay suficiente stock para {item.Producto.Nombre}");
            }

            // Descontar stock y crear el subtotal
            decimal totalCompra = 0;
            foreach (var item in carrito.Items)
            {
                item.Producto.Stock -= item.Cantidad;
                totalCompra += item.Cantidad * item.Producto.Precio;
            }

            // Guardar cambios
            await _context.SaveChangesAsync();

            // Vaciar el carrito
            _context.CarritoItems.RemoveRange(carrito.Items);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mensaje = "Compra realizada con éxito",
                Total = totalCompra
            });
        }

    }
    
}
