
namespace Tienda_UCN_api.src.Application.DTO.CartDTO
{
    public class CartDTO
    {
        public required string BuyerId { get; set; }
        public required int? UserId { get; set; }
        public required List<CartItemDTO> Items { get; set; } = new List<CartItemDTO>();
        public required string SubTotalPrice { get; set; }
        public required string TotalPrice { get; set; }
    }
}