namespace Tienda_UCN_api.Domain.Models
{
    public class Cart
    {
        /// <summary>
        /// Identificador único del carrito de compras.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Total del carrito de compras incluyendo descuentos.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Subtotal del carrito de compras sin descuentos.
        /// </summary>
        public int SubTotal { get; set; }

        /// <summary>
        /// Usuario que tiene el carrito (sin autenticación).
        /// </summary>
        public string BuyerId { get; set; } = null!;

        /// <summary>
        /// Identificador del usuario que posee el carrito de compras (autenticado).
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Lista de artículos en el carrito de compras.
        /// </summary>
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        /// <summary>
        /// Fecha de creación del carrito de compras.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de actualización del carrito de compras.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}