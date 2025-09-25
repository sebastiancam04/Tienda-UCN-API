using System.ComponentModel.DataAnnotations;
using Tienda_UCN_api.src.Application.Validators;

namespace Tienda_UCN_api.src.Application.DTO.AuthDTO
{
    public class RegisterDTO
    {
        /// <summary>
        /// Email del usuario.
        /// </summary>
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        public required string Email { get; set; }

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es requerida.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[0-9])(?=.*[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ])(?=.*[!@#$%^&*()_+\[\]{};':""\\|,.<>/?]).*$", ErrorMessage = "La contraseña debe ser alfanumérica y contener al menos una mayúscula y al menos un caracter especial.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [MaxLength(20, ErrorMessage = "La contraseña debe tener como máximo 20 caracteres")]
        public required string Password { get; set; }

        /// <summary>
        /// Confirmación de la contraseña del usuario.
        /// </summary>
        [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public required string ConfirmPassword { get; set; }

        /// <summary>
        /// Rut del usuario.
        /// </summary>
        [Required(ErrorMessage = "El Rut es obligatorio.")]
        [RegularExpression(@"^\d{7,8}-[0-9kK]$", ErrorMessage = "El Rut debe tener formato XXXXXXXX-X")]
        [RutValidation(ErrorMessage = "El Rut no es válido.")]
        public required string Rut { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-]+$", ErrorMessage = "El Nombre solo puede contener carácteres del abecedario español.")]
        [MinLength(2, ErrorMessage = "El nombre debe tener mínimo 2 letras.")]
        [MaxLength(20, ErrorMessage = "El nombre debe tener máximo 20 letras.")]
        public required string FirstName { get; set; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-]+$", ErrorMessage = "El Apellido solo puede contener carácteres del abecedario español.")]
        [MinLength(2, ErrorMessage = "El apellido debe tener mínimo 2 letras.")]
        [MaxLength(20, ErrorMessage = "El apellido debe tener máximo 20 letras.")]
        public required string LastName { get; set; }

        /// <summary>
        /// Fecha de nacimiento del usuario.
        /// </summary>
        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [BirthDateValidation]
        public required DateTime BirthDate { get; set; }

        /// <summary>
        /// Número de teléfono del usuario.
        /// </summary>
        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "El número de teléfono debe tener 9 dígitos.")]
        public required string PhoneNumber { get; set; }

        /// <summary>
        /// Género del usuario.
        /// </summary>
        [Required(ErrorMessage = "El género es obligatorio.")]
        [RegularExpression(@"^(Masculino|Femenino|Otro)$", ErrorMessage = "El género debe ser Masculino, Femenino u Otro.")]
        public required string Gender { get; set; }
    }
}