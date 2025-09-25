using Tienda_UCN_api.src.Application.DTO.BaseResponse;

namespace Tienda_UCN_api.src.Application.DTO
{
    /// <summary>
    /// Clase que representa una respuesta genérica de la aplicación.
    /// </summary>
    /// <typeparam name="T">Tipo de datos que contiene la respuesta.</typeparam>
    /// <param name="message">Mensaje de la respuesta.</param>
    /// <param name="data">Datos de la respuesta (opcional).</param>
    public class GenericResponse<T>(string message, T? data = default)
    {
        public string Message { get; set; } = message;
        public T? Data { get; set; } = data;
    }
}