using Microsoft.AspNetCore.Mvc;
using Tienda_UCN_api.src.Application.DTO;
using Tienda_UCN_api.src.Application.DTO.AuthDTO;
using Tienda_UCN_api.src.Application.Services.Interfaces;

namespace Tienda_UCN_api.src.api.Controllers
{
    /// <summary>
    /// Controlador de autenticación.
    /// Maneja registro, login y verificación de correo.
    /// </summary>
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Inicia sesión con el usuario proporcionado.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var (token, userId) = await _userService.LoginAsync(loginDTO, HttpContext);
            return Ok(new GenericResponse<string>("Inicio de sesión exitoso", token));
        }

        /// <summary>
        /// Registra un nuevo usuario.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            var message = await _userService.RegisterAsync(registerDTO, HttpContext);
            return Ok(new GenericResponse<string>("Registro exitoso", message));
        }

        /// <summary>
        /// Verifica el correo electrónico del usuario.
        /// </summary>
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDTO verifyEmailDTO)
        {
            var message = await _userService.VerifyEmailAsync(verifyEmailDTO);
            return Ok(new GenericResponse<string>("Verificación de correo electrónico exitosa", message));
        }

        /// <summary>
        /// Reenvía el código de verificación al correo electrónico del usuario.
        /// </summary>
        [HttpPost("resend-email-verification-code")]
        public async Task<IActionResult> ResendEmailVerificationCode([FromBody] ResendEmailVerificationCodeDTO resendEmailVerificationCodeDTO)
        {
            var message = await _userService.ResendEmailVerificationCodeAsync(resendEmailVerificationCodeDTO);
            return Ok(new GenericResponse<string>("Código de verificación reenviado exitosamente", message));
        }
    }
}