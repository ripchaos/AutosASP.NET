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
    public class VentaService : IVentaService
    {
        private readonly ApplicationDbContext _context;

        public VentaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Venta?> ObtenerVentaPorIdAsync(int id)
        {
            return await _context.Ventas
                .Include(v => v.Auto)
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<IEnumerable<Venta>> ObtenerVentasPorVendedorAsync(string vendedorId)
        {
            return await _context.Ventas
                .Include(v => v.Auto)
                .Include(v => v.Cliente)
                .Where(v => v.VendedorId == vendedorId)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> ObtenerVentasPorClienteAsync(string clienteId)
        {
            return await _context.Ventas
                .Include(v => v.Auto)
                .Include(v => v.Cliente)
                .Where(v => v.ClienteId == clienteId)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> ObtenerVentasReciententesAsync(int cantidad = 10)
        {
            return await _context.Ventas
                .Include(v => v.Auto)
                .Include(v => v.Cliente)
                .OrderByDescending(v => v.Fecha)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<bool> RegistrarVentaAsync(Venta venta)
        {
            try
            {
                // Establecer fecha actual si no está definida
                if (venta.Fecha == default)
                    venta.Fecha = DateTime.Now;

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<decimal> CalcularMontoTotalVentasAsync(string vendedorId)
        {
            var ventas = await _context.Ventas
                .Where(v => v.VendedorId == vendedorId)
                .ToListAsync();

            return ventas.Sum(v => v.MontoFinal);
        }

        public async Task<decimal> CalcularMontoTotalVentasMesAsync(string vendedorId, int mes, int anio)
        {
            var ventas = await _context.Ventas
                .Where(v => v.VendedorId == vendedorId && 
                            v.Fecha.Month == mes && 
                            v.Fecha.Year == anio)
                .ToListAsync();

            return ventas.Sum(v => v.MontoFinal);
        }

        public async Task<int> ContarVentasMesAsync(string vendedorId, int mes, int anio)
        {
            return await _context.Ventas
                .Where(v => v.VendedorId == vendedorId && 
                            v.Fecha.Month == mes && 
                            v.Fecha.Year == anio)
                .CountAsync();
        }

        public async Task<decimal> CalcularIngresosTotalesAsync()
        {
            var ventas = await _context.Ventas.ToListAsync();
            return ventas.Sum(v => v.MontoFinal);
        }

        public async Task<decimal> CalcularIngresosMensualesAsync(int mes, int anio)
        {
            var ventas = await _context.Ventas
                .Where(v => v.Fecha.Month == mes && v.Fecha.Year == anio)
                .ToListAsync();

            return ventas.Sum(v => v.MontoFinal);
        }

        public async Task<int> ContarVentasMesActualAsync()
        {
            var fechaInicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            return await _context.Ventas
                .Where(v => v.Fecha >= fechaInicioMes)
                .CountAsync();
        }
    }
} 