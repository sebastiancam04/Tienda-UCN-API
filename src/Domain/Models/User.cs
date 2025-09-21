using Microsoft.AspNetCore.Identity;
using Tienda_UCN_api.Domain.Models;
// Add the following using if VerificationCode is in another namespace, e.g.:
// <-- Change to actual namespace if needed


namespace Tienda_UCN_api.Domain.Models
{
    public enum Gender
    {
        Masculino,
        Femenino,
        Otro
    }
    public class User : IdentityUser<int>
    {

        /// <summary>
        /// Identificador único del usuario chileno.
        /// </summary>
        public required string Rut { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        public required string FirstName { get; set; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        public required string LastName { get; set; }

        /// <summary>
        /// Género del usuario.
        /// </summary>
        public required Gender Gender { get; set; }

        /// <summary>
        /// Fecha de nacimiento del usuario.
        /// </summary>
        public required DateTime BirthDate { get; set; }


        /// <summary>
        /// Indica si el usuario ha aceptado los términos y condiciones.
        /// </summary>
        public ICollection<VerificationCode> VerificationCodes { get; set; } = new List<VerificationCode>();


        /// <summary>
        /// Órdenes realizadas por el usuario.
        /// </summary>
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        /// <summary>
        /// Fecha de registro del usuario.
        /// </summary>
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de actualización del usuario.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}