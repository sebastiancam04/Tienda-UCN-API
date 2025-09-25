using Mapster;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Serilog;
using Tienda_UCN_api.src.Application.DTO.CartDTO;
using Tienda_UCN_api.src.Application.DTO.OrderDTO;
using Tienda_UCN_api.src.Application.DTO.ProductDTO;
using Tienda_UCN_api.src.Application.Services.Interfaces;
using Tienda_UCN_api.src.Domain.Models;
using Tienda_UCN_api.src.Infrastructure.Repositories.Interfaces;

namespace Tienda_UCN_api.src.Application.Services.Implements
{
    public class OrderService : IOrderService
    {

        private readonly IConfiguration _configuration;
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        private readonly int _defaultPageSize;
        public OrderService(IOrderRepository orderRepository, ICartRepository cartRepository, IProductRepository productRepository, IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _configuration = configuration;
            _defaultPageSize = int.Parse(_configuration["Products:DefaultPageSize"] ?? throw new InvalidOperationException("La configuración 'DefaultPageSize' no está definida."));
        }

        /// <summary>
        /// Crea una nueva orden y vacía el carrito de compras.
        /// </summary>
        /// <param name="userId">Id del usuario autenticado</param>
        /// <returns>Crea una nueva orden y vacía el carrito de compras.</returns>
        public async Task<string> CreateAsync(int userId)
        {
            Cart? cart = await _cartRepository.GetByUserIdAsync(userId) ?? throw new KeyNotFoundException("Carrito no encontrado");
            if (cart.CartItems.Count == 0)
            {
                Log.Information("El carrito del usuario con id {userId} está vacío.", userId);
                throw new InvalidOperationException("El carrito del usuario está vacío.");
            }
            string code = await GenerateOrderCodeAsync();
            Order order = cart.Adapt<Order>();
            order.Code = code;
            order.UserId = userId;
            await _orderRepository.CreateAsync(order);

            foreach (var item in cart.CartItems)
            {
                item.Product.Stock -= item.Quantity;
                await _productRepository.UpdateStockAsync(item.ProductId, item.Product.Stock);
            }
            cart.CartItems.Clear();
            cart.Total = 0;
            cart.SubTotal = 0;
            await _cartRepository.UpdateAsync(cart);
            return code;
        }

        /// <summary>
        /// Obtiene una lista de órdenes para un usuario específico.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda</param>
        /// <param name="userId">Id del usuario al que pertenecen las órdenes</param>
        /// <returns>Ordenes del usuario</returns>
        public async Task<ListedOrderDetailDTO> GetByUserIdAsync(SearchParamsDTO searchParams, int userId)
        {
            var (orders, totalCount) = await _orderRepository.GetByUserIdAsync(searchParams, userId);
            var totalPages = (int)Math.Ceiling((double)totalCount / (searchParams.PageSize ?? _defaultPageSize));
            int currentPage = searchParams.PageNumber;
            int pageSize = searchParams.PageSize ?? _defaultPageSize;
            if (currentPage < 1 || currentPage > totalPages)
            {
                throw new ArgumentOutOfRangeException("El número de página está fuera de rango.");
            }
            var listedOrders = new ListedOrderDetailDTO
            {
                Orders = orders.Adapt<List<OrderDetailDTO>>(),
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = currentPage,
                PageSize = pageSize
            };
            return listedOrders;
        }

        /// <summary>
        /// Obtiene los detalles de una orden.
        /// </summary>
        /// <param name="orderCode">Código de la orden</param>
        /// <returns>El detalle de la orden</returns>
        public async Task<OrderDetailDTO> GetDetailAsync(string orderCode)
        {
            Order? order = await _orderRepository.GetByCodeAsync(orderCode) ?? throw new KeyNotFoundException("Orden no encontrada");
            return order.Adapt<OrderDetailDTO>();
        }

        /// <summary>
        /// Genera un código único para la orden.
        /// </summary>
        /// <returns>Código de la orden</returns>
        private async Task<string> GenerateOrderCodeAsync()
        {
            string code;
            do
            {
                var timestamp = DateTime.UtcNow.ToString("yyMMddHHmmss");
                var random = Random.Shared.Next(100, 999);
                code = $"ORD-{timestamp}-{random}";
            }
            while (await _orderRepository.CodeExistsAsync(code));
            return code;
        }
    }
}