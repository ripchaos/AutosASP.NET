using System.Collections.Generic;
using System.Threading.Tasks;
using Autos.Models;

namespace Autos.Services.Interfaces
{
    public interface ISucursalService
    {
        Task<Sucursal?> ObtenerSucursalPorIdAsync(int id);
        Task<IEnumerable<Sucursal>> ObtenerTodasLasSucursalesAsync();
        Task<bool> CrearSucursalAsync(Sucursal sucursal);
        Task<bool> ActualizarSucursalAsync(Sucursal sucursal);
        Task<bool> EliminarSucursalAsync(int id);
        Task<bool> AsignarVendedorASucursalAsync(string vendedorId, int sucursalId, bool esPrincipal = false);
        Task<bool> DesasignarVendedorDeSucursalAsync(int asignacionId);
        Task<IEnumerable<Usuario>> ObtenerVendedoresDeSucursalAsync(int sucursalId);
        Task<Sucursal?> ObtenerSucursalDeGerenteAsync(string gerenteId);
        Task<IEnumerable<UsuarioSucursal>> ObtenerAsignacionesDeSucursalAsync(int sucursalId);
        Task<UsuarioSucursal?> ObtenerAsignacionUsuarioSucursalAsync(string usuarioId, bool soloActivas = true);
    }
} 