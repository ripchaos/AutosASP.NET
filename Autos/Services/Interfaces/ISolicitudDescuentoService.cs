using System.Collections.Generic;
using System.Threading.Tasks;
using Autos.Models;

namespace Autos.Services.Interfaces
{
    public interface ISolicitudDescuentoService
    {
        Task<SolicitudDescuento?> ObtenerSolicitudPorIdAsync(int id);
        Task<IEnumerable<SolicitudDescuento>> ObtenerSolicitudesPorVendedorAsync(string vendedorId);
        Task<IEnumerable<SolicitudDescuento>> ObtenerSolicitudesPendientesAsync();
        Task<bool> CrearSolicitudAsync(SolicitudDescuento solicitud);
        Task<bool> AprobarSolicitudAsync(int solicitudId, string gerenteId);
        Task<bool> RechazarSolicitudAsync(int solicitudId, string gerenteId, string motivo);
    }
} 