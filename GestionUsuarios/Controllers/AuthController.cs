using GestionUsuarios.Data;
using GestionUsuarios.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionUsuarios.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // Registro de Vecino
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _context.Usuarios.AnyAsync(u => u.email == dto.Correo))
            {
                return BadRequest("El correo ya está registrado.");
            }

            var usuario = new Usuario
            {
                nombre = dto.Nombre,
                email = dto.Correo,
                contraseña = BCrypt.Net.BCrypt.HashPassword(dto.Contraseña),
                telefono = dto.Telefono,
                rut = dto.Rut,
                dv = dto.Dv
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var vecino = new Vecino
            {
                id = usuario.id,
                direccion = dto.Direccion
            };

            _context.Vecinos.Add(vecino);
            await _context.SaveChangesAsync();

            return Ok("Vecino registrado exitosamente.");
        }

        // Inicio de sesión
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.email == dto.Correo);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Contraseña, usuario.contraseña))
            {
                return Unauthorized("Credenciales inválidas.");
            }

            // Generar token (opcional: JWT)
            return Ok(new { mensaje = "Inicio de sesión exitoso" });
        }

        public class RegisterDto
        {
            public string Nombre { get; set; }
            public string Correo { get; set; }
            public string Contraseña { get; set; }
            public string Direccion { get; set; }
            public int Telefono { get; set; }
            public int Rut { get; set; }
            public char Dv { get; set; }
        }

        public class LoginDto
        {
            public string Correo { get; set; }
            public string Contraseña { get; set; }
        }
    }
}
