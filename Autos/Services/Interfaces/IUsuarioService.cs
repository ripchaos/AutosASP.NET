using System.Collections.Generic;
using System.Threading.Tasks;
using Autos.Models;
using Microsoft.AspNetCore.Identity;

namespace Autos.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<Usuario?> ObtenerUsuarioPorIdAsync(string id);
        Task<Usuario?> ObtenerUsuarioActualAsync();
        Task<IList<string>> ObtenerRolesDeUsuarioAsync(Usuario usuario);
        Task<bool> AsignarRolAsync(Usuario usuario, string rol);
        Task<bool> EliminarRolAsync(Usuario usuario, string rol);
        Task<bool> RestablecerPasswordAsync(Usuario usuario, string nuevaPassword);
        Task<IEnumerable<Usuario>> ObtenerUsuariosPorRolAsync(string rol);
        Task<bool> AsignarVendedorAClienteAsync(string clienteId, string vendedorId);
    }
} 