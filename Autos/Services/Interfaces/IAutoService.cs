using System.Collections.Generic;
using System.Threading.Tasks;
using Autos.Models;

namespace Autos.Services.Interfaces
{
    public interface IAutoService
    {
        Task<Auto?> ObtenerAutoPorIdAsync(int id);
        Task<IEnumerable<Auto>> ObtenerAutosDisponiblesAsync();
        Task<IEnumerable<Auto>> BuscarAutosAsync(string marca = "", string modelo = "", int? anioDesde = null, int? anioHasta = null);
        Task<bool> ActualizarEstadoReservaAsync(int autoId, string estado);
        Task<bool> VerificarDisponibilidadAsync(int autoId);
        Task<bool> GuardarAutoAsync(Auto auto);
        Task<bool> EliminarAutoAsync(int autoId);
        Task<IEnumerable<Auto>> ObtenerTodosLosAutosAsync();
    }
} 