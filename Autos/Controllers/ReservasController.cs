using Autos.Data;
using Autos.Models;
using Autos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Autos.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;

        public ReservasController(
            ApplicationDbContext context, 
            UserManager<Usuario> userManager,
            IEmailService emailService,
            IEmailTemplateService emailTemplateService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
        }

        // GET: Reservas
        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return NotFound();
            }

            // Si es cliente, mostrar solo sus reservas
            if (usuario.Rol == "Cliente")
            {
                var reservasCliente = await _context.Reservas
                    .Include(r => r.Auto)
                    .Where(r => r.UsuarioId == usuario.Id)
                    .OrderByDescending(r => r.FechaReserva)
                    .ToListAsync();
                return View(reservasCliente);
            }

            // Si es vendedor, mostrar las reservas de sus clientes asignados
            if (usuario.Rol == "Vendedor")
            {
                var clientesAsignados = await _context.Users
                    .Where(u => u.VendedorAsignadoId == usuario.Id)
                    .Select(u => u.Id)
                    .ToListAsync();

                var reservasClientes = await _context.Reservas
                    .Include(r => r.Auto)
                    .Include(r => r.Usuario)
                    .Where(r => clientesAsignados.Contains(r.UsuarioId))
                    .OrderByDescending(r => r.FechaReserva)
                    .ToListAsync();
                return View(reservasClientes);
            }

            // Si es gerente o administrador, mostrar todas las reservas
            var reservas = await _context.Reservas
                .Include(r => r.Auto)
                .Include(r => r.Usuario)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();
            return View(reservas);
        }

        // GET: Reservas/Create/5 (5 es el ID del auto)
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Create(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auto = await _context.Autos.FindAsync(id);
            if (auto == null)
            {
                return NotFound();
            }

            // Verificar que el auto esté disponible
            if (!auto.Disponibilidad || auto.EstadoReserva != "Disponible")
            {
                // Si el auto no está disponible, preguntar si quiere unirse a la lista de espera
                TempData["Error"] = "Este auto no está disponible actualmente.";
                TempData["AutoId"] = auto.Id;
                TempData["AutoNombre"] = $"{auto.Marca} {auto.Modelo} ({auto.Anio})";
                if (auto.FechaFinReserva.HasValue)
                {
                    TempData["FechaDisponible"] = auto.FechaFinReserva.Value.AddDays(1).ToString("dd/MM/yyyy");
                }
                return RedirectToAction("ListaEspera", new { autoId = id });
            }

            // Auto disponible, mostrar vista de confirmación de reserva
            return View(auto);
        }

        // POST: Reservas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Create(int autoId)
        {
            var auto = await _context.Autos.FindAsync(autoId);
            if (auto == null)
            {
                return NotFound();
            }

            // Verificar nuevamente que el auto esté disponible (por si cambió entre la carga de la vista y el envío del formulario)
            if (!auto.Disponibilidad || auto.EstadoReserva != "Disponible")
            {
                TempData["Error"] = "Este auto ya no está disponible.";
                return RedirectToAction("Index", "Autos");
            }

            var usuarioActual = await _userManager.GetUserAsync(User);
            if (usuarioActual == null)
            {
                return NotFound();
            }

            // Crear la reserva
            var reserva = new Reserva
            {
                AutoId = autoId,
                UsuarioId = usuarioActual.Id,
                FechaReserva = DateTime.Now,
            };

            // Actualizar el estado del auto
            auto.EstadoReserva = "Reservado";
            auto.FechaFinReserva = DateTime.Now.AddDays(7); // Reserva por 7 días por defecto

            // Guardar los cambios
            _context.Reservas.Add(reserva);
            _context.Autos.Update(auto);
            await _context.SaveChangesAsync();

            // Enviar correo de confirmación si está habilitado
            var configuracion = await _context.ConfiguracionCorreo.FirstOrDefaultAsync();
            if (configuracion != null && configuracion.NotificarReservas && !string.IsNullOrEmpty(usuarioActual.Email))
            {
                var contenidoCorreo = _emailTemplateService.GenerarPlantillaConfirmacionReserva(reserva);
                await _emailService.EnviarCorreoAsync(usuarioActual.Email, "Confirmación de Reserva", contenidoCorreo);
            }

            TempData["Success"] = "¡Auto reservado correctamente! La reserva estará disponible por 7 días.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Reservas/Cancel/5
        [Authorize(Roles = "Cliente,Vendedor,Gerente,Administrador")]
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Auto)
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
            {
                return NotFound();
            }

            // Verificar permisos (solo el propio cliente, o un admin/gerente puede cancelar)
            var usuarioActual = await _userManager.GetUserAsync(User);
            if (usuarioActual == null)
            {
                return NotFound();
            }

            bool tienePermiso = usuarioActual.Id == reserva.UsuarioId || 
                               User.IsInRole("Administrador") || 
                               User.IsInRole("Gerente");

            if (!tienePermiso)
            {
                return Forbid();
            }

            return View(reserva);
        }

        // POST: Reservas/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente,Vendedor,Gerente,Administrador")]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Auto)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
            {
                return NotFound();
            }

            // Verificar permisos nuevamente
            var usuarioActual = await _userManager.GetUserAsync(User);
            if (usuarioActual == null)
            {
                return NotFound();
            }

            bool tienePermiso = usuarioActual.Id == reserva.UsuarioId || 
                               User.IsInRole("Administrador") || 
                               User.IsInRole("Gerente");

            if (!tienePermiso)
            {
                return Forbid();
            }

            // Actualizar el estado del auto
            var auto = reserva.Auto;
            if (auto != null)
            {
                auto.EstadoReserva = "Disponible";
                auto.FechaFinReserva = null;
                _context.Autos.Update(auto);

                // Verificar si hay interesados en lista de espera
                await NotificarListaEspera(auto.Id);
            }

            // Eliminar la reserva
            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Reserva cancelada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Reservas/ListaEspera/5
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> ListaEspera(int? autoId)
        {
            if (autoId == null)
            {
                return NotFound();
            }

            var auto = await _context.Autos
                .Include(a => a.Sucursal)
                .FirstOrDefaultAsync(a => a.Id == autoId);

            if (auto == null)
            {
                return NotFound();
            }

            // Verificar si el usuario ya está en la lista de espera
            var usuarioActual = await _userManager.GetUserAsync(User);
            if (usuarioActual == null)
            {
                return NotFound();
            }

            var yaEnListaEspera = await _context.ListaEspera
                .AnyAsync(l => l.AutoId == autoId && l.UsuarioId == usuarioActual.Id);

            if (yaEnListaEspera)
            {
                TempData["Info"] = "Ya estás en la lista de espera para este auto.";
                return RedirectToAction("Details", "Autos", new { id = autoId });
            }

            ViewBag.Auto = auto;
            return View();
        }

        // POST: Reservas/ListaEspera
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> ListaEspera(int autoId, string comentarios)
        {
            var auto = await _context.Autos.FindAsync(autoId);
            if (auto == null)
            {
                return NotFound();
            }

            var usuarioActual = await _userManager.GetUserAsync(User);
            if (usuarioActual == null)
            {
                return NotFound();
            }

            // Verificar si ya está en la lista de espera
            var yaEnListaEspera = await _context.ListaEspera
                .AnyAsync(l => l.AutoId == autoId && l.UsuarioId == usuarioActual.Id);

            if (yaEnListaEspera)
            {
                TempData["Info"] = "Ya estás en la lista de espera para este auto.";
                return RedirectToAction("Details", "Autos", new { id = autoId });
            }

            // Crear la entrada en la lista de espera
            var listaEspera = new ListaEspera
            {
                AutoId = autoId,
                UsuarioId = usuarioActual.Id,
                FechaSolicitud = DateTime.Now,
                Comentarios = comentarios
            };

            _context.ListaEspera.Add(listaEspera);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Te has agregado a la lista de espera. Serás notificado cuando el auto esté disponible.";
            return RedirectToAction("Details", "Autos", new { id = autoId });
        }

        // GET: Reservas/MisIntereses
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MisIntereses()
        {
            var usuarioActual = await _userManager.GetUserAsync(User);
            if (usuarioActual == null)
            {
                return NotFound();
            }

            var listaEspera = await _context.ListaEspera
                .Include(l => l.Auto)
                .Where(l => l.UsuarioId == usuarioActual.Id)
                .OrderByDescending(l => l.FechaSolicitud)
                .ToListAsync();

            return View(listaEspera);
        }

        // POST: Reservas/CancelarInteres/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> CancelarInteres(int id)
        {
            var interes = await _context.ListaEspera.FindAsync(id);
            if (interes == null)
            {
                return NotFound();
            }

            var usuarioActual = await _userManager.GetUserAsync(User);
            if (usuarioActual == null || usuarioActual.Id != interes.UsuarioId)
            {
                return Forbid();
            }

            _context.ListaEspera.Remove(interes);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Interés cancelado correctamente.";
            return RedirectToAction(nameof(MisIntereses));
        }

        // Método para notificar a los interesados en la lista de espera
        private async Task NotificarListaEspera(int autoId)
        {
            // Obtener el auto
            var auto = await _context.Autos.FindAsync(autoId);
            if (auto == null)
            {
                return;
            }

            // Verificar si hay configuración de correo
            var configuracion = await _context.ConfiguracionCorreo.FirstOrDefaultAsync();
            if (configuracion == null || !configuracion.NotificarReservas)
            {
                return;
            }

            // Obtener interesados en orden de solicitud (primero el más antiguo)
            var interesados = await _context.ListaEspera
                .Include(l => l.Usuario)
                .Where(l => l.AutoId == autoId && !l.Notificado)
                .OrderBy(l => l.FechaSolicitud)
                .ToListAsync();

            foreach (var interes in interesados)
            {
                if (interes.Usuario != null && !string.IsNullOrEmpty(interes.Usuario.Email))
                {
                    // Preparar el mensaje
                    string contenido = $@"
                        <h2>¡El auto que te interesa está disponible!</h2>
                        <p>Estimado/a {interes.Usuario.Nombre},</p>
                        <p>Nos complace informarte que el auto {auto.Marca} {auto.Modelo} ({auto.Anio}) ya está disponible.</p>
                        <p>Te recomendamos realizar la reserva lo antes posible, ya que otros clientes también pueden estar interesados.</p>
                        <p><a href='{Url.Action("Create", "Reservas", new { id = auto.Id }, Request.Scheme)}'>Haz clic aquí para reservar ahora</a></p>
                        <p>¡Gracias por tu interés en nuestros vehículos!</p>
                    ";

                    // Enviar notificación
                    await _emailService.EnviarCorreoAsync(interes.Usuario.Email, "¡Auto disponible para reserva!", contenido);

                    // Actualizar registro
                    interes.Notificado = true;
                    interes.FechaNotificacion = DateTime.Now;
                    _context.ListaEspera.Update(interes);
                }
            }

            // Guardar los cambios
            await _context.SaveChangesAsync();
        }
    }
} 