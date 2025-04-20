using Autos.Data;
using Autos.Models;
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

        public VentasController(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Ventas
        public async Task<IActionResult> Index()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Auto)
                .Include(v => v.Vendedor)
                .Include(v => v.Cliente)
                .ToListAsync();
            
            return View(ventas);
        }

        // GET: Ventas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.Ventas
                .Include(v => v.Auto)
                .Include(v => v.Vendedor)
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            
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
            var vendedorActual = await _userManager.GetUserAsync(User);
            if (vendedorActual == null)
            {
                return NotFound("No se pudo identificar al vendedor actual.");
            }

            // Pasar el ID y nombre del vendedor actual a la vista
            ViewBag.VendedorActual = new { Id = vendedorActual.Id, Nombre = vendedorActual.Nombre };
            
            // Cargar autos disponibles para venta
            ViewBag.Autos = _context.Autos
                .Where(a => a.Disponibilidad)
                .Select(a => new { Id = a.Id, Nombre = $"{a.Marca} {a.Modelo} ({a.Anio})" })
                .ToList();
            
            // Cargar clientes
            ViewBag.Clientes = _context.Users
                .Where(u => u.Rol == "Cliente")
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
                var cliente = await _userManager.FindByIdAsync(clienteId);
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
                var auto = await _context.Autos.FindAsync(autoId.Value);
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
                var auto = await _context.Autos.FindAsync(venta.AutoId);
                if (auto != null)
                {
                    auto.Disponibilidad = false;
                    _context.Update(auto);
                }
                
                _context.Add(venta);
                await _context.SaveChangesAsync();
                
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

            var auto = await _context.Autos.FindAsync(autoId);
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
            var auto = await _context.Autos.FindAsync(autoId);
            if (auto == null)
            {
                return NotFound();
            }

            var config = await _context.DescuentosConfig.FirstOrDefaultAsync();
            decimal maxDescuento = config?.PorcentajeMaximo ?? 10;
            
            // Obtener el vendedor actual
            var vendedorActual = await _userManager.GetUserAsync(User);
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
                
                _context.SolicitudesDescuento.Add(solicitud);
                await _context.SaveChangesAsync();
                
                TempData["Info"] = $"Solicitud de descuento del {porcentajeSolicitado}% enviada al gerente para su aprobación.";
                return RedirectToAction("Index", "Autos");
            }
        }

        // GET: Ventas/AutorizarDescuentos
        [Authorize(Roles = "Gerente,Administrador")]
        public async Task<IActionResult> AutorizarDescuentos()
        {
            // Por defecto, obtener todas las solicitudes pendientes
            var query = _context.SolicitudesDescuento
                .Include(s => s.Auto)
                .ThenInclude(a => a!.Sucursal)
                .Include(s => s.Vendedor)
                .Where(s => !s.Aprobada.HasValue); // Solo mostrar las pendientes
            
            // Si es gerente, solo mostrar solicitudes de su sucursal
            if (User.IsInRole("Gerente") && !User.IsInRole("Administrador"))
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario != null)
                {
                    // Obtener la sucursal del gerente
                    var sucursalGerente = await _context.Sucursales
                        .FirstOrDefaultAsync(s => s.GerenteId == usuario.Id);
                    
                    if (sucursalGerente != null)
                    {
                        // Filtrar solicitudes que pertenecen a autos de su sucursal
                        query = query.Where(s => s.Auto!.SucursalId == sucursalGerente.Id);
                    }
                }
            }
            
            var solicitudes = await query
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();
            
            return View(solicitudes);
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
            var solicitud = await _context.SolicitudesDescuento
                .Include(s => s.Auto)
                .FirstOrDefaultAsync(s => s.Id == solicitudId);
                
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
            var solicitud = await _context.SolicitudesDescuento
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (solicitud == null)
            {
                return NotFound();
            }

            solicitud.Aprobada = true;
            solicitud.FechaResolucion = DateTime.Now;
            solicitud.GerenteId = GetCurrentUserId();
            solicitud.ComentarioGerente = comentario;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AutorizarDescuentos));
        }
        
        // POST: Ventas/RechazarDescuento
        [Authorize(Roles = "Gerente")]
        [HttpPost]
        public async Task<IActionResult> RechazarDescuento(int id, string? comentario)
        {
            var solicitud = await _context.SolicitudesDescuento
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (solicitud == null)
            {
                return NotFound();
            }

            solicitud.Aprobada = false;
            solicitud.FechaResolucion = DateTime.Now;
            solicitud.GerenteId = GetCurrentUserId();
            solicitud.ComentarioGerente = comentario;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AutorizarDescuentos));
        }
    }
} 