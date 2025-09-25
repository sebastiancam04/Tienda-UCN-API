namespace Tienda_UCN_api.src.Application.DTO.ProductDTO.CustomerDTO
{
    public class ListedProductsForCustomerDTO
    {
        public List<ProductForCustomerDTO> Products { get; set; } = new List<ProductForCustomerDTO>();

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }
    }
}