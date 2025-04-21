using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autos.Data;
using Autos.Models;
using Autos.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Autos.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsuarioService(
            UserManager<Usuario> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Usuario?> ObtenerUsuarioPorIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<Usuario?> ObtenerUsuarioActualAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
                return null;

            return await _userManager.GetUserAsync(user);
        }

        public async Task<IList<string>> ObtenerRolesDeUsuarioAsync(Usuario usuario)
        {
            return await _userManager.GetRolesAsync(usuario);
        }

        public async Task<bool> AsignarRolAsync(Usuario usuario, string rol)
        {
            // Verificar que el rol existe
            if (!await _roleManager.RoleExistsAsync(rol))
                return false;

            var result = await _userManager.AddToRoleAsync(usuario, rol);
            return result.Succeeded;
        }

        public async Task<bool> EliminarRolAsync(Usuario usuario, string rol)
        {
            var result = await _userManager.RemoveFromRoleAsync(usuario, rol);
            return result.Succeeded;
        }

        public async Task<bool> RestablecerPasswordAsync(Usuario usuario, string nuevaPassword)
        {
            try
            {
                if (usuario == null || string.IsNullOrEmpty(nuevaPassword))
                {
                    return false;
                }

                // Generar token para restablecer contraseña
                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                
                // Restablecer contraseña con el token generado
                var result = await _userManager.ResetPasswordAsync(usuario, token, nuevaPassword);
                
                if (!result.Succeeded)
                {
                    // Si hay errores, podríamos registrarlos para diagnóstico
                    var errores = string.Join(", ", result.Errors.Select(e => e.Description));
                    Console.WriteLine($"Error al restablecer contraseña: {errores}");
                }
                
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                // Capturar y registrar cualquier excepción
                Console.WriteLine($"Excepción al restablecer contraseña: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<Usuario>> ObtenerUsuariosPorRolAsync(string rol)
        {
            var usuarios = await _userManager.GetUsersInRoleAsync(rol);
            return usuarios;
        }

        public async Task<bool> AsignarVendedorAClienteAsync(string clienteId, string vendedorId)
        {
            try
            {
                var cliente = await _userManager.FindByIdAsync(clienteId);
                var vendedor = await _userManager.FindByIdAsync(vendedorId);

                if (cliente == null || vendedor == null)
                    return false;

                var rolesCliente = await _userManager.GetRolesAsync(cliente);
                var rolesVendedor = await _userManager.GetRolesAsync(vendedor);

                if (!rolesCliente.Contains("Cliente") || !rolesVendedor.Contains("Vendedor"))
                    return false;

                cliente.VendedorAsignadoId = vendedorId;
                var result = await _userManager.UpdateAsync(cliente);
                return result.Succeeded;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
} 