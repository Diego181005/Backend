using BackendSimulacro.Data;
using BackendSimulacro.Dto;
using BackendSimulacro.Models;
using BackendSimulacro.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendSimulacro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;

        
        public AuthController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<UsuarioResponseDto>> Register(UsuarioDto dto)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Nombre == dto.Nombre))
                return BadRequest("El usuario ya existe.");

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                PasswordHash = dto.Password, // luego se puede hashear
                Rol = dto.Rol
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var response = new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Rol = (int)usuario.Rol
            };

            return CreatedAtAction(nameof(Register), response);
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<UsuarioResponseDto>> Login(LoginDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Nombre == dto.Nombre && u.PasswordHash == dto.Password);

            if (usuario == null)
                return Unauthorized("Usuario o contraseña incorrectos.");

            var token = _tokenService.GenerateToken(usuario);

            var response = new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Rol = (int)usuario.Rol,
                Token = token
            };

            return Ok(response);
        }
    }
}
