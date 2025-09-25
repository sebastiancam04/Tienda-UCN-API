using Mapster;
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
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IVerificationCodeRepository _verificationCodeRepository;
        private readonly int _verificationCodeExpirationTimeInMinutes;

        public UserService(ITokenService tokenService, IUserRepository userRepository, IEmailService emailService, IVerificationCodeRepository verificationCodeRepository, IConfiguration configuration)
        {
            _tokenService = tokenService;
            _userRepository = userRepository;
            _emailService = emailService;
            _verificationCodeRepository = verificationCodeRepository;
            _configuration = configuration;
            _verificationCodeExpirationTimeInMinutes = _configuration.GetValue<int>("VerificationCode:ExpirationTimeInMinutes");
        }

        /// <summary>
        /// Elimina usuarios no confirmados.
        /// </summary>
        /// <returns>Número de usuarios eliminados</returns>
        public async Task<int> DeleteUnconfirmedAsync()
        {
            return await _userRepository.DeleteUnconfirmedAsync();
        }

        /// <summary>
        /// Inicia sesión con el usuario proporcionado.
        /// </summary>
        public async Task<(string token, int userId)> LoginAsync(LoginDTO loginDTO, HttpContext httpContext)
        {
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "IP desconocida";
            var user = await _userRepository.GetByEmailAsync(loginDTO.Email);

            if (user == null)
            {
                Log.Warning($"Intento de inicio de sesión fallido para el usuario: {loginDTO.Email} desde la IP: {ipAddress}");
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }

            if (!user.EmailConfirmed)
            {
                Log.Warning($"Intento de inicio de sesión fallido para el usuario: {loginDTO.Email} desde la IP: {ipAddress} - Correo no confirmado.");
                throw new InvalidOperationException("El correo electrónico del usuario no ha sido confirmado.");
            }

            var result = await _userRepository.CheckPasswordAsync(user, loginDTO.Password);
            if (!result)
            {
                Log.Warning($"Intento de inicio de sesión fallido para el usuario: {loginDTO.Email} desde la IP: {ipAddress}");
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }

            string roleName = await _userRepository.GetUserRoleAsync(user) ?? throw new InvalidOperationException("El usuario no tiene un rol asignado.");

            // Generamos el token
            Log.Information($"Inicio de sesión exitoso para el usuario: {loginDTO.Email} desde la IP: {ipAddress}");
            var token = _tokenService.GenerateToken(user, roleName, loginDTO.RememberMe);
            return (token, user.Id);
        }

        /// <summary>
        /// Registra un nuevo usuario.
        /// </summary>
        public async Task<string> RegisterAsync(RegisterDTO registerDTO, HttpContext httpContext)
        {
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "IP desconocida";
            Log.Information($"Intento de registro de nuevo usuario: {registerDTO.Email} desde la IP: {ipAddress}");

            bool isRegistered = await _userRepository.ExistsByEmailAsync(registerDTO.Email);
            if (isRegistered)
            {
                Log.Warning($"El usuario con el correo {registerDTO.Email} ya está registrado.");
                throw new InvalidOperationException("El usuario ya está registrado.");
            }
            isRegistered = await _userRepository.ExistsByRutAsync(registerDTO.Rut);
            if (isRegistered)
            {
                Log.Warning($"El usuario con el RUT {registerDTO.Rut} ya está registrado.");
                throw new InvalidOperationException("El RUT ya está registrado.");
            }

            var user = registerDTO.Adapt<User>();
            user.UserName = registerDTO.Email;
            var result = await _userRepository.CreateAsync(user, registerDTO.Password);
            if (!result)
            {
                Log.Warning($"Error al registrar el usuario: {registerDTO.Email}");
                throw new Exception("Error al registrar el usuario.");
            }

            Log.Information($"Registro exitoso para el usuario: {registerDTO.Email} desde la IP: {ipAddress}");

            string code = new Random().Next(100000, 999999).ToString();
            var verificationCode = new VerificationCode
            {
                UserId = user.Id,
                Code = code,
                CodeType = CodeType.EmailVerification,
                ExpiryDate = DateTime.UtcNow.AddMinutes(_verificationCodeExpirationTimeInMinutes),
                CreatedAt = DateTime.UtcNow
            };

            var createdVerificationCode = await _verificationCodeRepository.CreateAsync(verificationCode);
            Log.Information($"Código de verificación generado para el usuario: {registerDTO.Email} - Código: {createdVerificationCode.Code}");

            await _emailService.SendVerificationCodeEmailAsync(registerDTO.Email, createdVerificationCode.Code);
            Log.Information($"Se ha enviado un código de verificación al correo electrónico: {registerDTO.Email}");
            return "Se ha enviado un código de verificación a su correo electrónico.";
        }

        /// <summary>
        /// Reenvía el código de verificación al correo electrónico del usuario.
        /// </summary>
        public async Task<string> ResendEmailVerificationCodeAsync(ResendEmailVerificationCodeDTO resendEmailVerificationCodeDTO)
        {
            var currentTime = DateTime.UtcNow;
            User? user = await _userRepository.GetByEmailAsync(resendEmailVerificationCodeDTO.Email);

            if (user == null)
            {
                Log.Warning($"El usuario con el correo {resendEmailVerificationCodeDTO.Email} no existe.");
                throw new KeyNotFoundException("El usuario no existe.");
            }

            if (user.EmailConfirmed)
            {
                Log.Warning($"El usuario con el correo {resendEmailVerificationCodeDTO.Email} ya ha verificado su correo electrónico.");
                throw new InvalidOperationException("El correo electrónico ya ha sido verificado.");
            }

            VerificationCode? verificationCode = await _verificationCodeRepository.GetLatestByUserIdAsync(user.Id, CodeType.EmailVerification);

            // Caso: no existe código previo → crear uno nuevo
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

                Log.Information($"Nuevo código de verificación generado y enviado a {resendEmailVerificationCodeDTO.Email}");
                return "Se ha generado y enviado un nuevo código de verificación.";
            }

            // Caso: ya existe un código → verificar expiración
            var expirationTime = verificationCode.CreatedAt.AddMinutes(_verificationCodeExpirationTimeInMinutes);
            if (expirationTime > currentTime)
            {
                int remainingSeconds = (int)(expirationTime - currentTime).TotalSeconds;
                Log.Warning($"El usuario {resendEmailVerificationCodeDTO.Email} ha solicitado un reenvío demasiado pronto.");
                throw new TimeoutException($"Debe esperar {remainingSeconds} segundos para solicitar un nuevo código de verificación.");
            }

            // Regenerar código y actualizar
            string updatedCode = new Random().Next(100000, 999999).ToString();
            verificationCode.Code = updatedCode;
            verificationCode.ExpiryDate = DateTime.UtcNow.AddMinutes(_verificationCodeExpirationTimeInMinutes);

            await _verificationCodeRepository.UpdateAsync(verificationCode);
            await _emailService.SendVerificationCodeEmailAsync(user.Email!, updatedCode);

            Log.Information($"Nuevo código de verificación reenviado a {resendEmailVerificationCodeDTO.Email}");
            return "Se ha reenviado un nuevo código de verificación a su correo electrónico.";
        }

        /// <summary>
        /// Verifica el correo electrónico del usuario.
        /// </summary>
        public async Task<string> VerifyEmailAsync(VerifyEmailDTO verifyEmailDTO)
        {
            User? user = await _userRepository.GetByEmailAsync(verifyEmailDTO.Email);
            if (user == null)
            {
                Log.Warning($"El usuario con el correo {verifyEmailDTO.Email} no existe.");
                throw new KeyNotFoundException("El usuario no existe.");
            }
            if (user.EmailConfirmed)
            {
                Log.Warning($"El usuario con el correo {verifyEmailDTO.Email} ya ha verificado su correo electrónico.");
                throw new InvalidOperationException("El correo electrónico ya ha sido verificado.");
            }

            CodeType codeType = CodeType.EmailVerification;
            VerificationCode? verificationCode = await _verificationCodeRepository.GetLatestByUserIdAsync(user.Id, codeType);

            if (verificationCode == null)
            {
                Log.Warning($"No se encontró un código de verificación para el usuario: {verifyEmailDTO.Email}");
                throw new KeyNotFoundException("El código de verificación no existe.");
            }

            if (verificationCode.Code != verifyEmailDTO.VerificationCode || DateTime.UtcNow >= verificationCode.ExpiryDate)
            {
                int attempsCountUpdated = await _verificationCodeRepository.IncreaseAttemptsAsync(user.Id, codeType);
                Log.Warning($"Código de verificación incorrecto o expirado para {verifyEmailDTO.Email}. Intentos: {attempsCountUpdated}");

                if (attempsCountUpdated >= 5)
                {
                    bool codeDeleteResult = await _verificationCodeRepository.DeleteByUserIdAsync(user.Id, codeType);
                    if (codeDeleteResult)
                    {
                        bool userDeleteResult = await _userRepository.DeleteAsync(user.Id);
                        if (userDeleteResult)
                        {
                            throw new ArgumentException("Se ha alcanzado el límite de intentos. El usuario ha sido eliminado.");
                        }
                    }
                }

                if (DateTime.UtcNow >= verificationCode.ExpiryDate)
                {
                    throw new ArgumentException("El código de verificación ha expirado.");
                }
                else
                {
                    throw new ArgumentException($"El código de verificación es incorrecto, quedan {5 - attempsCountUpdated} intentos.");
                }
            }

            bool emailConfirmed = await _userRepository.ConfirmEmailAsync(user.Email!);
            if (emailConfirmed)
            {
                bool codeDeleteResult = await _verificationCodeRepository.DeleteByUserIdAsync(user.Id, codeType);
                if (codeDeleteResult)
                {
                    await _emailService.SendWelcomeEmailAsync(user.Email!);
                    Log.Information($"Correo electrónico confirmado para {verifyEmailDTO.Email}");
                    return "!Ya puedes iniciar sesión y disfrutar de todos los beneficios de Tienda UCN!";
                }
                throw new Exception("Error al confirmar el correo electrónico.");
            }

            throw new Exception("Error al verificar el correo electrónico.");
        }
    }
}