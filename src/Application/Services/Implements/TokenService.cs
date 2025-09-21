
/*
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Serilog;
//using Tienda_UCN_api.Application.Services.Interfaces;
using Tienda_UCN_api.Domain.Models;

namespace Tienda_UCN_api.Application.Services.Implements
{
    /// <summary>
    /// Implementación del servicio de generación de tokens JWT.
    /// </summary>
    public class TokenService : ITokenService
    {
        //Cargamos la configuración desde appsettings.json
        private readonly IConfiguration _configuration;
        private readonly string _jwtSecret;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "La configuración no puede ser nula");
            _jwtSecret = _configuration["JWTSecret"] ?? throw new InvalidOperationException("La clave secreta JWT no está configurada.");
        }

        /// <summary>
        /// Genera un token JWT para el usuario proporcionado.
        /// </summary>
        /// <param name="user">El usuario para el cual se generará el token.</param>
        /// <param name="rememberMe">Indica si se debe recordar al usuario.</param>
        /// <param name="roleName">El nombre del rol del usuario.</param>
        /// <returns>Un string que representa el token JWT generado.</returns>
        public string GenerateToken(User user, string roleName, bool rememberMe = false)
        {
            try
            {
                // Listamos los claims que queremos incluir en el token (solo las necesarias, no todas las propiedades del usuario)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim(ClaimTypes.Role, roleName)
                };

                // Creamos la clave de seguridad
                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtSecret));

                // Creamos las credenciales de firma, ojo la clave debe ser lo suficientemente larga y segura (256 bits mínimo para HMACSHA256) que son 32 caracteres
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Creamos el token
                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddHours(rememberMe ? 24 : 1), // Si RememberMe es true, el token expira en 24 horas, sino en 1 hora
                    signingCredentials: creds
                );

                // Serializamos el token a string
                Log.Information("Token JWT generado exitosamente para el usuario {UserId}", user.Id);
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al generar el token JWT para el usuario {UserId}", user.Id);
                throw new InvalidOperationException("Error al generar el token JWT", ex);
            }

        }
    }
}

*/