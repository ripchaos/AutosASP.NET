using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autos.Models;

namespace Autos.Services.Interfaces
{
    public interface IReservaService
    {
        Task<Reserva?> ObtenerReservaPorIdAsync(int id);
        Task<IEnumerable<Reserva>> ObtenerReservasPorVendedorAsync(string vendedorId);
        Task<IEnumerable<Reserva>> ObtenerReservasPorClienteAsync(string clienteId);
        Task<IEnumerable<Reserva>> ObtenerReservasActivasAsync();
        Task<bool> CrearReservaAsync(Reserva reserva);
        Task<bool> CancelarReservaAsync(int reservaId, string motivo);
        Task<bool> ConcretarVentaAsync(int reservaId);
        Task<bool> ActualizarEstadoReservaAsync(int reservaId, string nuevoEstado);
    }
} 