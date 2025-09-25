using System.ComponentModel.DataAnnotations;

namespace Tienda_UCN_api.src.Application.DTO.ProductDTO
{
    public class CreateProductDTO
    {
        [Required(ErrorMessage = "El título del producto es obligatorio.")]
        [StringLength(50, ErrorMessage = "El título no puede exceder los 50 caracteres.")]
        [MinLength(3, ErrorMessage = "El título debe tener al menos 3 caracteres.")]
        public required string Title { get; set; }
        [Required(ErrorMessage = "La descripción del producto es obligatoria.")]
        [StringLength(100, ErrorMessage = "La descripción no puede exceder los 100 caracteres.")]
        [MinLength(10, ErrorMessage = "La descripción debe tener al menos 10 caracteres.")]
        public required string Description { get; set; }
        [Required(ErrorMessage = "El precio del producto es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El precio debe ser un valor entero positivo.")]
        public required int Price { get; set; }
        [Required(ErrorMessage = "El descuento del producto es obligatorio.")]
        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100.")]
        public required int Discount { get; set; }
        [Required(ErrorMessage = "El stock del producto es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser un valor positivo.")]
        public required int Stock { get; set; }
        [Required(ErrorMessage = "El estado del producto es obligatorio.")]
        [RegularExpression("^(New|Used)$", ErrorMessage = "El estado debe ser 'Nuevo' o 'Usado'.")]
        public required string Status { get; set; }
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre de la categoría no puede exceder los 50 caracteres.")]
        [MinLength(3, ErrorMessage = "El nombre de la categoría debe tener al menos 3 caracteres.")]
        public required string CategoryName { get; set; }
        [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre de la marca no puede exceder los 50 caracteres.")]
        [MinLength(3, ErrorMessage = "El nombre de la marca debe tener al menos 3 caracteres.")]
        public required string BrandName { get; set; }
        [Required(ErrorMessage = "Las imágenes del producto son obligatorias.")]
        public required List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}