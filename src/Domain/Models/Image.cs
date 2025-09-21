namespace Tienda_UCN_api.Domain.Models
{
    public class Image
    {
        /// <summary>
        /// Identificador único de la imagen.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// URL de la imagen.
        /// </summary>
        public required string ImageUrl { get; set; }

        /// <summary>
        /// Identificador público de la imagen.
        /// </summary>
        public required string PublicId { get; set; }

        /// <summary>
        /// Identificador del producto asociado a la imagen.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Fecha de creación de la imagen.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}