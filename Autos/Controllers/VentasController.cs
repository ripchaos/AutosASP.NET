using Autos.Data;
using Autos.Models;
using Autos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System;

namespace Autos.Controllers
{
    [Authorize(Roles = "Administrador,Gerente,Vendedor")]
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IVentaService _ventaService;
        private readonly IAutoService _autoService;
        private readonly ISolicitudDescuentoService _solicitudDescuentoService;
        private readonly IUsuarioService _usuarioService;

        public VentasController(
            ApplicationDbContext context, 
            UserManager<Usuario> userManager,
            IVentaService ventaService,
            IAutoService autoService,
            ISolicitudDescuentoService solicitudDescuentoService,
            IUsuarioService usuarioService)
        {
            _context = context;
            _userManager = userManager;
            _ventaService = ventaService;
            _autoService = autoService;
            _solicitudDescuentoService = solicitudDescuentoService;
            _usuarioService = usuarioService;
        }

        // GET: Ventas
        public async Task<IActionResult> Index()
        {
            var ventas = await _ventaService.ObtenerVentasReciententesAsync(100); // Obtener las 100 ventas más recientes
            return View(ventas);
        }

        // GET: Ventas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _ventaService.ObtenerVentaPorIdAsync(id.Value);
            
            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        // GET: Ventas/Create
        public async Task<IActionResult> Create(string? clienteId = null, int? autoId = null)
        {
            // Obtener el vendedor actual
            var vendedorActual = await _usuarioService.ObtenerUsuarioActualAsync();
            if (vendedorActual == null)
            {
                return NotFound("No se pudo identificar al vendedor actual.");
            }

            // Pasar el ID y nombre del vendedor actual a la vista
            ViewBag.VendedorActual = new { Id = vendedorActual.Id, Nombre = vendedorActual.Nombre };
            
            // Cargar autos disponibles para venta
            var autosDisponibles = await _autoService.ObtenerAutosDisponiblesAsync();
            ViewBag.Autos = autosDisponibles
                .Select(a => new { Id = a.Id, Nombre = $"{a.Marca} {a.Modelo} ({a.Anio})" })
                .ToList();
            
            // Cargar clientes
            var clientes = await _usuarioService.ObtenerUsuariosPorRolAsync("Cliente");
            ViewBag.Clientes = clientes
                .Select(u => new { Id = u.Id, Nombre = u.Nombre })
                .ToList();

            // Si se proporcionaron parámetros, crear un modelo de Venta pre-poblado
            var venta = new Venta
            {
                VendedorId = vendedorActual.Id,
                ClienteId = clienteId ?? string.Empty,
                Fecha = DateTime.Now
            };

            if (clienteId != null)
            {
                ViewBag.ClienteSeleccionado = clienteId;
                
                // Obtener información adicional del cliente
                var cliente = await _usuarioService.ObtenerUsuarioPorIdAsync(clienteId);
                if (cliente != null)
                {
                    ViewBag.ClienteSeleccionadoNombre = cliente.Nombre;
                }
            }

            if (autoId.HasValue)
            {
                venta.AutoId = autoId.Value;
                ViewBag.AutoSeleccionado = autoId;

                // Obtener el precio del auto para pre-cargar
                var auto = await _autoService.ObtenerAutoPorIdAsync(autoId.Value);
                if (auto != null)
                {
                    venta.Monto = auto.Precio;
                }
            }
            
            return View(venta);
        }

        // POST: Ventas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AutoId,VendedorId,ClienteId,Monto,PorcentajeDescuento,Fecha")] Venta venta)
        {
            if (ModelState.IsValid)
            {
                // Actualizar disponibilidad del auto
                var auto = await _autoService.ObtenerAutoPorIdAsync(venta.AutoId);
                if (auto != null)
                {
                    auto.Disponibilidad = false;
                    auto.EstadoReserva = "Vendido";
                    await _autoService.GuardarAutoAsync(auto);
                }
                
                // Registrar la venta
                await _ventaService.RegistrarVentaAsync(venta);
                
                TempData["Success"] = "¡Venta registrada correctamente!";
                return RedirectToAction(nameof(Index));
            }
            
            return View(venta);
        }

        // GET: Ventas/SolicitarDescuento/5
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> SolicitarDescuento(int? autoId)
        {
            if (autoId == null)
            {
                return NotFound();
            }

            var auto = await _autoService.ObtenerAutoPorIdAsync(autoId.Value);
            if (auto == null)
            {
                return NotFound();
            }

            // Cargar la configuración de descuento
            var config = await _context.DescuentosConfig.FirstOrDefaultAsync();
            ViewBag.PorcentajeMaximo = config?.PorcentajeMaximo ?? 10; // Default 10% si no hay configuración
            
            return View(auto);
        }

        // POST: Ventas/SolicitarDescuento/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> SolicitarDescuento(int autoId, decimal porcentajeSolicitado, string justificacion)
        {
            var auto = await _autoService.ObtenerAutoPorIdAsync(autoId);
            if (auto == null)
            {
                return NotFound();
            }

            var config = await _context.DescuentosConfig.FirstOrDefaultAsync();
            decimal maxDescuento = config?.PorcentajeMaximo ?? 10;
            
            // Obtener el vendedor actual
            var vendedorActual = await _usuarioService.ObtenerUsuarioActualAsync();
            if (vendedorActual == null)
            {
                return NotFound("No se pudo identificar al vendedor actual.");
            }
            
            if (porcentajeSolicitado <= maxDescuento)
            {
                // Descuento dentro del límite permitido
                TempData["Success"] = $"Descuento del {porcentajeSolicitado}% aprobado automáticamente.";
                return RedirectToAction("Create", new { autoId = autoId, descuento = porcentajeSolicitado });
            }
            else
            {
                // Descuento excede el límite, crear solicitud para aprobación del gerente
                var solicitud = new SolicitudDescuento
                {
                    AutoId = autoId,
                    VendedorId = vendedorActual.Id,
                    PorcentajeSolicitado = porcentajeSolicitado,
                    Justificacion = justificacion,
                    FechaSolicitud = DateTime.Now
                };
                
                await _solicitudDescuentoService.CrearSolicitudAsync(solicitud);
                
                TempData["Info"] = $"Solicitud de descuento del {porcentajeSolicitado}% enviada al gerente para su aprobación.";
                return RedirectToAction("Index", "Autos");
            }
        }

        // GET: Ventas/AutorizarDescuentos
        [Authorize(Roles = "Gerente,Administrador")]
        public async Task<IActionResult> AutorizarDescuentos()
        {
            // Obtener todas las solicitudes pendientes
            var solicitudesPendientes = await _solicitudDescuentoService.ObtenerSolicitudesPendientesAsync();
            
            // Si es gerente, filtrar por sucursal
            if (User.IsInRole("Gerente") && !User.IsInRole("Administrador"))
            {
                var usuario = await _usuarioService.ObtenerUsuarioActualAsync();
                if (usuario != null)
                {
                    // Obtener la sucursal del gerente
                    var sucursalGerente = await _context.Sucursales
                        .FirstOrDefaultAsync(s => s.GerenteId == usuario.Id);
                    
                    if (sucursalGerente != null)
                    {
                        // Filtrar solicitudes que pertenecen a autos de su sucursal
                        solicitudesPendientes = solicitudesPendientes
                            .Where(s => s.Auto != null && s.Auto.SucursalId == sucursalGerente.Id)
                            .ToList();
                    }
                }
            }
            
            return View(solicitudesPendientes);
        }
        
        // Obtener el ID del usuario actual
        private string GetCurrentUserId()
        {
            return _userManager.GetUserId(User) ?? string.Empty;
        }
        
        // Método para verificar si el gerente puede aprobar la solicitud
        private async Task<bool> PuedeAprobarSolicitud(int solicitudId)
        {
            if (User.IsInRole("Administrador"))
                return true;
                
            var usuarioId = GetCurrentUserId();
            
            // Obtener la sucursal del gerente
            var sucursalGerente = await _context.Sucursales
                .FirstOrDefaultAsync(s => s.GerenteId == usuarioId);
                
            if (sucursalGerente == null)
                return false;
                
            // Verificar si la solicitud pertenece a un auto de su sucursal
            var solicitud = await _solicitudDescuentoService.ObtenerSolicitudPorIdAsync(solicitudId);
            
            if (solicitud?.Auto == null)
                return false;
                
            // Si el auto no tiene sucursal asignada, no permitir la aprobación
            if (solicitud.Auto.SucursalId <= 0)
                return false;
                
            return solicitud.Auto.SucursalId == sucursalGerente.Id;
        }

        // POST: Ventas/AprobarDescuento
        [Authorize(Roles = "Gerente")]
        [HttpPost]
        public async Task<IActionResult> AprobarDescuento(int id, string? comentario)
        {
            // Verificar que el gerente pueda aprobar esta solicitud
            if (!await PuedeAprobarSolicitud(id))
            {
                TempData["Error"] = "No tienes permiso para aprobar esta solicitud de descuento.";
                return RedirectToAction(nameof(AutorizarDescuentos));
            }

            // Obtener ID del gerente actual
            var gerenteId = _userManager.GetUserId(User);
            
            // Comprobar si gerenteId es nulo y proporcionar un valor por defecto
            if (gerenteId == null)
            {
                TempData["Error"] = "No se pudo identificar al gerente. Por favor, intente nuevamente.";
                return RedirectToAction(nameof(AutorizarDescuentos));
            }
            
            // Aprobar la solicitud
            await _solicitudDescuentoService.AprobarSolicitudAsync(id, gerenteId);
            
            TempData["Success"] = "Solicitud de descuento aprobada correctamente.";
            return RedirectToAction(nameof(AutorizarDescuentos));
        }
        
        // POST: Ventas/RechazarDescuento
        [Authorize(Roles = "Gerente")]
        [HttpPost]
        public async Task<IActionResult> RechazarDescuento(int id, string? comentario)
        {
            // Verificar que el gerente pueda rechazar esta solicitud
            if (!await PuedeAprobarSolicitud(id))
            {
                TempData["Error"] = "No tienes permiso para rechazar esta solicitud de descuento.";
                return RedirectToAction(nameof(AutorizarDescuentos));
            }

            // Obtener ID del gerente actual
            var gerenteId = _userManager.GetUserId(User);
            
            // Comprobar si gerenteId es nulo y proporcionar un valor por defecto
            if (gerenteId == null)
            {
                TempData["Error"] = "No se pudo identificar al gerente. Por favor, intente nuevamente.";
                return RedirectToAction(nameof(AutorizarDescuentos));
            }
            
            // Rechazar la solicitud
            await _solicitudDescuentoService.RechazarSolicitudAsync(id, gerenteId, comentario ?? "");
            
            TempData["Success"] = "Solicitud de descuento rechazada.";
            return RedirectToAction(nameof(AutorizarDescuentos));
        }
    }
} 