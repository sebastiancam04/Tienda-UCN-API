using Mapster;
using Serilog;
using Tienda_UCN_api.src.Application.DTO.ProductDTO;
using Tienda_UCN_api.src.Application.DTO.ProductDTO.CustomerDTO;
using Tienda_UCN_api.src.Application.Services.Interfaces;
using Tienda_UCN_api.src.Domain.Models;
using Tienda_UCN_api.src.Infrastructure.Repositories.Interfaces;

namespace Tienda_UCN_api.src.Application.Services.Implements
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IConfiguration _configuration;
        private readonly IFileService _fileService;

        private readonly int _defaultPageSize;

        public ProductService(IProductRepository productRepository, IConfiguration configuration, IFileService fileService)
        {
            _productRepository = productRepository;
            _configuration = configuration;
            _fileService = fileService;
            _defaultPageSize = int.Parse(_configuration["Products:DefaultPageSize"] ?? throw new InvalidOperationException("La configuración 'DefaultPageSize' no está definida."));
        }

        /// <summary>
        /// Crea un nuevo producto en el sistema.
        /// </summary>
        /// <param name="createProductDTO">Los datos del producto a crear.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con el ID del producto creado.</returns>
        public async Task<string> CreateAsync(CreateProductDTO createProductDTO)
        {
            Product product = createProductDTO.Adapt<Product>();
            Category category = await _productRepository.CreateOrGetCategoryAsync(createProductDTO.CategoryName) ?? throw new Exception("Error al crear o obtener la categoría del producto.");
            Brand brand = await _productRepository.CreateOrGetBrandAsync(createProductDTO.BrandName) ?? throw new Exception("Error al crear o obtener la marca del producto.");
            product.CategoryId = category.Id;
            product.BrandId = brand.Id;
            product.Images = new List<Image>();
            int productId = await _productRepository.CreateAsync(product);
            Log.Information("Producto creado: {@Product}", product);
            if (createProductDTO.Images == null || !createProductDTO.Images.Any())
            {
                Log.Information("No se proporcionaron imágenes. Se asignará la imagen por defecto.");
                throw new InvalidOperationException("Debe proporcionar al menos una imagen para el producto.");
            }
            foreach (var image in createProductDTO.Images)
            {
                Log.Information("Imagen asociada al producto: {@Image}", image);
                await _fileService.UploadAsync(image, productId);
            }
            return product.Id.ToString();
        }

        /// <summary>
        /// Retorna un producto específico por su ID.
        /// </summary>
        /// <param name="id">El ID del producto a buscar.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con el producto encontrado o null si no se encuentra.</returns>
        public async Task<ProductDetailDTO> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");
            Log.Information("Producto encontrado: {@Product}", product);
            return product.Adapt<ProductDetailDTO>();
        }

        /// <summary>
        /// Retorna un producto específico por su ID desde el punto de vista de un admin.
        /// </summary>
        /// <param name="id">El ID del producto a buscar.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con el producto encontrado o null si no se encuentra.</returns>
        public async Task<ProductDetailDTO> GetByIdForAdminAsync(int id)
        {
            var product = await _productRepository.GetByIdForAdminAsync(id) ?? throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");
            Log.Information("Producto encontrado: {@Product}", product);
            return product.Adapt<ProductDetailDTO>();
        }

        /// <summary>
        /// Retorna todos los productos para el administrador según los parámetros de búsqueda.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda para filtrar los productos.</param>
        /// <returns>Una lista de productos filtrados para el administrador.</returns>
        public async Task<ListedProductsForAdminDTO> GetFilteredForAdminAsync(SearchParamsDTO searchParams)
        {
            Log.Information("Obteniendo productos para administrador con parámetros de búsqueda: {@SearchParams}", searchParams);
            var (products, totalCount) = await _productRepository.GetFilteredForAdminAsync(searchParams);
            var totalPages = (int)Math.Ceiling((double)totalCount / (searchParams.PageSize ?? _defaultPageSize));
            int currentPage = searchParams.PageNumber;
            int pageSize = searchParams.PageSize ?? _defaultPageSize;
            if (currentPage < 1 || currentPage > totalPages)
            {
                throw new ArgumentOutOfRangeException("El número de página está fuera de rango.");
            }
            Log.Information("Total de productos encontrados: {TotalCount}, Total de páginas: {TotalPages}, Página actual: {CurrentPage}, Tamaño de página: {PageSize}", totalCount, totalPages, currentPage, pageSize);

            // Convertimos los productos filtrados a DTOs para la respuesta
            return new ListedProductsForAdminDTO
            {
                Products = products.Adapt<List<ProductForAdminDTO>>(),
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = currentPage,
                PageSize = products.Count()
            };

        }

        /// <summary>
        /// Retorna todos los productos para el cliente según los parámetros de búsqueda.
        /// </summary>
        /// <param name="searchParams">Parámetros de búsqueda para filtrar los productos.</param>
        /// <returns>Una lista de productos filtrados para el cliente.</returns>
        public async Task<ListedProductsForCustomerDTO> GetFilteredForCustomerAsync(SearchParamsDTO searchParams)
        {
            var (products, totalCount) = await _productRepository.GetFilteredForCustomerAsync(searchParams);
            var totalPages = (int)Math.Ceiling((double)totalCount / (searchParams.PageSize ?? _defaultPageSize));
            int currentPage = searchParams.PageNumber;
            int pageSize = searchParams.PageSize ?? _defaultPageSize;
            if (currentPage < 1 || currentPage > totalPages)
            {
                throw new ArgumentOutOfRangeException("El número de página está fuera de rango.");
            }
            Log.Information("Total de productos encontrados: {TotalCount}, Total de páginas: {TotalPages}, Página actual: {CurrentPage}, Tamaño de página: {PageSize}", totalCount, totalPages, currentPage, pageSize);

            // Convertimos los productos filtrados a DTOs para la respuesta
            return new ListedProductsForCustomerDTO
            {
                Products = products.Adapt<List<ProductForCustomerDTO>>(),
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = currentPage,
                PageSize = products.Count()
            };
        }

        /// <summary>
        /// Cambia el estado activo de un producto por su ID.
        /// </summary>
        /// <param name="id">El ID del producto cuyo estado se cambiará.</param>
        public async Task ToggleActiveAsync(int id)
        {
            await _productRepository.ToggleActiveAsync(id);
        }

    }
}