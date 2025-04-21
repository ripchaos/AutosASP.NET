using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autos.Data;
using Autos.Models;
using Autos.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Autos.Services
{
    public class SucursalService : ISucursalService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public SucursalService(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<Sucursal?> ObtenerSucursalPorIdAsync(int id)
        {
            return await _context.Sucursales.FindAsync(id);
        }

        public async Task<IEnumerable<Sucursal>> ObtenerTodasLasSucursalesAsync()
        {
            return await _context.Sucursales.ToListAsync();
        }

        public async Task<bool> CrearSucursalAsync(Sucursal sucursal)
        {
            try
            {
                _context.Sucursales.Add(sucursal);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ActualizarSucursalAsync(Sucursal sucursal)
        {
            try
            {
                _context.Sucursales.Update(sucursal);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EliminarSucursalAsync(int id)
        {
            try
            {
                var sucursal = await _context.Sucursales.FindAsync(id);
                if (sucursal == null)
                    return false;

                _context.Sucursales.Remove(sucursal);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AsignarVendedorASucursalAsync(string vendedorId, int sucursalId, bool esPrincipal = false)
        {
            try
            {
                // Crear la asignación
                var asignacion = new UsuarioSucursal
                {
                    UsuarioId = vendedorId,
                    SucursalId = sucursalId,
                    EsPrincipal = esPrincipal
                };

                // Si es principal, actualizar las demás asignaciones
                if (esPrincipal)
                {
                    var asignacionesExistentes = await _context.UsuariosSucursales
                        .Where(us => us.UsuarioId == vendedorId && us.EsPrincipal)
                        .ToListAsync();

                    foreach (var asignacionExistente in asignacionesExistentes)
                    {
                        asignacionExistente.EsPrincipal = false;
                    }
                }

                _context.UsuariosSucursales.Add(asignacion);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DesasignarVendedorDeSucursalAsync(int asignacionId)
        {
            try
            {
                var asignacion = await _context.UsuariosSucursales.FindAsync(asignacionId);
                if (asignacion == null)
                    return false;

                _context.UsuariosSucursales.Remove(asignacion);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Usuario>> ObtenerVendedoresDeSucursalAsync(int sucursalId)
        {
            var usuariosIds = await _context.UsuariosSucursales
                .Where(us => us.SucursalId == sucursalId)
                .Select(us => us.UsuarioId)
                .ToListAsync();

            var usuarios = new List<Usuario>();
            foreach (var id in usuariosIds)
            {
                var usuario = await _userManager.FindByIdAsync(id);
                if (usuario != null)
                {
                    var roles = await _userManager.GetRolesAsync(usuario);
                    if (roles.Contains("Vendedor"))
                    {
                        usuarios.Add(usuario);
                    }
                }
            }

            return usuarios;
        }

        public async Task<Sucursal?> ObtenerSucursalDeGerenteAsync(string gerenteId)
        {
            return await _context.Sucursales
                .FirstOrDefaultAsync(s => s.GerenteId == gerenteId);
        }

        public async Task<IEnumerable<UsuarioSucursal>> ObtenerAsignacionesDeSucursalAsync(int sucursalId)
        {
            return await _context.UsuariosSucursales
                .Include(us => us.Usuario)
                .Include(us => us.Sucursal)
                .Where(us => us.SucursalId == sucursalId)
                .ToListAsync();
        }

        public async Task<UsuarioSucursal?> ObtenerAsignacionUsuarioSucursalAsync(string usuarioId, bool soloActivas = true)
        {
            var query = _context.UsuariosSucursales
                .Include(us => us.Sucursal)
                .Where(us => us.UsuarioId == usuarioId);
                
            if (soloActivas)
            {
                query = query.Where(us => us.Activo);
            }
            
            return await query.FirstOrDefaultAsync();
        }
    }
} 