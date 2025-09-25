using Tienda_UCN_api.src.Application.DTO.CartDTO;
using Tienda_UCN_api.src.Application.DTO.OrderDTO;
using Tienda_UCN_api.src.Application.DTO.ProductDTO;

namespace Tienda_UCN_api.src.Application.Services.Interfaces
{
    public interface IOrderService
    {
        /// <summary>
        /// Crea una nueva orden y vacía el carrito de compras.
        /// </summary>
        /// <param name="userId">Id del usuario autenticado</param>
        /// <returns>Crea una nueva orden y vacía el carrito de compras.</returns>
        Task<string> CreateAsync(int userId);

        /// <summary>
        /// Obtiene los detalles de una orden.
        /// </summary>
        /// <param name="orderCode">Código de la orden</param>
        /// <returns>El detalle de la orden</returns>
        Task<OrderDetailDTO> GetDetailAsync(string orderCode);

        /// <summary>
        /// Obtiene una lista de órdenes para un usuario específico.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda</param>
        /// <param name="userId">Id del usuario al que pertenecen las órdenes</param>
        /// <returns>Ordenes del usuario</returns>
        Task<ListedOrderDetailDTO> GetByUserIdAsync(SearchParamsDTO searchParams, int userId);
    }
}