using Serilog;
using Tienda_UCN_api.src.Application.Services.Interfaces;

namespace Tienda_UCN_api.src.API.Middlewares
{
    /// <summary>
    /// Middleware para manejar operaciones con el carrito de compras con cookies http-only
    /// </summary>
    public class CartMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly int _cookieExpirationDays;

        public CartMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
            _cookieExpirationDays = _configuration.GetValue<int?>("CookieExpirationDays") ?? throw new InvalidOperationException("La expiración en días de la cookie no está configurada.");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var buyerId = context.Request.Cookies["BuyerId"];

            if (string.IsNullOrEmpty(buyerId))
            {
                Log.Information("No se encontró la cookie de comprador, creando una nueva.");
                buyerId = Guid.CreateVersion7().ToString();

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax, //permite el envío de cookies en solicitudes de origen cruzado
                    Expires = DateTimeOffset.UtcNow.AddDays(_cookieExpirationDays), //extraemos del appsettings la expiración, si no se setea una se perderá el carrito
                    Path = "/", //las cookies serán accesibles desde cualquier ruta
                };
                context.Response.Cookies.Append("BuyerId", buyerId, cookieOptions);
                Log.Information("Se creó una nueva cookie de comprador: {BuyerId}", buyerId);
            }
            context.Items["BuyerId"] = buyerId; // almacenamos el buyerId en el contexto para ser usado en todo el pipeline

            await _next(context);
        }
    }
}