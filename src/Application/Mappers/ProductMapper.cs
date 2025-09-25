using Mapster;
using Tienda_UCN_api.src.Application.DTO.ProductDTO;
using Tienda_UCN_api.src.Application.DTO.ProductDTO.CustomerDTO;
using Tienda_UCN_api.src.Domain.Models;

namespace Tienda_UCN_api.src.Application.Mappers
{
    /// <summary>
    /// Clase para mapear objetos de tipo DTO a Product y viceversa.
    /// </summary>
    public class ProductMapper
    {
        private readonly IConfiguration _configuration;
        private readonly string? _defaultImageURL;
        private readonly int _fewUnitsAvailable;


        public ProductMapper(IConfiguration configuration)
        {
            _configuration = configuration;
            _defaultImageURL = _configuration.GetValue<string>("Products:DefaultImageUrl") ?? throw new InvalidOperationException("La URL de la imagen por defecto no puede ser nula.");
            _fewUnitsAvailable = _configuration.GetValue<int?>("Products:FewUnitsAvailable") ?? throw new InvalidOperationException("La configuración 'FewUnitsAvailable' no puede ser nula.");
        }

        public void ConfigureAllMappings()
        {
            ConfigureProductMappings();
        }

        public void ConfigureProductMappings()
        {
            TypeAdapterConfig<Product, ProductDetailDTO>.NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.ImagesURL, src => src.Images.Count() != 0 ? src.Images.Select(i => i.ImageUrl).ToList() : new List<string> { _defaultImageURL! })
                .Map(dest => dest.Price, src => src.Price.ToString("C"))
                .Map(dest => dest.Discount, src => src.Discount)
                .Map(dest => dest.Stock, src => src.Stock)
                .Map(dest => dest.StockIndicator, src => GetStockIndicator(src.Stock))
                .Map(dest => dest.CategoryName, src => src.Category.Name)
                .Map(dest => dest.BrandName, src => src.Brand.Name)
                .Map(dest => dest.StatusName, src => src.Status)
                .Map(dest => dest.IsAvailable, src => src.IsAvailable);

            TypeAdapterConfig<Product, ProductForCustomerDTO>.NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.MainImageURL, src => src.Images.FirstOrDefault() != null ? src.Images.First().ImageUrl : _defaultImageURL)
                .Map(dest => dest.Price, src => src.Price.ToString("C"))
                .Map(dest => dest.Discount, src => src.Discount);

            TypeAdapterConfig<Product, ProductForAdminDTO>.NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.MainImageURL, src => src.Images.FirstOrDefault() != null ? src.Images.First().ImageUrl : _defaultImageURL)
                .Map(dest => dest.Price, src => src.Price.ToString("C"))
                .Map(dest => dest.Stock, src => src.Stock)
                .Map(dest => dest.StockIndicator, src => GetStockIndicator(src.Stock))
                .Map(dest => dest.CategoryName, src => src.Category.Name)
                .Map(dest => dest.BrandName, src => src.Brand.Name)
                .Map(dest => dest.StatusName, src => src.Status)
                .Map(dest => dest.IsAvailable, src => src.IsAvailable);

            TypeAdapterConfig<CreateProductDTO, Product>.NewConfig()
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.Stock, src => src.Stock)
                .Map(dest => dest.Status, src => src.Status);
        }

        /// <summary>
        /// Obtiene el indicador de stock basado en la cantidad disponible.
        /// </summary>
        /// <param name="stock">Stock del producto</param>
        /// <returns>Retorna el mensaje adecuado</returns>
        private string GetStockIndicator(int stock)
        {
            if (stock == 0) { return "Producto sin stock"; }
            if (stock <= _fewUnitsAvailable) { return "Pocas unidades disponibles"; }
            return "Con Stock"!;
        }
    }
}