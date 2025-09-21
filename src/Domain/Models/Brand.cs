namespace Tienda_UCN_api.Domain.Models
{
    public class Brand
    {
        /// <summary>
        /// Identificador único de la marca.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre de la marca.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Fecha de creación de la marca.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}