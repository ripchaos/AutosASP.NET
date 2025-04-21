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
using System.Security.Claims;
using Autos.Services.Interfaces;

namespace Autos.Controllers
{
    [Authorize(Roles = "Gerente")]
    public class GerenteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IUsuarioService _usuarioService;
        private readonly ISucursalService _sucursalService;
        private readonly IVentaService _ventaService;
        private readonly IAutoService _autoService;

        public GerenteController(
            ApplicationDbContext context,
            UserManager<Usuario> userManager,
            IUsuarioService usuarioService,
            ISucursalService sucursalService,
            IVentaService ventaService,
            IAutoService autoService)
        {
            _context = context;
            _userManager = userManager;
            _usuarioService = usuarioService;
            _sucursalService = sucursalService;
            _ventaService = ventaService;
            _autoService = autoService;
        }

        // GET: Gerente/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Obtener el gerente actual
                var usuario = await _usuarioService.ObtenerUsuarioActualAsync();
                if (usuario == null)
                {
                    return NotFound();
                }

                // Obtener la sucursal que administra el gerente
                var sucursal = await _sucursalService.ObtenerSucursalDeGerenteAsync(usuario.Id);

                if (sucursal == null)
                {
                    TempData["Error"] = "No se encontró una sucursal asignada a este gerente.";
                    return View();
                }

                // Preparar el modelo para la vista
                var viewModel = new GerenteDashboardViewModel
                {
                    Sucursal = sucursal,
                    // Inicializar listas vacías para evitar errores de referencia nula
                    VentasPorMes = new List<object>(),
                    VentasPorVendedor = new List<object>(),
                    AutosMasVendidos = new List<object>(),
                    UltimasVentas = new List<Venta>()
                };

                // Obtener datos de ventas por mes para la sucursal de forma segura
                var fechaInicio = DateTime.Now.AddMonths(-6);
                var ventas = await _context.Ventas
                    .Include(v => v.Auto)
                    .Include(v => v.Vendedor)
                    .Where(v => v.Auto != null && v.Auto.SucursalId == sucursal.Id && v.Fecha >= fechaInicio)
                    .Select(v => new { 
                        v.Id, 
                        v.Monto, 
                        v.MontoFinal, 
                        v.PorcentajeDescuento, 
                        v.Fecha, 
                        v.VendedorId, 
                        Vendedor = v.Vendedor, 
                        Auto = v.Auto 
                    })
                    .ToListAsync();

                if (ventas.Any())
                {
                    // Agrupar ventas por mes
                    var ventasPorMes = ventas
                        .GroupBy(v => new { v.Fecha.Year, v.Fecha.Month })
                        .Select(g => new
                        {
                            Periodo = $"{g.Key.Year}-{g.Key.Month}",
                            Total = g.Sum(v => v.MontoFinal),
                            Cantidad = g.Count()
                        })
                        .OrderBy(x => x.Periodo)
                        .ToList();

                    viewModel.VentasPorMes = ventasPorMes.Cast<object>().ToList();

                    // Ventas por vendedor
                    var ventasPorVendedor = ventas
                        .Where(v => v.Vendedor != null)
                        .GroupBy(v => v.VendedorId)
                        .Select(g => new
                        {
                            VendedorId = g.Key,
                            NombreVendedor = g.First().Vendedor!.Nombre,
                            TotalVentas = g.Sum(v => v.MontoFinal),
                            Cantidad = g.Count()
                        })
                        .OrderByDescending(x => x.TotalVentas)
                        .ToList();

                    viewModel.VentasPorVendedor = ventasPorVendedor.Cast<object>().ToList();

                    // Autos más vendidos
                    var autosMasVendidos = ventas
                        .Where(v => v.Auto != null)
                        .GroupBy(v => new { v.Auto!.Marca, v.Auto.Modelo })
                        .Select(g => new
                        {
                            Auto = $"{g.Key.Marca} {g.Key.Modelo}",
                            Cantidad = g.Count(),
                            TotalVentas = g.Sum(v => v.MontoFinal)
                        })
                        .OrderByDescending(x => x.Cantidad)
                        .Take(5)
                        .ToList();

                    viewModel.AutosMasVendidos = autosMasVendidos.Cast<object>().ToList();

                    // Convertir ventas a lista de Venta (solo las propiedades necesarias)
                    var ultimasVentas = ventas
                        .OrderByDescending(v => v.Fecha)
                        .Take(5)
                        .Select(v => new Venta
                        {
                            Id = v.Id,
                            Monto = v.Monto,
                            PorcentajeDescuento = v.PorcentajeDescuento,
                            Fecha = v.Fecha,
                            VendedorId = v.VendedorId,
                            Vendedor = v.Vendedor,
                            AutoId = v.Auto?.Id ?? 0,
                            Auto = v.Auto,
                            ClienteId = string.Empty, // Requerido pero no usado en la vista
                            Estado = "Completada" // Establecer un valor predeterminado para Estado
                        })
                        .ToList();

                    // Calcular MontoFinal para cada venta (ya que es una propiedad calculada)
                    foreach (var venta in ultimasVentas)
                    {
                        // Accedemos a MontoFinal para asegurarnos de que se calcule
                        decimal temp = venta.MontoFinal;
                    }

                    viewModel.UltimasVentas = ultimasVentas;
                }

                // Obtener métricas adicionales
                viewModel.TotalVentas = ventas.Count;
                viewModel.TotalIngresos = ventas.Sum(v => v.MontoFinal);
                viewModel.TotalVendedores = await _context.UsuariosSucursales
                    .CountAsync(us => us.SucursalId == sucursal.Id && us.Activo);
                viewModel.TotalInventario = await _context.Autos
                    .CountAsync(a => a.SucursalId == sucursal.Id && a.Disponibilidad);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Capturar cualquier excepción y mostrar un mensaje amigable
                TempData["Error"] = "Ocurrió un error al cargar el dashboard: " + ex.Message;
                var sucursalDefault = new Sucursal { 
                    Nombre = "Error al cargar la sucursal",
                    Direccion = "N/A" 
                };
                return View(new GerenteDashboardViewModel 
                { 
                    Sucursal = sucursalDefault,
                    VentasPorMes = new List<object>(),
                    VentasPorVendedor = new List<object>(),
                    AutosMasVendidos = new List<object>(),
                    UltimasVentas = new List<Venta>()
                });
            }
        }

        // GET: Gerente/VentasPorVendedor
        public async Task<IActionResult> VentasPorVendedor()
        {
            try
            {
                // Obtener el gerente actual y su sucursal
                var gerente = await _usuarioService.ObtenerUsuarioActualAsync();
                var sucursal = await _sucursalService.ObtenerSucursalDeGerenteAsync(gerente!.Id);

                if (sucursal == null)
                {
                    TempData["Error"] = "No se encontró una sucursal asignada a este gerente.";
                    return RedirectToAction("Dashboard");
                }

                // Obtener todos los vendedores de la sucursal con sus ventas
                var vendedoresSucursal = await _context.UsuariosSucursales
                    .Include(us => us.Usuario)
                    .Where(us => us.SucursalId == sucursal.Id && us.Activo)
                    .ToListAsync();

                var vendedoresIds = vendedoresSucursal.Select(us => us.UsuarioId).ToList();

                // Consultar las ventas seleccionando solo las propiedades necesarias
                var ventas = await _context.Ventas
                    .Include(v => v.Vendedor)
                    .Include(v => v.Auto)
                    .Where(v => vendedoresIds.Contains(v.VendedorId))
                    .Select(v => new {
                        v.Id,
                        v.VendedorId,
                        v.Vendedor,
                        v.Fecha,
                        v.MontoFinal,
                        v.Auto
                    })
                    .ToListAsync();

                // Agrupar resultados por vendedor
                var resultados = vendedoresSucursal
                    .Select(us => new VendedorVentasViewModel
                    {
                        Vendedor = us.Usuario,
                        TotalVentas = ventas.Count(v => v.VendedorId == us.UsuarioId),
                        MontoTotal = ventas.Where(v => v.VendedorId == us.UsuarioId).Sum(v => v.MontoFinal),
                        UltimaVenta = ventas.Where(v => v.VendedorId == us.UsuarioId)
                            .OrderByDescending(v => v.Fecha)
                            .Select(v => new Venta {
                                Id = v.Id,
                                VendedorId = v.VendedorId,
                                Vendedor = v.Vendedor,
                                Fecha = v.Fecha,
                                // Propiedades requeridas pero no usadas en la vista
                                ClienteId = string.Empty,
                                Monto = v.MontoFinal, // Usamos MontoFinal como Monto para que coincida
                                AutoId = v.Auto?.Id ?? 0,
                                Auto = v.Auto,
                                Estado = "Completada" // Establecer un valor predeterminado para Estado
                            })
                            .FirstOrDefault(),
                        EsPrincipal = us.EsPrincipal
                    })
                    .OrderByDescending(v => v.MontoTotal)
                    .ToList();

                return View(resultados);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al cargar las ventas por vendedor: " + ex.Message;
                return RedirectToAction("Dashboard");
            }
        }

        // GET: Gerente/AdministrarVendedores
        public async Task<IActionResult> AdministrarVendedores()
        {
            try 
            {
                // Obtener la sucursal del gerente
                var gerente = await _usuarioService.ObtenerUsuarioActualAsync();
                var sucursal = await _sucursalService.ObtenerSucursalDeGerenteAsync(gerente!.Id);

                if (sucursal == null)
                {
                    TempData["Error"] = "No se encontró una sucursal asignada a este gerente.";
                    return RedirectToAction("Dashboard");
                }

                // Obtener SOLO los vendedores de la sucursal del gerente
                var vendedoresSucursal = await _sucursalService.ObtenerAsignacionesDeSucursalAsync(sucursal.Id);
                
                var viewModel = new AdministrarVendedoresViewModel
                {
                    Sucursal = sucursal,
                    VendedoresAsignados = vendedoresSucursal.ToList(),
                    VendedoresDisponibles = new List<Usuario>() // Lista vacía ya que no pueden asignar vendedores
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al cargar la administración de vendedores: " + ex.Message;
                return RedirectToAction("Dashboard");
            }
        }

        // POST: Gerente/AsignarVendedor - Ya no es necesario, pero mantenemos el método para evitar errores
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AsignarVendedor(string vendedorId, int sucursalId, bool esPrincipal = false)
        {
            // Los gerentes no deberían poder acceder a esta funcionalidad
            TempData["Error"] = "No tiene permisos para asignar vendedores. Esta acción solo puede ser realizada por un administrador.";
            return RedirectToAction("AdministrarVendedores");
        }

        // POST: Gerente/DesasignarVendedor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesasignarVendedor(int asignacionId)
        {
            // Obtener la asignación
            var asignacion = await _context.UsuariosSucursales
                .Include(us => us.Sucursal)
                .FirstOrDefaultAsync(us => us.Id == asignacionId);

            if (asignacion == null)
            {
                TempData["Error"] = "La asignación no existe.";
                return RedirectToAction("AdministrarVendedores");
            }

            // Verificar que el gerente administre esta sucursal
            var gerente = await _usuarioService.ObtenerUsuarioActualAsync();
            if (asignacion.Sucursal?.GerenteId != gerente!.Id)
            {
                TempData["Error"] = "No tiene permiso para desasignar vendedores de otras sucursales.";
                return RedirectToAction("AdministrarVendedores");
            }

            // Verificar si el vendedor tiene ventas pendientes
            var ventas = await _ventaService.ObtenerVentasPorVendedorAsync(asignacion.UsuarioId);
            if (ventas.Any())
            {
                TempData["Error"] = "No se puede desasignar porque el vendedor tiene ventas registradas.";
                return RedirectToAction("AdministrarVendedores");
            }

            // Desasignar el vendedor
            var resultado = await _sucursalService.DesasignarVendedorDeSucursalAsync(asignacionId);
            
            if (resultado)
            {
                TempData["Success"] = "Vendedor desasignado correctamente de la sucursal.";
            }
            else
            {
                TempData["Error"] = "No se pudo desasignar al vendedor. Intente nuevamente.";
            }
            
            return RedirectToAction("AdministrarVendedores");
        }

        // GET: Gerente/DetalleVendedor/id
        public async Task<IActionResult> DetalleVendedor(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Obtener el vendedor
            var vendedor = await _usuarioService.ObtenerUsuarioPorIdAsync(id);
            
            if (vendedor == null)
            {
                return NotFound("Vendedor no encontrado");
            }

            // Verificar que es un vendedor
            var roles = await _usuarioService.ObtenerRolesDeUsuarioAsync(vendedor);
            if (!roles.Contains("Vendedor"))
            {
                return NotFound("El usuario no es un vendedor");
            }

            // Obtener la sucursal del gerente actual
            var gerente = await _usuarioService.ObtenerUsuarioActualAsync();
            var sucursal = await _sucursalService.ObtenerSucursalDeGerenteAsync(gerente!.Id);

            if (sucursal == null)
            {
                return NotFound("Sucursal no encontrada");
            }

            // Verificar que el vendedor está asignado a la sucursal
            var asignacion = await _context.UsuariosSucursales
                .FirstOrDefaultAsync(us => us.UsuarioId == id && us.SucursalId == sucursal.Id);

            if (asignacion == null)
            {
                return NotFound("El vendedor no está asignado a su sucursal");
            }

            // Obtener estadísticas de ventas
            var ventas = await _context.Ventas
                .Where(v => v.VendedorId == id)
                .Include(v => v.Auto)
                .ToListAsync();

            // Crear el modelo de la vista
            var viewModel = new VendedorDetalleViewModel
            {
                Vendedor = vendedor,
                TotalVentas = ventas.Count,
                MontoTotal = ventas.Sum(v => v.MontoFinal),
                VentasRecientes = ventas.OrderByDescending(v => v.Fecha).Take(5).ToList(),
                FechaAsignacion = asignacion.FechaAsignacion,
                EsPrincipal = asignacion.EsPrincipal,
                Activo = asignacion.Activo
            };

            // Devolver vista parcial para usarla en modal
            return PartialView("_DetalleVendedor", viewModel);
        }

        // Obtener el ID del usuario actual
        private string GetCurrentUserId()
        {
            // Debemos asegurarnos de que el User.Identity está autenticado
            if (User.Identity?.IsAuthenticated != true)
            {
                return string.Empty;
            }
            
            // Obtener el ID del usuario de las claims
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        // GET: Gerente/SolicitudesDescuento
        public IActionResult SolicitudesDescuento()
        {
            // Redirigir a la acción AutorizarDescuentos en el controlador de Ventas
            return RedirectToAction("AutorizarDescuentos", "Ventas");
        }
    }
} 