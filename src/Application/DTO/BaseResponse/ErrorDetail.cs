namespace Tienda_UCN_api.src.Application.DTO.BaseResponse
{
    /// <summary>
    /// Clase que representa los detalles de un error.
    /// </summary>
    /// <param name="message">Mensaje de error.</param>
    /// <param name="details">Detalles adicionales del error (opcional).</param>
    public class ErrorDetail(string message, string? details = null)
    {
        public string Message { get; set; } = message;
        public string? Details { get; set; } = details;
    }
}