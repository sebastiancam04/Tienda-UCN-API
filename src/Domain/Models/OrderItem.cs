namespace Tienda_UCN_api.Domain.Models
{
    public class OrderItem
    {
        /// <summary>
        /// Identificador único del artículo del pedido.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Cantidad del artículo en el pedido.
        /// </summary>
        public required int Quantity { get; set; }

        /// <summary>
        /// Precio del artículo en el momento del pedido.
        /// </summary>
        public required int PriceAtMoment { get; set; }

        /// <summary>
        /// Título del artículo en el momento del pedido.
        /// </summary>
        public required string TitleAtMoment { get; set; }

        /// <summary>
        /// Descripción del artículo en el momento del pedido.
        /// </summary>
        public required string DescriptionAtMoment { get; set; }

        /// <summary>
        /// Imagen del artículo en el momento del pedido (URL).
        /// </summary>
        public required string ImageAtMoment { get; set; }

        /// <summary>
        /// Descuento aplicado al artículo al momento del pedido.
        /// </summary>
        public int DiscountAtMoment { get; set; }

        /// <summary>
        /// Identificador del pedido al que pertenece el artículo.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Pedido al que pertenece el artículo.
        /// </summary>
        public Order Order { get; set; } = null!;
    }
}