namespace Tienda_UCN_api.src.Application.DTO.OrderDTO
{
    public class OrderDetailDTO
    {
        public required string Code { get; set; }
        public required string Total { get; set; }
        public required string SubTotal { get; set; }
        public required DateTime PurchasedAt { get; set; }
        public required List<OrderItemDTO> Items { get; set; }
    }
}