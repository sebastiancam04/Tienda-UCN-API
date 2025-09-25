using Microsoft.EntityFrameworkCore;
using Tienda_UCN_api.src.Application.DTO.ProductDTO;
using Tienda_UCN_api.src.Domain.Models;
using Tienda_UCN_api.src.Infrastructure.Data;
using Tienda_UCN_api.src.Infrastructure.Repositories.Interfaces;

namespace Tienda_UCN_api.src.Infrastructure.Repositories.Implements
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageSize;

        public OrderRepository(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _defaultPageSize = int.Parse(_configuration["Products:DefaultPageSize"] ?? throw new InvalidOperationException("La configuración 'DefaultPageSize' no está definida."));
        }

        /// <summary>
        /// Verifica si un código de orden existe.
        /// </summary>
        /// <param name="orderCode">Código de la orden</param>
        /// <returns>True si el código existe, de lo contrario false.</returns>
        public async Task<bool> CodeExistsAsync(string orderCode)
        {
            return await _context.Orders.AnyAsync(o => o.Code == orderCode);
        }

        /// <summary>
        /// Crea una nueva orden.
        /// </summary>
        /// <param name="order">La orden a crear.</param>
        /// <returns>Booleano que indica si la creación fue exitosa.</returns>
        public async Task<bool> CreateAsync(Order order)
        {
            Order? existingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.Code == order.Code);
            if (existingOrder != null) { return false; }
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Obtiene una orden por su código.
        /// </summary>
        /// <param name="orderCode">El código de la orden.</param>
        /// <returns>La orden correspondiente o null si no se encuentra.</returns>
        public async Task<Order?> GetByCodeAsync(string orderCode)
        {
            return await _context.Orders.AsNoTracking().Include(or => or.OrderItems).FirstOrDefaultAsync(o => o.Code == orderCode);
        }

        /// <summary>
        /// Obtiene las órdenes de un usuario por su ID.
        /// </summary>
        /// <param name="userId">El ID del usuario.</param>
        /// <param name="searchParams">Los parámetros de búsqueda.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con una lista de órdenes y el conteo total de órdenes.</returns>
        public async Task<(IEnumerable<Order> orders, int totalCount)> GetByUserIdAsync(SearchParamsDTO searchParams, int userId)
        {
            var query = _context.Orders.AsNoTracking().Include(or => or.OrderItems).Where(or => or.UserId == userId);
            var totalCount = await query.CountAsync();
            string searchTerm = searchParams.SearchTerm?.Trim().ToLower() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
            {
                query = query.Where(or => or.Code.Contains(searchTerm)
                || or.OrderItems.Any(oi => oi.TitleAtMoment.ToLower().Contains(searchTerm)
                || or.OrderItems.Any(oi => oi.DescriptionAtMoment.ToLower().Contains(searchTerm))));
            }
            query = query.Skip((searchParams.PageNumber - 1) * searchParams.PageSize ?? _defaultPageSize)
                         .Take(searchParams.PageSize ?? _defaultPageSize);
            return (await query.ToListAsync(), totalCount);
        }
    }
}