using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Autos.Models;
using System;
using System.Threading.Tasks;

namespace Autos.Data
{
    public static class ApplicationDbHelper
    {
        /// <summary>
        /// Obtiene un auto por su ID desde cualquier vista
        /// </summary>
        /// <param name="autoId">ID del auto a buscar</param>
        /// <param name="serviceProvider">Proveedor de servicios (se obtiene de ViewContext.HttpContext.RequestServices)</param>
        /// <returns>El auto o null si no existe</returns>
        public static async Task<Auto?> GetAutoAsync(int autoId, IServiceProvider serviceProvider)
        {
            // Validación de parámetros
            if (autoId <= 0)
                return null;

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            try
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                return await dbContext.Autos
                    .Include(a => a.Sucursal)
                    .AsNoTracking() // Optimización: solo lectura
                    .FirstOrDefaultAsync(a => a.Id == autoId);
            }
            catch (Exception)
            {
                // En un entorno de producción, deberías registrar la excepción
                return null;
            }
        }
    }
} 