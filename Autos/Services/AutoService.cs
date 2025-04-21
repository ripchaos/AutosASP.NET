using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autos.Data;
using Autos.Models;
using Autos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Autos.Services
{
    public class AutoService : IAutoService
    {
        private readonly ApplicationDbContext _context;

        public AutoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Auto?> ObtenerAutoPorIdAsync(int id)
        {
            return await _context.Autos.FindAsync(id);
        }

        public async Task<IEnumerable<Auto>> ObtenerAutosDisponiblesAsync()
        {
            return await _context.Autos
                .Where(a => a.Disponibilidad)
                .ToListAsync();
        }

        public async Task<IEnumerable<Auto>> BuscarAutosAsync(string marca = "", string modelo = "", int? anioDesde = null, int? anioHasta = null)
        {
            var query = _context.Autos.AsQueryable();

            if (!string.IsNullOrEmpty(marca))
                query = query.Where(a => a.Marca.Contains(marca));

            if (!string.IsNullOrEmpty(modelo))
                query = query.Where(a => a.Modelo.Contains(modelo));

            if (anioDesde.HasValue)
                query = query.Where(a => a.Anio >= anioDesde.Value);

            if (anioHasta.HasValue)
                query = query.Where(a => a.Anio <= anioHasta.Value);

            return await query.ToListAsync();
        }

        public async Task<bool> ActualizarEstadoReservaAsync(int autoId, string estado)
        {
            var auto = await _context.Autos.FindAsync(autoId);
            if (auto == null)
                return false;

            auto.EstadoReserva = estado;
            _context.Autos.Update(auto);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> VerificarDisponibilidadAsync(int autoId)
        {
            var auto = await _context.Autos.FindAsync(autoId);
            return auto?.Disponibilidad ?? false;
        }

        public async Task<bool> GuardarAutoAsync(Auto auto)
        {
            if (auto.Id == 0)
                _context.Autos.Add(auto);
            else
                _context.Autos.Update(auto);

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> EliminarAutoAsync(int autoId)
        {
            var auto = await _context.Autos.FindAsync(autoId);
            if (auto == null)
                return false;

            _context.Autos.Remove(auto);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<IEnumerable<Auto>> ObtenerTodosLosAutosAsync()
        {
            return await _context.Autos
                .Include(a => a.Sucursal)
                .ToListAsync();
        }
    }
} 