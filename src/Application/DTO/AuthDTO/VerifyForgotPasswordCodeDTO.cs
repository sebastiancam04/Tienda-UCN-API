namespace Tienda_UCN_api.src.Application.DTO.AuthDTO
{
    public class VerifyForgotPasswordCodeDTO
    {
        public string Email { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}