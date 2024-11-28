using GestionUsuarios.Data;
using GestionUsuarios.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace GestionUsuarios.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
            // Determinar el tipo de usuario
            bool esVecino = await _context.Vecinos.AnyAsync(v => v.id == usuario.id);
            bool esFuncionario = await _context.FuncionariosMunicipal.AnyAsync(f => f.id == usuario.id);

            if (dto.TipoAplicacion == "Web" && !esFuncionario)
            {
                return Unauthorized("El usuario no tiene permiso para acceder a esta aplicación.");
            }

            if (dto.TipoAplicacion == "Movil" && !esVecino)
            {
                return Unauthorized("El usuario no tiene permiso para acceder a esta aplicación.");
            }
            // Generar token (opcional: JWT)
            var tipoUsuario = esVecino ? "Vecino" : "FuncionarioMunicipal";
            var token = GenerarToken(usuario, tipoUsuario);
            return Ok(new { Token = token });
            //return Ok(new { mensaje = "Inicio de sesión exitoso" });
        }

        private string GenerarToken(Usuario usuario, string tipoUsuario)
        {
            // Crear los claims (información dentro del token)
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.email),
            new Claim("nombre", usuario.nombre),
            new Claim("tipoUsuario", tipoUsuario),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // ID único del token
        };

            // Generar la clave de firma
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Crear el token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
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
            public string TipoAplicacion { get; set; }
        }
    }
}
