using System.ComponentModel.DataAnnotations;

namespace Tienda_UCN_api.src.Application.DTO.AuthDTO
{
    public class VerifyEmailDTO
    {
        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
        public required string Email { get; set; }

        /// <summary>
        /// Código de verificación enviado al correo electrónico.
        /// </summary>
        [Required(ErrorMessage = "El código de verificación es obligatorio.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "El código de verificación debe tener 6 dígitos.")]
        public required string VerificationCode { get; set; }
    }
}