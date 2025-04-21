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
    public class SolicitudDescuentoService : ISolicitudDescuentoService
    {
        private readonly ApplicationDbContext _context;

        public SolicitudDescuentoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SolicitudDescuento?> ObtenerSolicitudPorIdAsync(int id)
        {
            return await _context.SolicitudesDescuento
                .Include(s => s.Auto)
                .Include(s => s.Vendedor)
                .Include(s => s.Gerente)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<SolicitudDescuento>> ObtenerSolicitudesPorVendedorAsync(string vendedorId)
        {
            return await _context.SolicitudesDescuento
                .Include(s => s.Auto)
                .Include(s => s.Vendedor)
                .Include(s => s.Gerente)
                .Where(s => s.VendedorId == vendedorId)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();
        }

        public async Task<IEnumerable<SolicitudDescuento>> ObtenerSolicitudesPendientesAsync()
        {
            return await _context.SolicitudesDescuento
                .Include(s => s.Auto)
                .Include(s => s.Vendedor)
                .Where(s => !s.Aprobada.HasValue)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();
        }

        public async Task<bool> CrearSolicitudAsync(SolicitudDescuento solicitud)
        {
            try
            {
                // Establecer fecha de solicitud
                solicitud.FechaSolicitud = DateTime.Now;
                
                // Guardar la solicitud
                _context.SolicitudesDescuento.Add(solicitud);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AprobarSolicitudAsync(int solicitudId, string gerenteId)
        {
            try
            {
                // Obtener la solicitud
                var solicitud = await _context.SolicitudesDescuento.FindAsync(solicitudId);
                if (solicitud == null)
                    return false;

                // Actualizar la solicitud
                solicitud.Aprobada = true;
                solicitud.FechaRespuesta = DateTime.Now;
                solicitud.GerenteId = gerenteId;

                _context.SolicitudesDescuento.Update(solicitud);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RechazarSolicitudAsync(int solicitudId, string gerenteId, string motivo)
        {
            try
            {
                // Obtener la solicitud
                var solicitud = await _context.SolicitudesDescuento.FindAsync(solicitudId);
                if (solicitud == null)
                    return false;

                // Actualizar la solicitud
                solicitud.Aprobada = false;
                solicitud.FechaRespuesta = DateTime.Now;
                solicitud.GerenteId = gerenteId;
                solicitud.MotivoRechazo = motivo;

                _context.SolicitudesDescuento.Update(solicitud);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 