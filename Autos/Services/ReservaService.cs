using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autos.Data;
using Autos.Models;
using Autos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Autos.Services
{
    public class ReservaService : IReservaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAutoService _autoService;

        public ReservaService(ApplicationDbContext context, IAutoService autoService)
        {
            _context = context;
            _autoService = autoService;
        }

        public async Task<Reserva?> ObtenerReservaPorIdAsync(int id)
        {
            return await _context.Reservas
                .Include(r => r.Auto)
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Reserva>> ObtenerReservasPorVendedorAsync(string vendedorId)
        {
            return await _context.Reservas
                .Include(r => r.Auto)
                .Include(r => r.Usuario)
                .Where(r => r.VendedorId == vendedorId)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> ObtenerReservasPorClienteAsync(string clienteId)
        {
            return await _context.Reservas
                .Include(r => r.Auto)
                .Include(r => r.Usuario)
                .Where(r => r.UsuarioId == clienteId)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> ObtenerReservasActivasAsync()
        {
            return await _context.Reservas
                .Include(r => r.Auto)
                .Include(r => r.Usuario)
                .Where(r => r.Estado == "Activa")
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();
        }

        public async Task<bool> CrearReservaAsync(Reserva reserva)
        {
            try
            {
                // Verificar disponibilidad del auto
                if (!await _autoService.VerificarDisponibilidadAsync(reserva.AutoId))
                    return false;

                // Establecer valores predeterminados
                reserva.FechaReserva = DateTime.Now;
                reserva.Estado = "Activa";

                // Guardar la reserva
                _context.Reservas.Add(reserva);
                await _context.SaveChangesAsync();

                // Actualizar estado del auto
                await _autoService.ActualizarEstadoReservaAsync(reserva.AutoId, "Reservado");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CancelarReservaAsync(int reservaId, string motivo)
        {
            var reserva = await _context.Reservas.FindAsync(reservaId);
            if (reserva == null)
                return false;

            reserva.Estado = "Cancelada";
            reserva.MotivoCancelacion = motivo;
            reserva.FechaCancelacion = DateTime.Now;

            _context.Reservas.Update(reserva);
            await _context.SaveChangesAsync();

            // Liberar el auto
            await _autoService.ActualizarEstadoReservaAsync(reserva.AutoId, "Disponible");

            return true;
        }

        public async Task<bool> ConcretarVentaAsync(int reservaId)
        {
            var reserva = await _context.Reservas.FindAsync(reservaId);
            if (reserva == null)
                return false;

            reserva.Estado = "Concretada";
            reserva.FechaConcretacion = DateTime.Now;

            _context.Reservas.Update(reserva);
            await _context.SaveChangesAsync();

            // Actualizar estado del auto
            await _autoService.ActualizarEstadoReservaAsync(reserva.AutoId, "Vendido");

            return true;
        }

        public async Task<bool> ActualizarEstadoReservaAsync(int reservaId, string nuevoEstado)
        {
            var reserva = await _context.Reservas.FindAsync(reservaId);
            if (reserva == null)
                return false;

            reserva.Estado = nuevoEstado;
            
            switch (nuevoEstado)
            {
                case "Cancelada":
                    reserva.FechaCancelacion = DateTime.Now;
                    break;
                case "Concretada":
                    reserva.FechaConcretacion = DateTime.Now;
                    break;
            }

            _context.Reservas.Update(reserva);
            await _context.SaveChangesAsync();

            return true;
        }
    }
} 