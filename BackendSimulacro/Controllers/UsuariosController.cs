

using System.Security.Claims;
using BackendSimulacro.Data;
using BackendSimulacro.Dto;
using BackendSimulacro.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendSimulacro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Password = "", // nunca devolver contraseña
                    Rol = u.Rol
                })
                .ToListAsync();

            return Ok(usuarios);
        }
        [HttpDelete("{id}")]
        [Authorize] // Debe estar logueado
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            // Obtener id y rol del usuario que hace la petición
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var currentUserRole = int.Parse(User.FindFirstValue(ClaimTypes.Role)!);

            // Solo admin o dueño de la cuenta puede eliminar
            if (currentUserRole != 3 && currentUserId != id) // 3 = Admin
                return Forbid("No tienes permisos para eliminar este usuario.");

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}