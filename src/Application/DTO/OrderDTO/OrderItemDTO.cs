namespace Tienda_UCN_api.src.Application.DTO.OrderDTO
{
    public class OrderItemDTO
    {
        public required string ProductTitle { get; set; }
        public required string ProductDescription { get; set; }
        public required string MainImageURL { get; set; }
        public required string PriceAtMoment { get; set; }
        public required int Quantity { get; set; }
    }
}