using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Autos.Data;
using Autos.Models;
using Autos.Models.ViewModels;
using Autos.Services.Interfaces;

namespace Autos.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClienteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IUsuarioService _usuarioService;
        private readonly IReservaService _reservaService;
        private readonly IVentaService _ventaService;
        private readonly IAutoService _autoService;

        public ClienteController(
            ApplicationDbContext context,
            UserManager<Usuario> userManager,
            IUsuarioService usuarioService,
            IReservaService reservaService,
            IVentaService ventaService,
            IAutoService autoService)
        {
            _context = context;
            _userManager = userManager;
            _usuarioService = usuarioService;
            _reservaService = reservaService;
            _ventaService = ventaService;
            _autoService = autoService;
        }

        // GET: Cliente/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Obtener el cliente actual
                var cliente = await _usuarioService.ObtenerUsuarioActualAsync();
                if (cliente == null)
                {
                    return NotFound();
                }

                // Verificar el rol del usuario
                var roles = await _usuarioService.ObtenerRolesDeUsuarioAsync(cliente);
                if (!roles.Contains("Cliente"))
                {
                    return RedirectToAction("Index", "Home");
                }

                // Obtener el vendedor asignado al cliente
                Usuario? vendedorAsignado = null;
                if (!string.IsNullOrEmpty(cliente.VendedorAsignadoId))
                {
                    vendedorAsignado = await _usuarioService.ObtenerUsuarioPorIdAsync(cliente.VendedorAsignadoId);
                }

                // Obtener auto de interés si existe
                Auto? autoInteres = null;
                if (cliente.AutoInteresadoId.HasValue)
                {
                    autoInteres = await _autoService.ObtenerAutoPorIdAsync(cliente.AutoInteresadoId.Value);
                }

                // Obtener las reservas activas del cliente
                var reservasActivas = await _reservaService.ObtenerReservasPorClienteAsync(cliente.Id);
                reservasActivas = reservasActivas.Where(r => r.Estado == "Activa" || r.Estado == "Pendiente").ToList();

                // Obtener historial de compras del cliente
                var compras = await _ventaService.ObtenerVentasPorClienteAsync(cliente.Id);

                // Crear el viewmodel
                var viewModel = new ClienteDashboardViewModel
                {
                    Cliente = cliente,
                    VendedorAsignado = vendedorAsignado,
                    AutoInteres = autoInteres,
                    ReservasActivas = reservasActivas.ToList(),
                    HistorialCompras = compras.ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Registrar el error
                Console.WriteLine($"Error en Dashboard de Cliente: {ex.Message}");
                TempData["Error"] = "Ha ocurrido un error al cargar el dashboard. Por favor, intente nuevamente.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Cliente/MisReservas
        public async Task<IActionResult> MisReservas()
        {
            try
            {
                var cliente = await _usuarioService.ObtenerUsuarioActualAsync();
                if (cliente == null)
                {
                    return NotFound();
                }

                var reservas = await _reservaService.ObtenerReservasPorClienteAsync(cliente.Id);
                
                // Cargar la información completa de cada reserva
                foreach (var reserva in reservas)
                {
                    if (reserva.Auto != null && reserva.Auto.Sucursal == null && reserva.Auto.SucursalId > 0)
                    {
                        reserva.Auto.Sucursal = await _context.Sucursales.FindAsync(reserva.Auto.SucursalId);
                    }
                }
                
                return View(reservas);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en MisReservas: {ex.Message}");
                TempData["Error"] = "Ha ocurrido un error al cargar sus reservas.";
                return RedirectToAction("Dashboard");
            }
        }

        // GET: Cliente/MisCompras
        public async Task<IActionResult> MisCompras()
        {
            try
            {
                var cliente = await _usuarioService.ObtenerUsuarioActualAsync();
                if (cliente == null)
                {
                    return NotFound();
                }

                var compras = await _ventaService.ObtenerVentasPorClienteAsync(cliente.Id);
                
                // Cargar la información completa de cada compra
                foreach (var compra in compras)
                {
                    if (compra.Vendedor == null && !string.IsNullOrEmpty(compra.VendedorId))
                    {
                        compra.Vendedor = await _userManager.FindByIdAsync(compra.VendedorId);
                    }
                    
                    if (compra.Auto != null && compra.Auto.Sucursal == null && compra.Auto.SucursalId > 0)
                    {
                        compra.Auto.Sucursal = await _context.Sucursales.FindAsync(compra.Auto.SucursalId);
                    }
                }
                
                return View(compras);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en MisCompras: {ex.Message}");
                TempData["Error"] = "Ha ocurrido un error al cargar sus compras.";
                return RedirectToAction("Dashboard");
            }
        }

        // GET: Cliente/DetalleReserva/5
        public async Task<IActionResult> DetalleReserva(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _usuarioService.ObtenerUsuarioActualAsync();
            if (cliente == null)
            {
                return NotFound();
            }

            var reserva = await _reservaService.ObtenerReservaPorIdAsync(id.Value);
            if (reserva == null || reserva.UsuarioId != cliente.Id)
            {
                return NotFound();
            }

            // Cargar datos relacionados
            if (reserva.Auto != null && reserva.Auto.Sucursal == null && reserva.Auto.SucursalId > 0)
            {
                reserva.Auto.Sucursal = await _context.Sucursales.FindAsync(reserva.Auto.SucursalId);
            }

            if (reserva.Vendedor == null && !string.IsNullOrEmpty(reserva.VendedorId))
            {
                reserva.Vendedor = await _userManager.FindByIdAsync(reserva.VendedorId);
            }

            return View(reserva);
        }

        // GET: Cliente/DetalleCompra/5
        public async Task<IActionResult> DetalleCompra(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _usuarioService.ObtenerUsuarioActualAsync();
            if (cliente == null)
            {
                return NotFound();
            }

            var venta = await _ventaService.ObtenerVentaPorIdAsync(id.Value);
            if (venta == null || venta.ClienteId != cliente.Id)
            {
                return NotFound();
            }

            // Cargar datos relacionados
            if (venta.Auto != null && venta.Auto.Sucursal == null && venta.Auto.SucursalId > 0)
            {
                venta.Auto.Sucursal = await _context.Sucursales.FindAsync(venta.Auto.SucursalId);
            }

            if (venta.Vendedor == null && !string.IsNullOrEmpty(venta.VendedorId))
            {
                venta.Vendedor = await _userManager.FindByIdAsync(venta.VendedorId);
            }

            return View(venta);
        }

        // GET: Cliente/MiPerfil
        public async Task<IActionResult> MiPerfil()
        {
            var cliente = await _usuarioService.ObtenerUsuarioActualAsync();
            if (cliente == null)
            {
                return NotFound();
            }

            // Obtener el vendedor asignado al cliente
            Usuario? vendedorAsignado = null;
            if (!string.IsNullOrEmpty(cliente.VendedorAsignadoId))
            {
                vendedorAsignado = await _usuarioService.ObtenerUsuarioPorIdAsync(cliente.VendedorAsignadoId);
            }

            var viewModel = new ClientePerfilViewModel
            {
                Cliente = cliente,
                VendedorAsignado = vendedorAsignado
            };

            return View(viewModel);
        }

        // GET: Cliente/CancelarReserva/5
        public async Task<IActionResult> CancelarReserva(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _usuarioService.ObtenerUsuarioActualAsync();
            if (cliente == null)
            {
                return NotFound();
            }

            var reserva = await _reservaService.ObtenerReservaPorIdAsync(id.Value);
            if (reserva == null || reserva.UsuarioId != cliente.Id)
            {
                return NotFound();
            }

            // Verificar que la reserva esté en estado que permita cancelación
            if (reserva.Estado != "Activa" && reserva.Estado != "Pendiente")
            {
                TempData["Error"] = "Solo se pueden cancelar reservas activas o pendientes.";
                return RedirectToAction(nameof(DetalleReserva), new { id = id });
            }

            // Mostrar vista de confirmación con el modelo
            return View(reserva);
        }

        // POST: Cliente/CancelarReserva/5
        [HttpPost, ActionName("CancelarReserva")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarReservaConfirmado(int id, string motivo)
        {
            var cliente = await _usuarioService.ObtenerUsuarioActualAsync();
            if (cliente == null)
            {
                return NotFound();
            }

            var reserva = await _reservaService.ObtenerReservaPorIdAsync(id);
            if (reserva == null || reserva.UsuarioId != cliente.Id)
            {
                return NotFound();
            }

            // Verificar que la reserva esté en estado que permita cancelación
            if (reserva.Estado != "Activa" && reserva.Estado != "Pendiente")
            {
                TempData["Error"] = "Solo se pueden cancelar reservas activas o pendientes.";
                return RedirectToAction(nameof(DetalleReserva), new { id = id });
            }

            // Llamar al servicio para cancelar la reserva
            var resultado = await _reservaService.CancelarReservaAsync(id, motivo);
            
            if (resultado)
            {
                TempData["Success"] = "La reserva ha sido cancelada correctamente.";
                return RedirectToAction(nameof(MisReservas));
            }
            else
            {
                TempData["Error"] = "Ha ocurrido un error al cancelar la reserva. Por favor, intente nuevamente.";
                return RedirectToAction(nameof(DetalleReserva), new { id = id });
            }
        }
    }
} 