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
        
        // POST: api/carrito
        [HttpPost]
        public async Task<IActionResult> AgregarProducto(CarritoItemDto dto)
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var carrito = await _context.Carritos
                .Include(c => c.Items).ThenInclude(carritoItem => carritoItem.Producto)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (carrito == null)
            {
                carrito = new Carrito { UsuarioId = usuarioId };
                _context.Carritos.Add(carrito);
                await _context.SaveChangesAsync();
            }

            var item = carrito.Items.FirstOrDefault(i => i.ProductoId == dto.ProductoId);
            if (item != null)
            {
                item.Cantidad += dto.Cantidad;
            }
            else
            {
                carrito.Items.Add(new CarritoItem
                {
                    ProductoId = dto.ProductoId,
                    Cantidad = dto.Cantidad
                });
            }

            await _context.SaveChangesAsync();

            // Retornar el carrito actualizado
            var response = carrito.Items.Select(i => new CarritoResponseDto
            {
                Id = i.Id,
                ProductoId = i.ProductoId,
                NombreProducto = i.Producto.Nombre,
                Precio = i.Producto.Precio,
                Cantidad = i.Cantidad,
                Subtotal = i.Cantidad * i.Producto.Precio
            }).ToList();

            return Ok(response);
        }

        // PUT: api/carrito/{itemId}
        [HttpPut("{itemId}")]
        public async Task<IActionResult> ActualizarCantidad(int itemId, CarritoItemDto dto)
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var item = await _context.CarritoItems
                .Include(i => i.Carrito)
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.Carrito.UsuarioId == usuarioId);

            if (item == null) return NotFound("Producto no encontrado en tu carrito");

            item.Cantidad = dto.Cantidad;
            await _context.SaveChangesAsync();

            var response = new CarritoResponseDto
            {
                Id = item.Id,
                ProductoId = item.ProductoId,
                NombreProducto = item.Producto.Nombre,
                Precio = item.Producto.Precio,
                Cantidad = item.Cantidad,
                Subtotal = item.Cantidad * item.Producto.Precio
            };

            return Ok(response);
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
