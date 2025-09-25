using System.ComponentModel.DataAnnotations;

namespace Tienda_UCN_api.src.Application.DTO
{
    /// <summary>
    /// DTO para el proceso de inicio de sesión.
    /// </summary>
    public class LoginDTO
    {
        /// <summary>
        /// Email del usuario.
        /// </summary>
        [Required(ErrorMessage = "El email es requerido.")]
        [EmailAddress(ErrorMessage = "El Correo electrónico no es válido.")]
        public required string Email { get; set; }

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es requerida.")]
        public required string Password { get; set; }

        /// <summary>
        /// Indica si se debe recordar al usuario.
        /// </summary>
        public bool RememberMe { get; set; }
    }
}