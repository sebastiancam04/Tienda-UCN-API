using Mapster;
using Tienda_UCN_api.src.Application.DTO.CartDTO;
using Tienda_UCN_api.src.Application.DTO.OrderDTO;
using Tienda_UCN_api.src.Domain.Models;

namespace Tienda_UCN_api.src.Application.Mappers
{
    /// <summary>
    /// Clase para mapeo de órdenes a DTO y viceversa.
    /// </summary>
    public class OrderMapper
    {
        private readonly IConfiguration _configuration;
        private readonly string _defaultImageURL;
        private readonly TimeZoneInfo _timeZone;

        public OrderMapper(IConfiguration configuration)
        {
            _configuration = configuration;
            _defaultImageURL = _configuration["Products:DefaultImageUrl"] ?? throw new InvalidOperationException("La configuración de DefaultImageUrl es necesaria.");
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id);
        }

        public void ConfigureAllMappings()
        {
            ConfigureOrderItemsMappings();
            ConfigureOrderMappings();
        }

        public void ConfigureOrderMappings()
        {
            TypeAdapterConfig<Order, OrderDetailDTO>.NewConfig()
                .Map(dest => dest.Items, src => src.OrderItems)
                .Map(dest => dest.PurchasedAt, src => TimeZoneInfo.ConvertTimeFromUtc(src.CreatedAt, _timeZone))
                .Map(dest => dest.Code, src => src.Code)
                .Map(dest => dest.Total, src => src.Total.ToString("C"))
                .Map(dest => dest.SubTotal, src => src.SubTotal.ToString("C"));

            TypeAdapterConfig<Cart, Order>.NewConfig()
                .Map(dest => dest.Total, src => src.Total)
                .Map(dest => dest.SubTotal, src => src.SubTotal)
                .Map(dest => dest.OrderItems, src => src.CartItems)
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.Code)
                .Ignore(dest => dest.CreatedAt);
        }

        public void ConfigureOrderItemsMappings()
        {
            TypeAdapterConfig<OrderItem, OrderItemDTO>.NewConfig()
                .Map(dest => dest.ProductTitle, src => src.TitleAtMoment)
                .Map(dest => dest.Quantity, src => src.Quantity)
                .Map(dest => dest.ProductDescription, src => src.DescriptionAtMoment)
                .Map(dest => dest.MainImageURL, src => src.ImageAtMoment)
                .Map(dest => dest.PriceAtMoment, src => src.PriceAtMoment.ToString("C"));

            TypeAdapterConfig<CartItem, OrderItem>.NewConfig()
                .Map(dest => dest.TitleAtMoment, src => src.Product.Title)
                .Map(dest => dest.Quantity, src => src.Quantity)
                .Map(dest => dest.DescriptionAtMoment, src => src.Product.Description)
                .Map(dest => dest.ImageAtMoment, src => src.Product.Images != null && src.Product.Images.Any() ? src.Product.Images.First().ImageUrl : _defaultImageURL)
                .Map(dest => dest.PriceAtMoment, src => src.Product.Price)
                .Map(dest => dest.DiscountAtMoment, src => src.Product.Discount)
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.OrderId)
                .Ignore(dest => dest.Order);
        }

    }
}