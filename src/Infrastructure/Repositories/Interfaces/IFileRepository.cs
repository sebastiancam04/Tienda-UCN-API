using Tienda_UCN_api.src.Domain.Models;

namespace Tienda_UCN_api.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de archivos.
    /// Define métodos para manejar operaciones relacionadas con archivos de imagen.
    /// </summary>
    public interface IFileRepository
    {
        /// <summary>
        /// Crea un archivo de imagen en la base de datos.
        /// </summary>
        /// <param name="file">El archivo de imagen a crear.</param>
        /// <returns>True si el archivo se creó correctamente, de lo contrario false y null en caso de que la imagen ya existe.</returns>
        Task<bool?> CreateAsync(Image file);

        /// <summary>
        /// Elimina un archivo de imagen de la base de datos.
        /// </summary>
        /// <param name="publicId">El identificador público del archivo a eliminar.</param>
        /// <returns>True si el archivo se eliminó correctamente, de lo contrario false y null si la imagen no existe.</returns>
        Task<bool?> DeleteAsync(string publicId);
    }
}