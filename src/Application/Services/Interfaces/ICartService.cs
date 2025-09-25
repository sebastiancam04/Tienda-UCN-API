using Tienda_UCN_api.src.Application.DTO.CartDTO;

namespace Tienda_UCN_api.src.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio del carrito de compras
    /// </summary>
    public interface ICartService
    {
        ///<summary>
        /// Agrega un artículo al carrito de compras.
        ///</summary>
        /// <param name="buyerId">ID del comprador.</param>
        /// <param name="productId">ID del producto.</param>
        /// <param name="quantity">Cantidad del producto.</param>
        /// <param name="userId">ID del usuario si es que está autenticado</param>
        /// <returns>Tarea que representa la operación asincrónica que retorna un objeto del tipo CartDTO.</returns>
        Task<CartDTO> AddItemAsync(string buyerId, int productId, int quantity, int? userId = null);

        /// <summary>
        /// Crea un nuevo carrito o devuelve uno existente.
        /// </summary>
        /// <param name="buyerId">ID del comprador.</param>
        /// <param name="userId">ID del usuario si es que está autenticado</param>
        /// <returns>Tarea que representa la operación asincrónica retornando un objeto CartDTO.</returns>
        Task<CartDTO> CreateOrGetAsync(string buyerId, int? userId = null);

        /// <summary>
        /// Elimina un artículo del carrito de compras.
        /// </summary>
        /// <param name="buyerId">ID del comprador.</param>
        /// <param name="productId">ID del producto.</param>
        /// <param name="userId">ID del usuario si es que está autenticado</param>
        /// <returns>Tarea que representa la operación asincrónica retornando un objeto CartDTO.</returns>
        Task<CartDTO> RemoveItemAsync(string buyerId, int productId, int? userId = null);

        /// <summary>
        /// Limpia el carrito de compras.
        /// </summary>
        /// <param name="buyerId">ID del comprador.</param>
        /// <param name="userId">ID del usuario si es que está autenticado</param>
        /// <returns>Tarea que representa la operación asincrónica retornando un objeto CartDTO.</returns>
        Task<CartDTO> ClearAsync(string buyerId, int? userId = null);

        /// <summary>
        /// Actualiza la cantidad de un artículo en el carrito de compras.
        /// </summary>
        /// <param name="buyerId">ID del comprador.</param>
        /// <param name="productId">ID del producto.</param>
        /// <param name="quantity">Nueva cantidad del producto.</param>
        /// <param name="userId">ID del usuario si es que está autenticado</param>
        /// <returns>Tarea que representa la operación asincrónica retornando un objeto CartDTO.</returns>
        Task<CartDTO> UpdateItemQuantityAsync(string buyerId, int productId, int quantity, int? userId = null);

        /// <summary>
        /// Asocia el carrito de compras con un usuario.
        /// </summary>
        /// <param name="buyerId">ID del comprador.</param>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Tarea que representa la operación asincrónica.</returns>
        Task AssociateWithUserAsync(string buyerId, int userId);

        /// <summary>
        /// Procesa el pago ajustando las cantidades de los productos en el carrito.
        /// </summary>
        /// <param name="buyerId">ID del comprador.</param>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Tarea que representa la operación asincrónica retornando un objeto CartDTO.</returns>
        Task<CartDTO> CheckoutAsync(string buyerId, int? userId);
    }
}