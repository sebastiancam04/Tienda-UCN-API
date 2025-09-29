using Tienda_UCN_api.src.Application.DTO;
using Tienda_UCN_api.src.Application.DTO.AuthDTO;

namespace Tienda_UCN_api.src.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de usuarios.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Inicia sesión con el usuario proporcionado.
        /// </summary>
        /// <param name="loginDTO">DTO que contiene las credenciales del usuario.</param>
        /// <param name="httpContext">El contexto HTTP actual.</param>
        /// <returns>Un string que representa el token JWT generadon y la id del usuario.</returns>
        Task<(string token, int userId)> LoginAsync(LoginDTO loginDTO, HttpContext httpContext);

        /// <summary>
        /// Registra un nuevo usuario.
        /// </summary>
        /// <param name="registerDTO">DTO que contiene la información del nuevo usuario.</param>
        /// <param name="httpContext">El contexto HTTP actual.</param>
        /// <returns>Un string que representa el mensaje de éxito del registro.</returns>
        Task<string> RegisterAsync(RegisterDTO registerDTO, HttpContext httpContext);

        /// <summary>
        /// Verifica el correo electrónico del usuario.
        /// </summary>
        /// <param name="verifyEmailDTO">DTO que contiene el correo electrónico y el código de verificación.</param>
        /// <returns>Un string que representa el mensaje de éxito de la verificación.</returns>
        Task<string> VerifyEmailAsync(VerifyEmailDTO verifyEmailDTO);

        /// <summary>
        /// Reenvía el código de verificación al correo electrónico del usuario.
        /// </summary>
        /// <param name="resendEmailVerificationCodeDTO">DTO que contiene el correo electrónico del usuario.</param>
        /// <returns>Un string que representa el mensaje de éxito del reenvío.</returns>
        Task<string> ResendEmailVerificationCodeAsync(ResendEmailVerificationCodeDTO resendEmailVerificationCodeDTO);

        /// <summary>
        /// Elimina usuarios no confirmados.
        /// </summary>
        /// <returns>Número de usuarios eliminados</returns>
        Task<int> DeleteUnconfirmedAsync();

        Task<string> SendForgotPasswordCodeAsync(ForgotPasswordRequestDTO dto);
        Task<string> VerifyForgotPasswordCodeAsync(VerifyForgotPasswordCodeDTO dto);

    }
}