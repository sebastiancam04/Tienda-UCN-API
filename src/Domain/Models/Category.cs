namespace Tienda_UCN_api.Domain.Models
{
    public class Category
    {
        /// <summary>
        /// Identificador único de la categoría.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre de la categoría.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Fecha de creación de la categoría.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}