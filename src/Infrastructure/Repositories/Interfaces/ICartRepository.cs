using Tienda_UCN_api.src.Domain.Models;

namespace Tienda_UCN_api.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio del carrito de compras
    /// </summary>
    public interface ICartRepository
    {

        /// <summary>
        /// Busca un carrito de compras.
        /// </summary>
        /// <param name="buyerId">ID del comprador.</param>
        /// <param name="userId">ID del usuario si es que está autenticado</param>
        /// <returns>Tarea que representa la operación asincrónica retornando un objeto Cart.</returns>
        Task<Cart?> FindAsync(string buyerId, int? userId);

        /// <summary>
        /// Obtiene un carrito por ID de usuario.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Tarea que representa la operación asincrónica retornando un objeto Cart.</returns>
        Task<Cart?> GetByUserIdAsync(int userId);

        /// <summary>
        /// Obtiene un carrito anónimo.
        /// </summary>
        /// <param name="buyerId">ID del comprador.</param>
        /// <returns>Tarea que representa la operación asincrónica retornando un objeto Cart.</returns>
        Task<Cart?> GetAnonymousAsync(string buyerId);

        /// <summary>
        /// Crea un nuevo carrito de compras.
        /// </summary>
        /// <param name="buyerId">ID del comprador.</param>
        /// <param name="userId">ID del usuario si es que está autenticado</param>
        /// <returns>Tarea que representa la operación asincrónica retornando un objeto Cart.</returns>
        Task<Cart> CreateAsync(string buyerId, int? userId = null);

        /// <summary>
        /// Actualiza el carrito de compras.
        /// </summary>
        /// <param name="cart">El carrito a actualizar.</param>
        /// <returns>Tarea que representa la operación asincrónica.</returns>
        Task UpdateAsync(Cart cart);

        /// <summary>
        /// Elimina el carrito de compras.
        /// </summary>
        /// <param name="cart">El carrito a eliminar.</param>
        /// <returns>Tarea que representa la operación asincrónica.</returns>
        /// <param name="cart">El carrito a eliminar.</param>
        /// <returns>Tarea que representa la operación asincrónica.</returns>
        Task DeleteAsync(Cart cart);

        /// <summary>
        /// Agrega un artículo al carrito de compras.
        /// </summary>
        /// <param name="cart">El carrito al que se agregará el artículo.</param>
        /// <param name="cartItem">El artículo del carrito a agregar.</param>
        /// <returns>Tarea que representa la operación asincrónica.</returns>
        Task AddItemAsync(Cart cart, CartItem cartItem);

        /// <summary>
        /// Elimina un artículo del carrito de compras.
        /// </summary>
        /// <param name="cartItem">El artículo del carrito a eliminar.</param>
        /// <returns>Tarea que representa la operación asincrónica.</returns>
        Task RemoveItemAsync(CartItem cartItem);
    }
}