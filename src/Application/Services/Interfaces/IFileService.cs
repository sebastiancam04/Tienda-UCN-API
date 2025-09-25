namespace Tienda_UCN_api.src.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de archivos.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Sube un archivo a Cloudinary.
        /// </summary>
        /// <param name="file">El archivo a subir.</param>
        /// <param name="productId">El ID del producto al que pertenece la imagen.</param>
        /// <returns>True si la carga fue exitosa, de lo contrario False.</returns>
        Task<bool> UploadAsync(IFormFile file, int productId);

        /// <summary>
        /// Elimina un archivo de Cloudinary.
        /// </summary>
        /// <param name="publicId">El ID público del archivo a eliminar.</param>
        /// <returns>True si la eliminación fue exitosa, de lo contrario false.</returns>
        Task<bool> DeleteAsync(string publicId);
    }
}