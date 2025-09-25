using Tienda_UCN_api.src.Domain.Models;

namespace Tienda_UCN_api.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de usuarios.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Obtiene un usuario por su ID.
        /// </summary>
        /// <param name="id">Id del usuario</param>
        /// <returns>Usuario encontrado o nulo</returns>
        Task<User?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene un usuario por su correo electrónico.
        /// </summary>
        /// <param name="email">Correo electrónico del usuario</param>
        /// <returns>Usuario encontrado o nulo</returns>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Verifica si un usuario existe por su correo electrónico.
        /// </summary>
        /// <param name="email">Correo electrónico del usuario</param>
        /// <returns>True si el usuario existe, false en caso contrario</returns>
        Task<bool> ExistsByEmailAsync(string email);

        /// <summary>
        /// Verifica si un usuario existe por su RUT.
        /// </summary>
        /// <param name="rut">RUT del usuario</param>
        /// <returns>True si el usuario existe, false en caso contrario</returns>
        Task<bool> ExistsByRutAsync(string rut);

        /// <summary>
        /// Obtiene un usuario por su RUT.
        /// </summary>
        /// <param name="rut">RUT del usuario</param>
        /// <param name="trackChanges">Indica si se debe rastrear los cambios en la entidad</param>
        /// <returns>Usuario encontrado o nulo</returns>
        Task<User?> GetByRutAsync(string rut, bool trackChanges = false);

        /// <summary>
        /// Crea un nuevo usuario en la base de datos.
        /// </summary>
        /// <param name="user">Usuario a crear</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <returns>True si es exitoso, false en caso contrario</returns>
        Task<bool> CreateAsync(User user, string password);

        /// <summary>
        /// Verifica si la contraseña proporcionada es correcta para el usuario.
        /// </summary>
        /// <param name="user">Usuario al que se le verificará la contraseña</param>
        /// <param name="password">Contraseña a verificar</param>
        /// <returns>True si la contraseña es correcta, false en caso contrario</returns>
        Task<bool> CheckPasswordAsync(User user, string password);

        /// <summary>
        /// Obtiene el rol del usuario.
        /// </summary>
        /// <param name="user">Usuario del cual se desea obtener el rol</param>
        /// <returns>Nombre del rol del usuario</returns>
        Task<string> GetUserRoleAsync(User user);

        /// <summary>
        /// Elimina un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario a eliminar</param>
        /// <returns>True si la eliminación fue exitosa, false en caso contrario</returns>
        Task<bool> DeleteAsync(int userId);

        /// <summary>
        /// Confirma el correo electrónico del usuario.
        /// </summary>
        /// <param name="email">Correo electrónico del usuario</param>
        /// <returns>True si la confirmación fue exitosa, false en caso contrario</returns>
        Task<bool> ConfirmEmailAsync(string email);

        /// <summary>
        /// Elimina usuarios no confirmados.
        /// </summary>
        /// <returns>Número de usuarios eliminados</returns>
        Task<int> DeleteUnconfirmedAsync();
    }
}