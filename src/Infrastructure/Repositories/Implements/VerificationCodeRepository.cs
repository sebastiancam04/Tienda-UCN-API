using Microsoft.EntityFrameworkCore;
using Tienda_UCN_api.src.Domain.Models;
using Tienda_UCN_api.src.Infrastructure.Data;
using Tienda_UCN_api.src.Infrastructure.Repositories.Interfaces;

namespace Tienda_UCN_api.src.Infrastructure.Repositories.Implements
{
    /// <summary>
    /// Implementación del repositorio de códigos de verificación.
    /// </summary>
    public class VerificationCodeRepository : IVerificationCodeRepository
    {
        private readonly DataContext _context;

        public VerificationCodeRepository(DataContext dataContext)
        {
            _context = dataContext;
        }

        /// <summary>
        /// Crea un nuevo código de verificación.
        /// </summary>
        /// <param name="verificationCode">El código de verificación a crear.</param>
        /// <returns>El código de verificación creado.</returns>
        public async Task<VerificationCode> CreateAsync(VerificationCode verificationCode)
        {
            await _context.VerificationCodes.AddAsync(verificationCode);
            await _context.SaveChangesAsync();
            return verificationCode;
        }

        /// <summary>
        /// Elimina un código de verificación por ID de usuario y tipo de código.
        /// </summary>
        /// <param name="id">El ID del usuario.</param>
        /// <param name="codeType">El tipo de código de verificación.</param>
        /// <returns>True si se eliminó correctamente, false si no existía.</returns
        public async Task<bool> DeleteByUserIdAsync(int id, CodeType codeType)
        {
            await _context.VerificationCodes.Where(vc => vc.UserId == id && vc.CodeType == codeType).ExecuteDeleteAsync();
            var exists = await _context.VerificationCodes.AnyAsync(vc => vc.UserId == id && vc.CodeType == codeType);
            return !exists;
        }

        /// <summary>
        /// Elimina todos los códigos de verificación asociados a un usuario.
        /// </summary>
        /// <param name="userId">El ID del usuario.</param>
        /// <returns>El número de códigos de verificación eliminados.</returns>
        public async Task<int> DeleteByUserIdAsync(int userId)
        {
            var codes = await _context.VerificationCodes.Where(vc => vc.UserId == userId).ToListAsync();
            _context.VerificationCodes.RemoveRange(codes);
            await _context.SaveChangesAsync();
            return codes.Count;
        }

        /// <summary>
        /// Obtiene el último código de verificación por ID de usuario y tipo de código.
        /// </summary>
        /// <param name="userId">El ID del usuario.</param>
        /// <param name="codeType">El tipo de código de verificación.</param>
        /// <returns>El último código de verificación encontrado, o null si no existe.</returns>
        public async Task<VerificationCode?> GetLatestByUserIdAsync(int userId, CodeType codeType)
        {
            return await _context.VerificationCodes.Where(vc => vc.UserId == userId && vc.CodeType == codeType).OrderByDescending(vc => vc.CreatedAt).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Aumenta el contador de intentos de un código de verificación.
        /// </summary>
        /// <param name="userId">El ID del usuario.</param>
        /// <param name="codeType">El tipo de código de verificación.</param>
        /// <returns>El número de intentos incrementados.</returns>
        public async Task<int> IncreaseAttemptsAsync(int userId, CodeType codeType)
        {
            await _context.VerificationCodes.Where(vc => vc.UserId == userId && vc.CodeType == codeType).ExecuteUpdateAsync(setters => setters.SetProperty(vc => vc.AttemptCount, vc => vc.AttemptCount + 1));
            var newCount = await _context.VerificationCodes.AsNoTracking().Where(vc => vc.UserId == userId && vc.CodeType == codeType).Select(vc => vc.AttemptCount).FirstOrDefaultAsync();
            return newCount;
        }

        /// <summary>
        /// Actualiza un código de verificación existente.
        /// </summary>
        /// <param name="verificationCode">El código de verificación a actualizar.</param>
        /// <returns>El código de verificación actualizado.</returns>
        public async Task<VerificationCode?> UpdateAsync(VerificationCode verificationCode)
        {
            await _context.VerificationCodes
                .Where(vc => vc.Id == verificationCode.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(vc => vc.Code, verificationCode.Code)
                    .SetProperty(vc => vc.AttemptCount, verificationCode.AttemptCount)
                    .SetProperty(vc => vc.ExpiryDate, verificationCode.ExpiryDate));
            return await _context.VerificationCodes.AsNoTracking().FirstOrDefaultAsync(vc => vc.Id == verificationCode.Id);
        }
    }
}