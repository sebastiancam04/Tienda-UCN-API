namespace Tienda_UCN_api.src.Domain.Models
{
    public enum CodeType
    {
        EmailVerification,
        PasswordReset,
        PasswordChange
    }

    public class VerificationCode
    {

        /// <summary>
        /// Identificador único del código de verificación.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tipo de código de verificación.
        /// </summary>
        public required CodeType CodeType { get; set; }

        /// <summary>
        /// Código de verificación.
        /// </summary>
        public required string Code { get; set; }

        /// <summary>
        /// Cantidad de intentos realizados para usar el código (acumulativo).
        /// </summary>
        public int AttemptCount { get; set; } = 0;

        /// <summary>
        /// Fecha y hora de expiración del código de verificación (3 minutos por defecto).
        /// </summary>
        public required DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Identificador único del usuario asociado al código de verificación.
        /// </summary>
        public required int UserId { get; set; }

        /// <summary>
        /// Fecha y hora en que se creó el código de verificación.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}