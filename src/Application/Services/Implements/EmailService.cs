using Resend;
using Tienda_UCN_api.src.Application.Services.Interfaces;

namespace Tienda_UCN_api.src.Application.Services.Implements
{
    /// <summary>
    /// Servicio para enviar correos electrónicos.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailService(IResend resend, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _resend = resend;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Envía un código de verificación al correo electrónico del usuario (registro).
        /// </summary>
        public async Task SendVerificationCodeEmailAsync(string email, string code)
        {
            var htmlBody = await LoadTemplate("VerificationCode", code);

            var message = new EmailMessage
            {
                To = email,
                Subject = _configuration["EmailConfiguration:VerificationSubject"]
                          ?? throw new ArgumentNullException("El asunto del correo de verificación no puede ser nulo."),
                From = _configuration["EmailConfiguration:From"]
                       ?? throw new ArgumentNullException("La configuración de 'From' no puede ser nula."),
                HtmlBody = htmlBody
            };
            await _resend.EmailSendAsync(message);
        }

        /// <summary>
        /// Envía un correo electrónico de bienvenida al usuario.
        /// </summary>
        public async Task SendWelcomeEmailAsync(string email)
        {
            var htmlBody = await LoadTemplate("Welcome", null);

            var message = new EmailMessage
            {
                To = email,
                Subject = _configuration["EmailConfiguration:WelcomeSubject"]
                          ?? throw new ArgumentNullException("El asunto del correo de bienvenida no puede ser nulo."),
                From = _configuration["EmailConfiguration:From"]
                       ?? throw new ArgumentNullException("La configuración de 'From' no puede ser nula."),
                HtmlBody = htmlBody
            };

            await _resend.EmailSendAsync(message);
        }

        /// <summary>
        /// Envía un correo de recuperación de contraseña con código.
        /// </summary>
        public async Task SendForgotPasswordEmailAsync(string email, string code)
        {
            var htmlBody = await LoadTemplate("ForgotPasswordTemplate", code);

            var message = new EmailMessage
            {
                To = email,
                Subject = _configuration["EmailConfiguration:ForgotPasswordSubject"]
                          ?? "Recuperación de contraseña - Tienda UCN",
                From = _configuration["EmailConfiguration:From"]
                       ?? throw new ArgumentNullException("La configuración de 'From' no puede ser nula."),
                HtmlBody = htmlBody
            };

            await _resend.EmailSendAsync(message);
        }

        /// <summary>
        /// Carga una plantilla de correo electrónico desde el sistema de archivos y reemplaza el marcador de código.
        /// </summary>
        private async Task<string> LoadTemplate(string templateName, string? code)
        {
            var templatePath = Path.Combine(
                _webHostEnvironment.ContentRootPath,
                "Src", "Application", "Templates", "Email",
                $"{templateName}.html");

            var html = await File.ReadAllTextAsync(templatePath);

            if (!string.IsNullOrEmpty(code))
                html = html.Replace("{{CODE}}", code);

            return html;
        }
    }
}