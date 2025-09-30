using Mapster;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Tienda_UCN_api.src.Application.DTO;
using Tienda_UCN_api.src.Application.DTO.AuthDTO;
using Tienda_UCN_api.src.Application.Services.Interfaces;
using Tienda_UCN_api.src.Domain.Models;
using Tienda_UCN_api.src.Infrastructure.Repositories.Interfaces;

namespace Tienda_UCN_api.src.Application.Services.Implements
{
    /// <summary>
    /// Implementación del servicio de usuarios.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IVerificationCodeRepository _verificationCodeRepository;
        private readonly int _verificationCodeExpirationTimeInMinutes;

        public UserService(
            ITokenService tokenService,
            UserManager<User> userManager,
            IUserRepository userRepository,
            IEmailService emailService,
            IVerificationCodeRepository verificationCodeRepository,
            IConfiguration configuration)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _userRepository = userRepository;
            _emailService = emailService;
            _verificationCodeRepository = verificationCodeRepository;
            _configuration = configuration;
            _verificationCodeExpirationTimeInMinutes = _configuration.GetValue<int>("VerificationCode:ExpirationTimeInMinutes");
        }

        // ---------------------------
        // LOGIN
        // ---------------------------
        public async Task<(string token, int userId)> LoginAsync(LoginDTO loginDTO, HttpContext httpContext)
        {
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "IP desconocida";
            var user = await _userRepository.GetByEmailAsync(loginDTO.Email);

            if (user == null)
                throw new UnauthorizedAccessException("Credenciales inválidas.");

            if (!user.EmailConfirmed)
                throw new InvalidOperationException("El correo electrónico del usuario no ha sido confirmado.");

            var result = await _userRepository.CheckPasswordAsync(user, loginDTO.Password);
            if (!result)
                throw new UnauthorizedAccessException("Credenciales inválidas.");

            string roleName = await _userRepository.GetUserRoleAsync(user)
                ?? throw new InvalidOperationException("El usuario no tiene un rol asignado.");

            var token = _tokenService.GenerateToken(user, roleName, loginDTO.RememberMe);
            return (token, user.Id);
        }

        // ---------------------------
        // REGISTER
        // ---------------------------
        public async Task<string> RegisterAsync(RegisterDTO registerDTO, HttpContext httpContext)
        {
            bool isRegistered = await _userRepository.ExistsByEmailAsync(registerDTO.Email);
            if (isRegistered)
                throw new InvalidOperationException("El usuario ya está registrado.");

            isRegistered = await _userRepository.ExistsByRutAsync(registerDTO.Rut);
            if (isRegistered)
                throw new InvalidOperationException("El RUT ya está registrado.");

            var user = registerDTO.Adapt<User>();
            user.UserName = registerDTO.Email;
            var result = await _userRepository.CreateAsync(user, registerDTO.Password);

            if (!result)
                throw new Exception("Error al registrar el usuario.");

            string code = new Random().Next(100000, 999999).ToString();
            var verificationCode = new VerificationCode
            {
                UserId = user.Id,
                Code = code,
                CodeType = CodeType.EmailVerification,
                ExpiryDate = DateTime.UtcNow.AddMinutes(_verificationCodeExpirationTimeInMinutes),
                CreatedAt = DateTime.UtcNow
            };

            await _verificationCodeRepository.CreateAsync(verificationCode);
            await _emailService.SendVerificationCodeEmailAsync(registerDTO.Email, code);

            return "Se ha enviado un código de verificación a su correo electrónico.";
        }

        // ---------------------------
        // REGISTER ADMIN
        // ---------------------------
        public async Task<string> RegisterAdminAsync(RegisterDTO registerDTO, HttpContext httpContext)
        {
            Log.Information("Intentando registrar un nuevo administrador con email: {Email}", registerDTO.Email);

            bool isRegistered = await _userRepository.ExistsByEmailAsync(registerDTO.Email);
            if (isRegistered)
                throw new InvalidOperationException("El administrador ya está registrado.");

            isRegistered = await _userRepository.ExistsByRutAsync(registerDTO.Rut);
            if (isRegistered)
                throw new InvalidOperationException("El RUT ya está registrado.");

            var user = registerDTO.Adapt<User>();
            user.UserName = registerDTO.Email;


            var result = await _userRepository.CreateAsync(user, registerDTO.Password);
            if (!result)
                throw new Exception("Error al registrar el administrador.");

            // 🔹 Asignar el rol Admin
            await _userManager.AddToRoleAsync(user, "Admin");

            string code = new Random().Next(100000, 999999).ToString();
            var verificationCode = new VerificationCode
            {
                UserId = user.Id,
                Code = code,
                CodeType = CodeType.EmailVerification,
                ExpiryDate = DateTime.UtcNow.AddMinutes(_verificationCodeExpirationTimeInMinutes),
                CreatedAt = DateTime.UtcNow
            };

            await _verificationCodeRepository.CreateAsync(verificationCode);

            try
            {
                await _emailService.SendVerificationCodeEmailAsync(user.Email!, code);
                Log.Information("Correo de bienvenida + verificación enviado exitosamente al administrador con email: {Email}", user.Email);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "No se pudo enviar el correo de verificación al administrador con email: {Email}", user.Email);
            }

            return "Administrador registrado exitosamente. Se ha enviado un código de verificación a su correo electrónico.";
        }

        // ---------------------------
        // RESEND EMAIL VERIFICATION
        // ---------------------------
        public async Task<string> ResendEmailVerificationCodeAsync(ResendEmailVerificationCodeDTO dto)
        {
            User? user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
                throw new KeyNotFoundException("El usuario no existe.");

            if (user.EmailConfirmed)
                throw new InvalidOperationException("El correo electrónico ya ha sido verificado.");

            VerificationCode? verificationCode =
                await _verificationCodeRepository.GetLatestByUserIdAsync(user.Id, CodeType.EmailVerification);

            if (verificationCode == null)
            {
                string newCode = new Random().Next(100000, 999999).ToString();
                var newVerificationCode = new VerificationCode
                {
                    UserId = user.Id,
                    Code = newCode,
                    CodeType = CodeType.EmailVerification,
                    ExpiryDate = DateTime.UtcNow.AddMinutes(_verificationCodeExpirationTimeInMinutes),
                    CreatedAt = DateTime.UtcNow
                };

                await _verificationCodeRepository.CreateAsync(newVerificationCode);
                await _emailService.SendVerificationCodeEmailAsync(user.Email!, newCode);

                return "Se ha generado y enviado un nuevo código de verificación.";
            }

            var expirationTime = verificationCode.CreatedAt.AddMinutes(_verificationCodeExpirationTimeInMinutes);
            if (expirationTime > DateTime.UtcNow)
                throw new TimeoutException("Debe esperar antes de solicitar un nuevo código.");

            string updatedCode = new Random().Next(100000, 999999).ToString();
            verificationCode.Code = updatedCode;
            verificationCode.ExpiryDate = DateTime.UtcNow.AddMinutes(_verificationCodeExpirationTimeInMinutes);

            await _verificationCodeRepository.UpdateAsync(verificationCode);
            await _emailService.SendVerificationCodeEmailAsync(user.Email!, updatedCode);

            return "Se ha reenviado un nuevo código de verificación a su correo electrónico.";
        }

        // ---------------------------
        // VERIFY EMAIL
        // ---------------------------
        public async Task<string> VerifyEmailAsync(VerifyEmailDTO verifyEmailDTO)
        {
            User? user = await _userRepository.GetByEmailAsync(verifyEmailDTO.Email);
            if (user == null)
                throw new KeyNotFoundException("El usuario no existe.");

            if (user.EmailConfirmed)
                throw new InvalidOperationException("El correo electrónico ya ha sido verificado.");

            VerificationCode? verificationCode =
                await _verificationCodeRepository.GetLatestByUserIdAsync(user.Id, CodeType.EmailVerification);

            if (verificationCode == null)
                throw new KeyNotFoundException("El código de verificación no existe.");

            if (verificationCode.Code != verifyEmailDTO.VerificationCode || DateTime.UtcNow >= verificationCode.ExpiryDate)
                throw new ArgumentException("El código de verificación es incorrecto o ha expirado.");

            bool emailConfirmed = await _userRepository.ConfirmEmailAsync(user.Email!);
            if (!emailConfirmed)
                throw new Exception("Error al confirmar el correo electrónico.");

            var roles = await _userManager.GetRolesAsync(user);

            // 🔹 Solo un correo de bienvenida según el rol
            if (roles.Contains("Admin"))
            {
                await _emailService.SendWelcomeAdminAsync(user.Email!);
            }
            else
            {
                await _emailService.SendWelcomeEmailAsync(user.Email!);
            }

            await _verificationCodeRepository.DeleteByUserIdAsync(user.Id, CodeType.EmailVerification);

            return "¡Ya puedes iniciar sesión y disfrutar de todos los beneficios de Tienda UCN!";
        }

        // ---------------------------
        // FORGOT PASSWORD
        // ---------------------------
        public async Task<string> SendForgotPasswordCodeAsync(ForgotPasswordRequestDTO dto)
        {
            User? user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
                throw new KeyNotFoundException("El usuario no existe.");

            string code = new Random().Next(100000, 999999).ToString();
            var verificationCode = new VerificationCode
            {
                UserId = user.Id,
                Code = code,
                CodeType = CodeType.ForgotPassword,
                ExpiryDate = DateTime.UtcNow.AddMinutes(_verificationCodeExpirationTimeInMinutes),
                CreatedAt = DateTime.UtcNow
            };

            await _verificationCodeRepository.CreateAsync(verificationCode);
            await _emailService.SendForgotPasswordEmailAsync(user.Email!, code);

            return "Se ha enviado un código de recuperación a su correo electrónico.";
        }

        public async Task<string> VerifyForgotPasswordCodeAsync(VerifyForgotPasswordCodeDTO dto)
        {
            User? user = await _userRepository.GetByEmailAsync(dto.Email);

            Log.Information("NewPassword: {NewPassword}, ConfirmPassword: {ConfirmPassword}", dto.NewPassword, dto.ConfirmNewPassword);

            if (user == null)
                throw new KeyNotFoundException("El usuario no existe.");

            VerificationCode? verificationCode =
                await _verificationCodeRepository.GetLatestByUserIdAsync(user.Id, CodeType.ForgotPassword);

            if (verificationCode == null)
                throw new KeyNotFoundException("No existe un código de recuperación activo.");

            if (verificationCode.Code != dto.VerificationCode || DateTime.UtcNow >= verificationCode.ExpiryDate)
                throw new ArgumentException("El código de recuperación es incorrecto o ha expirado.");

            await _verificationCodeRepository.DeleteByUserIdAsync(user.Id, CodeType.ForgotPassword);
            if (dto.NewPassword.Trim() != dto.ConfirmNewPassword.Trim())
                throw new ArgumentException("Las nuevas contraseñas no coinciden.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded)
                throw new Exception("Error al restablecer la contraseña.");

            return "Código de recuperación verificado correctamente, puedes restablecer la contraseña.";
        }

        // ---------------------------
        // DELETE UNCONFIRMED USERS
        // ---------------------------
        public Task<int> DeleteUnconfirmedAsync()
        {
            throw new NotImplementedException();
        }
    }
}