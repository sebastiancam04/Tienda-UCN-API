namespace Tienda_UCN_api.src.Application.DTO.OrderDTO
{
    public class ListedOrderDetailDTO
    {
        public required List<OrderDetailDTO> Orders { get; set; } = new List<OrderDetailDTO>();
        public int TotalCount { get; set; }

        public int TotalPages { get; set; }

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }
    }
}