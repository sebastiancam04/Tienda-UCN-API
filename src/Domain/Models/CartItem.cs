namespace Tienda_UCN_api.src.Domain.Models
{
    public class CartItem
    {
        /// <summary>
        /// Identificador único del artículo en el carrito de compras.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Cantidad del producto en el carrito de compras.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Id del producto asociado al artículo del carrito de compras.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Producto asociado al artículo del carrito de compras.
        /// </summary>
        public Product Product { get; set; } = null!;

        /// <summary>
        /// Id del carrito de compras al que pertenece el artículo.
        /// </summary>
        public int CartId { get; set; }

        /// <summary>
        /// Carrito de compras al que pertenece el artículo.
        /// </summary>
        public Cart Cart { get; set; } = null!;
    }
}