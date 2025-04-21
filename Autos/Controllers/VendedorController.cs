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
    [Authorize(Roles = "Vendedor")]
    public class VendedorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IUsuarioService _usuarioService;
        private readonly IVentaService _ventaService;
        private readonly IReservaService _reservaService;
        private readonly ISolicitudDescuentoService _solicitudDescuentoService;

        public VendedorController(
            ApplicationDbContext context,
            UserManager<Usuario> userManager,
            IUsuarioService usuarioService,
            IVentaService ventaService,
            IReservaService reservaService,
            ISolicitudDescuentoService solicitudDescuentoService)
        {
            _context = context;
            _userManager = userManager;
            _usuarioService = usuarioService;
            _ventaService = ventaService;
            _reservaService = reservaService;
            _solicitudDescuentoService = solicitudDescuentoService;
        }

        // GET: Vendedor/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Obtener el vendedor actual
                var vendedor = await _usuarioService.ObtenerUsuarioActualAsync();
                if (vendedor == null)
                {
                    return NotFound();
                }

                // Preparar el modelo para la vista
                var viewModel = new VendedorDashboardViewModel
                {
                    Vendedor = vendedor
                };

                // Obtener datos de solicitudes de descuento
                var solicitudes = await _solicitudDescuentoService
                    .ObtenerSolicitudesPorVendedorAsync(vendedor.Id);
                viewModel.SolicitudesDescuento = solicitudes.ToList();

                // Obtener datos de reservas activas
                var reservas = await _reservaService
                    .ObtenerReservasPorVendedorAsync(vendedor.Id);
                viewModel.ReservasActivas = reservas.ToList();

                // Obtener datos de ventas recientes
                var ventas = await _ventaService
                    .ObtenerVentasPorVendedorAsync(vendedor.Id);
                viewModel.VentasRecientes = ventas.ToList();

                // Obtener estadísticas
                viewModel.TotalSolicitudesPendientes = await _context.SolicitudesDescuento
                    .CountAsync(s => s.VendedorId == vendedor.Id && !s.Aprobada.HasValue);

                viewModel.TotalReservasActivas = await _context.Reservas
                    .CountAsync(r => r.VendedorId == vendedor.Id && r.Estado == "Activa");

                viewModel.TotalVentas = await _context.Ventas
                    .CountAsync(v => v.VendedorId == vendedor.Id);

                viewModel.TotalVentasMes = await _ventaService
                    .ContarVentasMesAsync(vendedor.Id, DateTime.Now.Month, DateTime.Now.Year);

                // Calcular monto total de ventas
                viewModel.MontoTotalVentas = await _ventaService
                    .CalcularMontoTotalVentasAsync(vendedor.Id);

                // Obtener clientes recientes del vendedor
                var clientesAsignados = await _userManager.Users
                    .Where(u => u.VendedorAsignadoId != null && u.VendedorAsignadoId == vendedor.Id && u.Rol == "Cliente")
                    .ToListAsync();

                viewModel.ClientesRecientes = clientesAsignados.Take(10).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Capturar cualquier excepción y mostrar un mensaje amigable
                TempData["Error"] = "Ocurrió un error al cargar el dashboard: " + ex.Message;
                return View(new VendedorDashboardViewModel { Vendedor = await _usuarioService.ObtenerUsuarioActualAsync() ?? throw new InvalidOperationException("No se pudo obtener el usuario actual") });
            }
        }

        // GET: Vendedor/SolicitudesDescuento
        public async Task<IActionResult> SolicitudesDescuento()
        {
            // Obtener el vendedor actual
            var vendedor = await _userManager.GetUserAsync(User);
            if (vendedor == null)
            {
                return NotFound();
            }

            // Obtener todas las solicitudes del vendedor
            var solicitudes = await _context.SolicitudesDescuento
                .Include(s => s.Auto)
                .Include(s => s.Vendedor)
                .Include(s => s.Gerente)
                .Where(s => s.VendedorId == vendedor.Id)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            return View(solicitudes);
        }

        // GET: Vendedor/Reservas
        public async Task<IActionResult> Reservas()
        {
            // Obtener el vendedor actual
            var vendedor = await _userManager.GetUserAsync(User);
            if (vendedor == null)
            {
                return NotFound();
            }

            // Obtener todas las reservas del vendedor
            var reservas = await _context.Reservas
                .Include(r => r.Auto)
                .Include(r => r.Usuario)
                .Where(r => r.VendedorId == vendedor.Id)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();

            return View(reservas);
        }

        // GET: Vendedor/Ventas
        public async Task<IActionResult> Ventas()
        {
            // Obtener el vendedor actual
            var vendedor = await _userManager.GetUserAsync(User);
            if (vendedor == null)
            {
                return NotFound();
            }

            // Obtener todas las ventas del vendedor
            var ventas = await _context.Ventas
                .Include(v => v.Auto)
                .Include(v => v.Cliente)
                .Where(v => v.VendedorId == vendedor.Id)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            return View(ventas);
        }

        // GET: Vendedor/Clientes
        public async Task<IActionResult> Clientes()
        {
            // Obtener el vendedor actual
            var vendedor = await _userManager.GetUserAsync(User);
            if (vendedor == null)
            {
                return NotFound();
            }

            // Obtener todos los clientes únicos del vendedor basado en ventas
            var clientesIds = await _context.Ventas
                .Where(v => v.VendedorId == vendedor.Id && v.Cliente != null)
                .Select(v => v.ClienteId)
                .Distinct()
                .ToListAsync();

            // También incluir clientes que tienen asignado a este vendedor aunque no tengan ventas aún
            var clientesAsignados = await _userManager.Users
                .Where(u => u.VendedorAsignadoId == vendedor.Id && u.Rol == "Cliente")
                .Select(u => u.Id)
                .ToListAsync();

            // Combinar las dos listas eliminando duplicados
            clientesIds = clientesIds.Union(clientesAsignados).ToList();

            var clientes = await _userManager.Users
                .Where(u => clientesIds.Contains(u.Id))
                .ToListAsync();

            return View(clientes);
        }

        // GET: Vendedor/DetalleCliente/id
        public async Task<IActionResult> DetalleCliente(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Obtener el vendedor actual
            var vendedor = await _userManager.GetUserAsync(User);
            if (vendedor == null)
            {
                return NotFound();
            }

            // Obtener cliente
            var cliente = await _userManager.FindByIdAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }

            // Verificar que el cliente pertenece a este vendedor (por ventas o por asignación)
            var clienteEsDelVendedor = await _context.Ventas
                .AnyAsync(v => v.VendedorId == vendedor.Id && v.ClienteId == id) ||
                cliente.VendedorAsignadoId != null && cliente.VendedorAsignadoId == vendedor.Id;

            if (!clienteEsDelVendedor)
            {
                return Forbid();
            }

            // Obtener historial de ventas del cliente con este vendedor
            var ventas = await _context.Ventas
                .Include(v => v.Auto)
                .Where(v => v.VendedorId == vendedor.Id && v.ClienteId == id)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            // Para cada venta, asegurarse de obtener la información actualizada del auto
            foreach (var venta in ventas.Where(v => v.Auto != null))
            {
                // Recargar el estado actual del auto
                var autoActualizado = await _context.Autos.FindAsync(venta.AutoId);
                if (autoActualizado != null && venta.Auto != null)
                {
                    // Actualizar las propiedades de estado
                    venta.Auto.Disponibilidad = autoActualizado.Disponibilidad;
                    venta.Auto.EstadoReserva = autoActualizado.EstadoReserva;
                }
            }

            // Crear el modelo de vista
            var viewModel = new ClienteDetalleViewModel
            {
                Cliente = cliente,
                Ventas = ventas,
                TotalCompras = ventas.Count,
                MontoTotal = ventas.Sum(v => v.MontoFinal)
            };

            return View(viewModel);
        }

        // POST: Vendedor/AsignarAutoInteresado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarAutoInteresado(string clienteId, int autoId, bool intentarReservar = false)
        {
            if (string.IsNullOrEmpty(clienteId) || autoId <= 0)
            {
                TempData["Error"] = "Datos inválidos para asignar auto interesado.";
                return RedirectToAction("Clientes");
            }

            // Obtener el vendedor actual
            var vendedor = await _userManager.GetUserAsync(User);
            if (vendedor == null)
            {
                return NotFound();
            }

            // Obtener cliente
            var cliente = await _userManager.FindByIdAsync(clienteId);
            if (cliente == null)
            {
                TempData["Error"] = "Cliente no encontrado.";
                return RedirectToAction("Clientes");
            }

            // Verificar que el cliente pertenece a este vendedor
            if (cliente.VendedorAsignadoId == null || cliente.VendedorAsignadoId != vendedor.Id)
            {
                // Si el cliente no está asignado a este vendedor, asignarlo
                cliente.VendedorAsignadoId = vendedor.Id;
            }

            // Verificar que el auto existe
            var auto = await _context.Autos.FindAsync(autoId);
            if (auto == null)
            {
                TempData["Error"] = "Auto no encontrado.";
                return RedirectToAction("DetalleCliente", new { id = clienteId });
            }

            // Asignar auto interesado
            cliente.AutoInteresadoId = autoId;
            await _userManager.UpdateAsync(cliente);

            TempData["Success"] = $"Auto {auto.Marca} {auto.Modelo} asignado como interés del cliente.";
            
            // Si intentarReservar es true, redirigir a la acción ReservarAuto
            if (intentarReservar)
            {
                return RedirectToAction("ReservarAuto", new { clienteId, autoId });
            }
            
            return RedirectToAction("DetalleCliente", new { id = clienteId });
        }

        // GET: Vendedor/SeleccionarAuto/clienteId?marca=&modelo=
        public async Task<IActionResult> SeleccionarAuto(string clienteId, string marca = "", string modelo = "", int? anioDesde = null, int? anioHasta = null, bool intentarReservar = false)
        {
            if (string.IsNullOrEmpty(clienteId))
            {
                TempData["Error"] = "ID de cliente inválido.";
                return RedirectToAction("Clientes");
            }

            // Obtener el vendedor actual
            var vendedor = await _userManager.GetUserAsync(User);
            if (vendedor == null)
            {
                return NotFound();
            }

            // Obtener cliente
            var cliente = await _userManager.FindByIdAsync(clienteId);
            if (cliente == null)
            {
                TempData["Error"] = "Cliente no encontrado.";
                return RedirectToAction("Clientes");
            }

            // Verificar que el cliente pertenece a este vendedor o está asignado a él
            var clienteEsDelVendedor = await _context.Ventas
                .AnyAsync(v => v.VendedorId == vendedor.Id && v.ClienteId == clienteId) ||
                cliente.VendedorAsignadoId != null && cliente.VendedorAsignadoId == vendedor.Id;

            if (!clienteEsDelVendedor)
            {
                return Forbid();
            }

            // Obtener autos disponibles
            // Primero obtenemos la sucursal del vendedor
            var sucursalVendedor = await _context.UsuariosSucursales
                .FirstOrDefaultAsync(us => us.UsuarioId == vendedor.Id && us.Activo);

            if (sucursalVendedor == null)
            {
                TempData["Error"] = "No se encontró una sucursal activa para este vendedor.";
                return RedirectToAction("DetalleCliente", new { id = clienteId });
            }

            // Obtener lista de marcas disponibles para el filtro
            var marcasDisponibles = await _context.Autos
                .Where(a => a.SucursalId == sucursalVendedor.SucursalId && a.Disponibilidad)
                .Select(a => a.Marca)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            // Aplicar filtros si existen
            var autosQuery = _context.Autos
                .Where(a => a.SucursalId == sucursalVendedor.SucursalId && a.Disponibilidad);

            if (!string.IsNullOrEmpty(marca))
            {
                autosQuery = autosQuery.Where(a => a.Marca == marca);
            }

            if (!string.IsNullOrEmpty(modelo))
            {
                autosQuery = autosQuery.Where(a => a.Modelo.Contains(modelo));
            }

            if (anioDesde.HasValue)
            {
                autosQuery = autosQuery.Where(a => a.Anio >= anioDesde.Value);
            }

            if (anioHasta.HasValue)
            {
                autosQuery = autosQuery.Where(a => a.Anio <= anioHasta.Value);
            }

            var autos = await autosQuery
                .OrderBy(a => a.Marca)
                .ThenBy(a => a.Modelo)
                .ToListAsync();

            // Configurar ViewBag para la vista
            ViewBag.ClienteId = clienteId;
            ViewBag.ClienteNombre = cliente.Nombre;
            ViewBag.Marcas = marcasDisponibles;
            ViewBag.FiltroMarca = marca;
            ViewBag.FiltroModelo = modelo;
            ViewBag.FiltroAnioDesde = anioDesde;
            ViewBag.FiltroAnioHasta = anioHasta;
            ViewBag.IntentarReservar = intentarReservar;

            return View(autos);
        }

        // GET: Vendedor/ReservarAuto/clienteId/autoId
        public async Task<IActionResult> ReservarAuto(string clienteId, int autoId)
        {
            if (string.IsNullOrEmpty(clienteId) || autoId <= 0)
            {
                TempData["Error"] = "Datos inválidos para la reserva.";
                return RedirectToAction("Clientes");
            }

            // Obtener el vendedor actual
            var vendedor = await _userManager.GetUserAsync(User);
            if (vendedor == null)
            {
                return NotFound();
            }

            // Obtener cliente
            var cliente = await _userManager.FindByIdAsync(clienteId);
            if (cliente == null)
            {
                TempData["Error"] = "Cliente no encontrado.";
                return RedirectToAction("Clientes");
            }

            // Verificar que el cliente pertenece a este vendedor o está asignado a él
            var clienteEsDelVendedor = await _context.Ventas
                .AnyAsync(v => v.VendedorId == vendedor.Id && v.ClienteId == clienteId) ||
                cliente.VendedorAsignadoId != null && cliente.VendedorAsignadoId == vendedor.Id;

            if (!clienteEsDelVendedor)
            {
                return Forbid();
            }

            // Verificar que el auto existe y está disponible
            var auto = await _context.Autos
                .Include(a => a.Sucursal)
                .FirstOrDefaultAsync(a => a.Id == autoId);

            if (auto == null)
            {
                TempData["Error"] = "Auto no encontrado.";
                return RedirectToAction("DetalleCliente", new { id = clienteId });
            }

            if (!auto.Disponibilidad || auto.EstadoReserva != "Disponible")
            {
                TempData["Error"] = "El auto seleccionado no está disponible para reserva.";
                return RedirectToAction("DetalleCliente", new { id = clienteId });
            }

            // Preparar el modelo para la vista
            var viewModel = new ReservarAutoDesdeVendedorViewModel
            {
                ClienteId = clienteId,
                NombreCliente = cliente.Nombre,
                Auto = auto,
                VendedorId = vendedor.Id,
                DuracionReserva = 3 // Valor predeterminado de 3 días
            };

            return View(viewModel);
        }

        // POST: Vendedor/ReservarAuto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReservarAuto(ReservarAutoDesdeVendedorViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Recargar auto completo para la vista si el Id está presente
                    if (model.Auto != null && model.Auto.Id > 0)
                    {
                        var autoRecargado = await _context.Autos
                            .Include(a => a.Sucursal)
                            .FirstOrDefaultAsync(a => a.Id == model.Auto.Id);
                        
                        // Solo asignar si no es nulo
                        if (autoRecargado != null)
                        {
                            model.Auto = autoRecargado;
                        }
                        else
                        {
                            TempData["Error"] = "No se pudo cargar la información del auto.";
                            return RedirectToAction("DetalleCliente", new { id = model.ClienteId });
                        }
                    }
                    
                    // Obtener el nombre del cliente si no está establecido
                    if (string.IsNullOrEmpty(model.NombreCliente) && !string.IsNullOrEmpty(model.ClienteId))
                    {
                        var usuarioCliente = await _userManager.FindByIdAsync(model.ClienteId);
                        if (usuarioCliente != null)
                        {
                            model.NombreCliente = usuarioCliente.Nombre;
                        }
                    }
                    
                    return View(model);
                }

                // Verificar que el auto existe y está disponible
                if (model.Auto == null || model.Auto.Id <= 0)
                {
                    TempData["Error"] = "No se especificó un auto válido.";
                    return RedirectToAction("DetalleCliente", new { id = model.ClienteId });
                }

                // Obtener el auto completo de la base de datos
                var auto = await _context.Autos.FindAsync(model.Auto.Id);
                if (auto == null)
                {
                    TempData["Error"] = "El auto seleccionado no existe.";
                    return RedirectToAction("DetalleCliente", new { id = model.ClienteId });
                }

                // Verificar disponibilidad
                if (!auto.Disponibilidad || auto.EstadoReserva != "Disponible")
                {
                    TempData["Error"] = "El auto ya no está disponible para reserva.";
                    return RedirectToAction("DetalleCliente", new { id = model.ClienteId });
                }

                // Verificar que el cliente existe
                var cliente = await _userManager.FindByIdAsync(model.ClienteId);
                if (cliente == null)
                {
                    TempData["Error"] = "El cliente no existe o no se encuentra en el sistema.";
                    return RedirectToAction("Clientes");
                }

                // Crear la reserva
                var reserva = new Reserva
                {
                    UsuarioId = model.ClienteId,
                    AutoId = auto.Id,
                    FechaReserva = DateTime.Now,
                    FechaExpiracion = DateTime.Now.AddDays(model.DuracionReserva),
                    Estado = "Activa",
                    VendedorId = model.VendedorId,
                    Comentarios = model.Comentarios
                };

                _context.Reservas.Add(reserva);

                // Actualizar el estado del auto
                auto.EstadoReserva = "Reservado";
                auto.FechaFinReserva = reserva.FechaExpiracion;
                _context.Autos.Update(auto);

                // Actualizar el auto interesado del cliente
                cliente.AutoInteresadoId = auto.Id;
                await _userManager.UpdateAsync(cliente);

                await _context.SaveChangesAsync();

                TempData["Exito"] = "La reserva se ha realizado exitosamente.";
                return RedirectToAction("SolicitudesDescuento");
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                TempData["Error"] = $"Error al crear la reserva: {ex.Message}";
                
                try
                {
                    // Intentar recargar el modelo con datos completos
                    if (model?.Auto != null && model.Auto.Id > 0)
                    {
                        var autoRecargado = await _context.Autos
                            .Include(a => a.Sucursal)
                            .FirstOrDefaultAsync(a => a.Id == model.Auto.Id);
                            
                        // Solo asignar si no es nulo
                        if (autoRecargado != null)
                        {
                            model.Auto = autoRecargado;
                        }
                    }
                }
                catch
                {
                    // Ignorar errores secundarios al recargar el auto
                }
                
                return View(model);
            }
        }

        // GET: Vendedor/IniciarVenta/clienteId/autoId
        public async Task<IActionResult> IniciarVenta(string clienteId, int? autoId = null)
        {
            if (string.IsNullOrEmpty(clienteId))
            {
                TempData["Error"] = "ID de cliente inválido.";
                return RedirectToAction("Clientes");
            }

            // Obtener el vendedor actual
            var vendedor = await _userManager.GetUserAsync(User);
            if (vendedor == null)
            {
                return NotFound();
            }

            // Obtener cliente
            var cliente = await _userManager.FindByIdAsync(clienteId);
            if (cliente == null)
            {
                TempData["Error"] = "Cliente no encontrado.";
                return RedirectToAction("Clientes");
            }

            // Verificar que el cliente pertenece a este vendedor o está asignado a él
            var clienteEsDelVendedor = await _context.Ventas
                .AnyAsync(v => v.VendedorId == vendedor.Id && v.ClienteId == clienteId) ||
                cliente.VendedorAsignadoId != null && cliente.VendedorAsignadoId == vendedor.Id;

            if (!clienteEsDelVendedor)
            {
                return Forbid();
            }

            // Si no se pasó un autoId, pero el cliente tiene un auto interesado, usarlo
            if (autoId == null && cliente.AutoInteresadoId.HasValue)
            {
                autoId = cliente.AutoInteresadoId.Value;
            }

            // Redirigir al controlador de Ventas con los datos pre-cargados
            return RedirectToAction("Create", "Ventas", new { clienteId, autoId });
        }

        // Obtener el ID del usuario actual
        private string GetCurrentUserId()
        {
            return _userManager.GetUserId(User) ?? string.Empty;
        }

        // Clase auxiliar para comparar usuarios por ID y evitar duplicados
        private class UsuarioComparer : IEqualityComparer<Usuario>
        {
            public bool Equals(Usuario? x, Usuario? y)
            {
                if (x == null || y == null)
                    return false;
                return x.Id == y.Id;
            }

            public int GetHashCode(Usuario obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
} 