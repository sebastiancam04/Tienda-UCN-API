using Tienda_UCN_api.src.Application.DTO.ProductDTO;
using Tienda_UCN_api.src.Domain.Models;

namespace Tienda_UCN_api.src.Infrastructure.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        /// <summary>
        /// Obtiene una orden por su código.
        /// </summary>
        /// <param name="orderCode">El código de la orden.</param>
        /// <returns>La orden correspondiente o null si no se encuentra.</returns>
        Task<Order?> GetByCodeAsync(string orderCode);

        /// <summary>
        /// Crea una nueva orden.
        /// </summary>
        /// <param name="order">La orden a crear.</param>
        /// <returns>Booleano que indica si la creación fue exitosa.</returns>
        Task<bool> CreateAsync(Order order);

        /// <summary>
        /// Verifica si un código de orden existe.
        /// </summary>
        /// <param name="orderCode">Código de la orden</param>
        /// <returns>True si el código existe, de lo contrario false.</returns>
        Task<bool> CodeExistsAsync(string orderCode);

        /// <summary>
        /// Obtiene las órdenes de un usuario por su ID.
        /// </summary>
        /// <param name="userId">El ID del usuario.</param>
        /// <param name="searchParams">Los parámetros de búsqueda.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con una lista de órdenes y el conteo total de órdenes.</returns>
        Task<(IEnumerable<Order> orders, int totalCount)> GetByUserIdAsync(SearchParamsDTO searchParams, int userId);
    }
}