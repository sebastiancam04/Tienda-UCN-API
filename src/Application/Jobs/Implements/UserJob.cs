using Hangfire;
using Serilog;
using Tienda_UCN_api.src.Application.Jobs.Interfaces;
using Tienda_UCN_api.src.Application.Services.Interfaces;

namespace Tienda_UCN_api.src.Application.Jobs.Implements
{
    /// <summary>
    /// Clase para manejar trabajos de usuario con Hangfire.
    /// </summary>
    public class UserJob : IUserJob
    {
        private readonly IUserService _userService;

        public UserJob(IUserService userService, IConfiguration _configuration)
        {
            _userService = userService;

        }

        [AutomaticRetry(Attempts = 10, DelaysInSeconds = new int[] { 60, 120, 300, 600, 900 })]
        public async Task DeleteUnconfirmedAsync()
        {

            Log.Information("Eliminando usuarios no confirmados...");
            await _userService.DeleteUnconfirmedAsync();
        }
    }
}