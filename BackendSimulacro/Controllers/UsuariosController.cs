

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
        [Authorize]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            // Obtener claims del token
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var currentUserRoleStr = User.FindFirstValue(ClaimTypes.Role)!;

            // Convertir string a enum
            var currentUserRole = Enum.Parse<RolUsuario>(currentUserRoleStr);

            // Solo admin o dueño de la cuenta puede eliminar
            if (currentUserRole != RolUsuario.Administrador && currentUserId != id)
                return Forbid("No tienes permisos para eliminar este usuario.");

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}