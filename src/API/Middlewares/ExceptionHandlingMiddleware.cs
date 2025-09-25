using Serilog;
using System.Security;
using System.Text.Json;
using Tienda_UCN_api.src.Application.DTO.BaseResponse;

namespace Tienda_UCN_api.src.Infrastructure.Middlewares
{
    /// <summary>
    /// Middleware para el manejo de excepciones en la aplicación.
    /// </summary>
    public class ExceptionHandlingMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        /// <summary>
        /// Método que se invoca en cada solicitud HTTP.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Pasamos al siguiente middleware en la cadena si no hay excepciones
                await _next(context);
            }
            catch (Exception ex)
            {
                //Capturamos excepciones no controladas y generación de un ID de seguimiento único
                var traceId = Guid.NewGuid().ToString();
                context.Response.Headers["trace-id"] = traceId;

                var (statusCode, title) = MapExceptionToStatus(ex);

                // Creamos un objeto ProblemDetails para la respuesta
                ErrorDetail error = new ErrorDetail(title, ex.Message);

                Log.Error(ex, "Excepción no controlada. Trace ID: {TraceId}", traceId);

                // Configuramos la respuesta HTTP como JSON
                context.Response.ContentType = "application/json";
                // Establecemos el código de estado HTTP adecuado
                context.Response.StatusCode = statusCode;

                // Serializamos el objeto ProblemDetails a JSON y lo escribimos en la respuesta
                var json = JsonSerializer.Serialize(
                    error,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );

                // Acá se escribe la respuesta al cliente
                await context.Response.WriteAsync(json);
            }
        }

        private static (int, string) MapExceptionToStatus(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException _ => (StatusCodes.Status401Unauthorized, "No autorizado"),
                ArgumentNullException _ => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
                KeyNotFoundException _ => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
                InvalidOperationException _ => (StatusCodes.Status409Conflict, "Conflicto de operación"),
                FormatException _ => (StatusCodes.Status400BadRequest, "Formato inválido"),
                SecurityException _ => (StatusCodes.Status403Forbidden, "Acceso prohibido"),
                ArgumentOutOfRangeException _ => (StatusCodes.Status400BadRequest, "Argumento fuera de rango"),
                ArgumentException _ => (StatusCodes.Status400BadRequest, "Argumento inválido"),
                TimeoutException _ => (StatusCodes.Status429TooManyRequests, "Demasiadas solicitudes"),
                JsonException _ => (StatusCodes.Status400BadRequest, "JSON inválido"),
                _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor"),
            };
        }
    }
}