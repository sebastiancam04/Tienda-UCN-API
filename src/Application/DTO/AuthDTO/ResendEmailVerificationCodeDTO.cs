using System.ComponentModel.DataAnnotations;

namespace Tienda_UCN_api.src.Application.DTO.AuthDTO
{
    public class ResendEmailVerificationCodeDTO
    {
        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
        public required string Email { get; set; }
    }
}