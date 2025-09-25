using Mapster;
using Serilog;
using Tienda_UCN_api.src.Application.DTO.CartDTO;
using Tienda_UCN_api.src.Application.Services.Interfaces;
using Tienda_UCN_api.src.Domain.Models;
using Tienda_UCN_api.src.Infrastructure.Repositories.Interfaces;

namespace Tienda_UCN_api.src.Application.Services.Implements
{
    /// <summary>
    /// Servicio para manejar operaciones del carrito de compras
    /// </summary>
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        public CartService(ICartRepository cartRepository, IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        ///<summary>
        /// Agrega un artículo al carrito de compras.
        ///</summary>
        /// <param name="buyerId">ID del comprador.</param>
        /// <param name="productId">ID del producto.</param>
        /// <param name="quantity">Cantidad del producto.</param>
        /// <param name="userId">ID del usuario si es que está autenticado</param>
        /// <returns>Tarea que representa la operación asincrónica que retorna un objeto del tipo CartDTO.</returns>
        public async Task<CartDTO> AddItemAsync(string buyerId, int productId, int quantity, int? userId = null)
        {
            Cart? cart = await _cartRepository.FindAsync(buyerId, userId);
            Product? product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
            {
                Log.Information("El producto con ID {ProductId} no existe.", productId);
                throw new KeyNotFoundException("El producto no existe.");
            }

            if (product.Stock < quantity)
            {
                Log.Information("El producto con ID {ProductId} no tiene suficiente stock.", productId);
                throw new ArgumentException("No hay suficiente stock del producto.");
            }

            if (cart == null)
            {
                Log.Information("Creando nuevo carrito para buyerId: {BuyerId}", buyerId);
                cart = await _cartRepository.CreateAsync(buyerId, userId);
            }

            var existingProduct = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (existingProduct != null)
            {
                existingProduct.Quantity += quantity;
                Log.Information("Actualizando cantidad del producto {ProductId} en el carrito.", productId);
            }
            else
            {
                var newCartItem = new CartItem
                {
                    ProductId = product.Id,
                    Product = product,
                    CartId = cart.Id,
                    Quantity = quantity,
                };

                await _cartRepository.AddItemAsync(cart, newCartItem);
                Log.Information("Nuevo producto agregado al carrito. ProductId: {ProductId}, Cantidad: {Quantity}", productId, quantity);
            }

            RecalculateCartTotals(cart);
            await _cartRepository.UpdateAsync(cart);
            Log.Information("Carrito guardado exitosamente. CartId: {CartId}", cart.Id);

            return cart.Adapt<CartDTO>();
        }

        /// <summary>
        /// Limpia el carrito de compras.
        /// </summary>
        /// <param name="buyerId">ID del comprador.</param>
        /// <param name="userId">ID del usuario si es que está autenticado</param>
        /// <returns>Tarea que representa la operación asincrónica retornando un objeto CartDTO.</returns>
        public async Task<CartDTO> ClearAsync(string buyerId, int? userId = null)
        {
            Cart? cart = await _cartRepository.FindAsync(buyerId, userId);
            if (cart == null)
            {
                Log.Information("El carrito no existe para el comprador especificado {BuyerId}.", buyerId);
                throw new KeyNotFoundException("El carrito no existe para el comprador especificado.");
            }

            cart.CartItems.Clear();
            RecalculateCartTotals(cart);
            await _cartRepository.UpdateAsync(cart);
            Log.Information("Carrito limpiado exitosamente. CartId: {CartId}", cart.Id);

            return cart.Adapt<CartDTO>();
        }

        /// <summary>
        /// Crea un nuevo carrito o devuelve uno existente.
        /// </summary>
        /// <param name="buyerId">ID del comprador.</param>
        /// <param name="userId">ID del usuario si es que está autenticado</param>
        /// <returns>Tarea que representa la operación asincrónica retornando un objeto CartDTO.</returns>
        public async Task<CartDTO> CreateOrGetAsync(string buyerId, int? userId = null)
        {
            Cart? cart = await _cartRepository.FindAsync(buyerId, userId);

            if (cart == null)
            {
                if (userId.HasValue)
                {
                    var existingUserCart = await _cartRepository.GetByUserIdAsync(userId.Value);
                    if (existingUserCart != null)
                    {
                        Log.Information("Se encontró carrito existente del usuario durante CreateOrGet. UserId: {UserId}", userId);
                        return existingUserCart.Adapt<CartDTO>();
                    }
                }

                cart = await _cartRepository.CreateAsync(buyerId, userId);
                Log.Information("Nuevo carrito creado para buyerId: {BuyerId}, userId: {UserId}", buyerId, userId);
            }

            return cart.Adapt<CartDTO>();
        }

        /// <summary>
        /// Elimina un artículo del carrito de compras.
        /// </summary>
        /// <param name="buyerId">ID del comprador.</param>
        /// <param name="productId">ID del producto.</param>
        /// <param name="userId">ID del usuario si es que está autenticado</param>
        /// <returns>Tarea que representa la operación asincrónica retornando un objeto CartDTO.</returns>
        public async Task<CartDTO> RemoveItemAsync(string buyerId, int productId, int? userId = null)
        {
            Cart? cart = await _cartRepository.FindAsync(buyerId, userId);
            if (cart == null)
            {
                Log.Information("El carrito no existe para el comprador especificado {BuyerId}.", buyerId);
                throw new KeyNotFoundException("El carrito no existe para el comprador especificado.");
            }

            CartItem? itemToRemove = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (itemToRemove == null)
            {
                Log.Information("El artículo no existe en el carrito para el comprador especificado {BuyerId}.", buyerId);
                throw new KeyNotFoundException("El artículo no existe en el carrito.");
            }

            cart.CartItems.Remove(itemToRemove);
            await _cartRepository.RemoveItemAsync(itemToRemove);
            RecalculateCartTotals(cart);
            await _cartRepository.UpdateAsync(cart);

            return cart.Adapt<CartDTO>();
        }

        /// <summary>
        /// Asocia un carrito de compras con un usuario.
        /// </summary>
        public async Task AssociateWithUserAsync(string buyerId, int userId)
        {
            Cart? cart = await _cartRepository.GetAnonymousAsync(buyerId);
            if (cart == null)
            {
                Log.Information("No hay carrito para asociar con buyerId: {BuyerId}", buyerId);
                return;
            }

            var existingUserCart = await _cartRepository.GetByUserIdAsync(userId);

            if (existingUserCart != null)
            {
                foreach (var anonymousItem in cart.CartItems)
                {
                    var existingItem = existingUserCart.CartItems.FirstOrDefault(i => i.ProductId == anonymousItem.ProductId);

                    if (existingItem != null)
                    {
                        existingItem.Quantity += anonymousItem.Quantity;
                    }
                    else
                    {
                        anonymousItem.CartId = existingUserCart.Id;
                        existingUserCart.CartItems.Add(anonymousItem);
                    }
                }

                RecalculateCartTotals(existingUserCart);
                await _cartRepository.UpdateAsync(existingUserCart);
                await _cartRepository.DeleteAsync(cart);

                Log.Information("Carritos fusionados. Carrito anónimo {AnonymousCartId} → Carrito usuario {UserCartId}", cart.Id, existingUserCart.Id);
            }
            else
            {
                cart.UserId = userId;
                await _cartRepository.UpdateAsync(cart);
                Log.Information("Carrito anónimo asociado con usuario. BuyerId: {BuyerId} → UserId: {UserId}", buyerId, userId);
            }
        }

        /// <summary>
        /// Actualiza la cantidad de un artículo en el carrito de compras.
        /// </summary>
        /// <param name="buyerId">ID del comprador.</param>
        /// <param name="productId">ID del producto.</param>
        /// <param name="quantity">Nueva cantidad del producto.</param>
        /// <param name="userId">ID del usuario si es que está autenticado</param>
        /// <returns>Tarea que representa la operación asincrónica retornando un objeto CartDTO.</returns>
        public async Task<CartDTO> UpdateItemQuantityAsync(string buyerId, int productId, int quantity, int? userId = null)
        {
            Cart? cart = await _cartRepository.FindAsync(buyerId, userId);
            if (cart == null)
            {
                Log.Information("El carrito no existe para el comprador especificado {BuyerId}.", buyerId);
                throw new KeyNotFoundException("El carrito no existe para el comprador especificado.");
            }

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                Log.Information("El producto no existe para el ID especificado {ProductId}.", productId);
                throw new KeyNotFoundException("El producto no existe para el ID especificado.");
            }

            var itemToUpdate = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (itemToUpdate == null)
            {
                throw new KeyNotFoundException("Producto del carrito no encontrado");
            }

            if (product.Stock < quantity)
            {
                Log.Information("El producto con ID {ProductId} no tiene suficiente stock para la cantidad solicitada.", productId);
                throw new ArgumentException("Stock insuficiente");
            }

            itemToUpdate.Quantity = quantity;
            RecalculateCartTotals(cart);
            await _cartRepository.UpdateAsync(cart);

            return cart.Adapt<CartDTO>();
        }

        /// <summary>
        /// Recalcula los totales del carrito.
        /// </summary>
        private static void RecalculateCartTotals(Cart cart)
        {
            if (!cart.CartItems.Any())
            {
                cart.SubTotal = 0;
                cart.Total = 0;
                return;
            }

            cart.SubTotal = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity);

            var totalWithDiscounts = cart.CartItems.Sum(ci =>
            {
                var itemTotal = ci.Product.Price * ci.Quantity;
                var discount = ci.Product.Discount;
                return (int)(itemTotal * (1 - (decimal)discount / 100));
            });

            cart.Total = totalWithDiscounts;

            Log.Information("Totales recalculados. SubTotal: {SubTotal}, Total: {Total}", cart.SubTotal, cart.Total);
        }

        /// <summary>
        /// Procesa el pago ajustando las cantidades de los productos en el carrito.
        /// </summary>
        /// <param name="buyerId">ID del comprador.</param>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Tarea que representa la operación asincrónica retornando un objeto CartDTO.</returns>
        public async Task<CartDTO> CheckoutAsync(string buyerId, int? userId)
        {
            Cart? cart = await _cartRepository.FindAsync(buyerId, userId);
            if (cart == null)
            {
                Log.Information("El carrito no existe para el comprador especificado {BuyerId}.", buyerId);
                throw new KeyNotFoundException("El carrito no existe para el comprador especificado.");
            }
            if (cart.CartItems.Count == 0)
            {
                Log.Information("El carrito está vacío para el comprador especificado {BuyerId}.", buyerId);
                throw new InvalidOperationException("El carrito está vacío.");
            }
            var itemsToRemove = new List<CartItem>();
            var itemsToUpdate = new List<(CartItem item, int newQuantity)>();
            bool hasChanges = false;

            foreach (var item in cart.CartItems.ToList())
            {
                int productStock = await _productRepository.GetRealStockAsync(item.ProductId);

                if (productStock == 0)
                {
                    Log.Information("El producto con ID {ProductId} está agotado.", item.ProductId);
                    itemsToRemove.Add(item);
                    hasChanges = true;
                }
                else if (item.Quantity > productStock)
                {
                    Log.Information("Ajustando cantidad del producto {ProductId} con cantidad actualizada {NewQuantity}", item.ProductId, productStock);
                    itemsToUpdate.Add((item, productStock));
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                foreach (var itemToRemove in itemsToRemove)
                {
                    cart.CartItems.Remove(itemToRemove);
                    await _cartRepository.RemoveItemAsync(itemToRemove);
                }

                foreach (var (item, newQuantity) in itemsToUpdate)
                {
                    item.Quantity = newQuantity;
                }

                RecalculateCartTotals(cart);

                await _cartRepository.UpdateAsync(cart);

                Log.Information("Carrito actualizado tras checkout. ItemsEliminados: {RemovedCount}, ItemsActualizados: {UpdatedCount}", itemsToRemove.Count, itemsToUpdate.Count);
            }
            else
            {
                Log.Information("No se requirieron ajustes en el carrito durante checkout. CartId: {CartId}", cart.Id);
            }

            return cart.Adapt<CartDTO>();
        }
    }
}