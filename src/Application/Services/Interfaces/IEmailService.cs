namespace Tienda_UCN_api.src.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de envío de correos electrónicos.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envía un código de verificación al correo electrónico del usuario.
        /// </summary>
        /// <param name="email">El correo electrónico del usuario.</param>
        /// <param name="code">El código de verificación a enviar.</param>
        Task SendVerificationCodeEmailAsync(string email, string code);

        /// <summary>
        /// Envía un correo electrónico de bienvenida al usuario.
        /// </summary>
        /// <param name="email">El correo electrónico del usuario.</param>
        Task SendWelcomeEmailAsync(string email);
    }
}