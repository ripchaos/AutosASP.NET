using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Autos.Data;
using Autos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Autos.Services.Interfaces;

namespace Autos.Controllers
{
    public class AutosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IAutoService _autoService;
        private readonly ISucursalService _sucursalService;
        private readonly IUsuarioService _usuarioService;
        private readonly IReservaService _reservaService;

        public AutosController(
            ApplicationDbContext context,
            UserManager<Usuario> userManager,
            IAutoService autoService,
            ISucursalService sucursalService,
            IUsuarioService usuarioService,
            IReservaService reservaService)
        {
            _context = context;
            _userManager = userManager;
            _autoService = autoService;
            _sucursalService = sucursalService;
            _usuarioService = usuarioService;
            _reservaService = reservaService;
        }

        // GET: Autos
        public async Task<IActionResult> Index(string marca, string modelo, int? anioMin, int? anioMax, 
            decimal? precioMin, decimal? precioMax, string color, string categoria, int? sucursalId, 
            bool mostrarNoDisponibles = false)
        {
            // Obtener todos los autos
            var todosLosAutos = await _autoService.ObtenerTodosLosAutosAsync();
            var listaAutos = todosLosAutos.ToList();

            // Filtrar por disponibilidad (a menos que se indique lo contrario)
            if (!mostrarNoDisponibles)
            {
                listaAutos = listaAutos.Where(a => a.Disponibilidad && a.EstadoReserva == "Disponible").ToList();
            }

            // Aplicar filtros
            if (!string.IsNullOrEmpty(marca))
            {
                listaAutos = listaAutos.Where(a => a.Marca.Contains(marca, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(modelo))
            {
                listaAutos = listaAutos.Where(a => a.Modelo.Contains(modelo, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (anioMin.HasValue)
            {
                listaAutos = listaAutos.Where(a => a.Anio >= anioMin.Value).ToList();
            }

            if (anioMax.HasValue)
            {
                listaAutos = listaAutos.Where(a => a.Anio <= anioMax.Value).ToList();
            }

            if (precioMin.HasValue)
            {
                listaAutos = listaAutos.Where(a => a.Precio >= precioMin.Value).ToList();
            }

            if (precioMax.HasValue)
            {
                listaAutos = listaAutos.Where(a => a.Precio <= precioMax.Value).ToList();
            }

            if (!string.IsNullOrEmpty(color))
            {
                listaAutos = listaAutos.Where(a => a.Color == color).ToList();
            }

            if (!string.IsNullOrEmpty(categoria))
            {
                listaAutos = listaAutos.Where(a => a.Categoria == categoria).ToList();
            }

            if (sucursalId.HasValue)
            {
                listaAutos = listaAutos.Where(a => a.SucursalId == sucursalId.Value).ToList();
            }

            // Cargar datos para los filtros
            ViewBag.Marcas = todosLosAutos.Select(a => a.Marca).Distinct().OrderBy(m => m).ToList();
            ViewBag.Colores = todosLosAutos.Select(a => a.Color).Distinct().OrderBy(c => c).ToList();
            ViewBag.Categorias = todosLosAutos.Select(a => a.Categoria).Distinct().OrderBy(c => c).ToList();
            
            var sucursales = await _sucursalService.ObtenerTodasLasSucursalesAsync();
            ViewBag.Sucursales = new SelectList(sucursales.Where(s => s.Activa), "Id", "Nombre");

            // Verificar el rol del usuario para mostrar acciones específicas
            if (User.Identity?.IsAuthenticated == true)
            {
                var usuario = await _usuarioService.ObtenerUsuarioActualAsync();
                if (usuario != null)
                {
                    var roles = await _usuarioService.ObtenerRolesDeUsuarioAsync(usuario);
                    ViewBag.UserRole = roles.FirstOrDefault() ?? usuario.Rol;
                    
                    // Si es recepcionista, obtener su sucursal
                    if (roles.Contains("Recepcionista"))
                    {
                        var asignacion = await _sucursalService.ObtenerAsignacionUsuarioSucursalAsync(usuario.Id);
                        ViewBag.SucursalId = asignacion?.SucursalId;
                    }
                }
            }

            return View(listaAutos);
        }

        // GET: Autos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auto = await _autoService.ObtenerAutoPorIdAsync(id.Value);

            if (auto == null)
            {
                return NotFound();
            }

            // Cargar la sucursal si no está incluida
            if (auto.Sucursal == null && auto.SucursalId > 0)
            {
                auto.Sucursal = await _sucursalService.ObtenerSucursalPorIdAsync(auto.SucursalId);
            }

            // Verificar el rol del usuario para mostrar acciones específicas
            if (User.Identity?.IsAuthenticated == true)
            {
                var usuario = await _usuarioService.ObtenerUsuarioActualAsync();
                if (usuario != null)
                {
                    var roles = await _usuarioService.ObtenerRolesDeUsuarioAsync(usuario);
                    ViewBag.UserRole = roles.FirstOrDefault() ?? usuario.Rol;
                }
            }

            return View(auto);
        }

        // GET: Autos/Create
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> Create()
        {
            var sucursales = await _sucursalService.ObtenerTodasLasSucursalesAsync();
            ViewData["SucursalId"] = new SelectList(sucursales.Where(s => s.Activa), "Id", "Nombre");
            return View();
        }

        // POST: Autos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> Create(Auto auto)
        {
            if (ModelState.IsValid)
            {
                var exito = await _autoService.GuardarAutoAsync(auto);
                if (exito)
                {
                    return RedirectToAction(nameof(Index));
                }
                // Si no se pudo guardar, mostramos un error
                ModelState.AddModelError(string.Empty, "No se pudo crear el auto. Intente nuevamente.");
            }
            
            var sucursales = await _sucursalService.ObtenerTodasLasSucursalesAsync();
            ViewData["SucursalId"] = new SelectList(sucursales.Where(s => s.Activa), "Id", "Nombre", auto.SucursalId);
            return View(auto);
        }

        // GET: Autos/Edit/5
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auto = await _autoService.ObtenerAutoPorIdAsync(id.Value);
            if (auto == null)
            {
                return NotFound();
            }
            
            var sucursales = await _sucursalService.ObtenerTodasLasSucursalesAsync();
            ViewData["SucursalId"] = new SelectList(sucursales.Where(s => s.Activa), "Id", "Nombre", auto.SucursalId);
            return View(auto);
        }

        // POST: Autos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> Edit(int id, Auto auto)
        {
            if (id != auto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var exito = await _autoService.GuardarAutoAsync(auto);
                    if (exito)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    // Si no se pudo actualizar, mostramos un error
                    ModelState.AddModelError(string.Empty, "No se pudo actualizar el auto. Intente nuevamente.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    var existe = await _autoService.ObtenerAutoPorIdAsync(auto.Id) != null;
                    if (!existe)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            var sucursales = await _sucursalService.ObtenerTodasLasSucursalesAsync();
            ViewData["SucursalId"] = new SelectList(sucursales.Where(s => s.Activa), "Id", "Nombre", auto.SucursalId);
            return View(auto);
        }

        // GET: Autos/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auto = await _autoService.ObtenerAutoPorIdAsync(id.Value);
            if (auto == null)
            {
                return NotFound();
            }
            
            // Cargar la sucursal si no está incluida
            if (auto.Sucursal == null && auto.SucursalId > 0)
            {
                auto.Sucursal = await _sucursalService.ObtenerSucursalPorIdAsync(auto.SucursalId);
            }

            return View(auto);
        }

        // POST: Autos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var exito = await _autoService.EliminarAutoAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Autos/Reservar/5
        [Authorize(Roles = "Recepcionista,Vendedor")]
        public async Task<IActionResult> Reservar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auto = await _autoService.ObtenerAutoPorIdAsync(id.Value);
            if (auto == null)
            {
                return NotFound();
            }
            
            // Cargar la sucursal si no está incluida
            if (auto.Sucursal == null && auto.SucursalId > 0)
            {
                auto.Sucursal = await _sucursalService.ObtenerSucursalPorIdAsync(auto.SucursalId);
            }

            // Verificar si el auto pertenece a la sucursal del usuario
            var usuario = await _usuarioService.ObtenerUsuarioActualAsync();
            if (usuario == null)
            {
                TempData["Error"] = "No se pudo identificar el usuario actual.";
                return RedirectToAction(nameof(Index));
            }
            
            var asignacion = await _sucursalService.ObtenerAsignacionUsuarioSucursalAsync(usuario.Id);

            if (asignacion == null || asignacion.SucursalId != auto.SucursalId)
            {
                TempData["Error"] = "No tienes permiso para reservar autos de otras sucursales.";
                return RedirectToAction(nameof(Index));
            }

            return View(auto);
        }

        // POST: Autos/ProcesarReserva
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Recepcionista,Vendedor")]
        public async Task<IActionResult> ProcesarReserva(int AutoId, int DuracionReserva, string Comentarios)
        {
            var auto = await _autoService.ObtenerAutoPorIdAsync(AutoId);
            if (auto == null)
            {
                TempData["Error"] = "Auto no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar si el auto sigue disponible
            var disponible = await _autoService.VerificarDisponibilidadAsync(AutoId);
            if (!disponible || auto.EstadoReserva != "Disponible")
            {
                TempData["Error"] = "El auto ya no está disponible para reserva.";
                return RedirectToAction(nameof(Index));
            }

            // Crear la reserva
            var usuario = await _usuarioService.ObtenerUsuarioActualAsync();
            if (usuario == null)
            {
                TempData["Error"] = "No se pudo identificar el usuario actual.";
                return RedirectToAction(nameof(Index));
            }
            
            var reserva = new Reserva
            {
                UsuarioId = usuario.Id,
                AutoId = AutoId,
                FechaReserva = DateTime.Now,
                FechaExpiracion = DateTime.Now.AddDays(DuracionReserva),
                Estado = "Activa",
                VendedorId = usuario.Id,
                Comentarios = Comentarios
            };

            var exito = await _reservaService.CrearReservaAsync(reserva);
            if (exito)
            {
                TempData["Success"] = "Reserva creada con éxito.";
            }
            else
            {
                TempData["Error"] = "No se pudo crear la reserva. Intente nuevamente.";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}
