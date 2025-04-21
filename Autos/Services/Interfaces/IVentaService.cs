using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autos.Models;

namespace Autos.Services.Interfaces
{
    public interface IVentaService
    {
        Task<Venta?> ObtenerVentaPorIdAsync(int id);
        Task<IEnumerable<Venta>> ObtenerVentasPorVendedorAsync(string vendedorId);
        Task<IEnumerable<Venta>> ObtenerVentasPorClienteAsync(string clienteId);
        Task<IEnumerable<Venta>> ObtenerVentasReciententesAsync(int cantidad = 10);
        Task<bool> RegistrarVentaAsync(Venta venta);
        Task<decimal> CalcularMontoTotalVentasAsync(string vendedorId);
        Task<decimal> CalcularMontoTotalVentasMesAsync(string vendedorId, int mes, int anio);
        Task<int> ContarVentasMesAsync(string vendedorId, int mes, int anio);
        Task<decimal> CalcularIngresosTotalesAsync();
        Task<decimal> CalcularIngresosMensualesAsync(int mes, int anio);
        Task<int> ContarVentasMesActualAsync();
    }
} 