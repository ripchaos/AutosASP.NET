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

namespace Autos.Controllers
{
    public class AutosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public AutosController(
            ApplicationDbContext context,
            UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Autos
        public async Task<IActionResult> Index(string marca, string modelo, int? anioMin, int? anioMax, 
            decimal? precioMin, decimal? precioMax, string color, string categoria, int? sucursalId, 
            bool mostrarNoDisponibles = false)
        {
            // Consulta base
            var autos = _context.Autos
                .Include(a => a.Sucursal)
                .AsQueryable();

            // Por defecto, mostrar solo autos disponibles a menos que se indique lo contrario
            if (!mostrarNoDisponibles)
            {
                autos = autos.Where(a => a.Disponibilidad && a.EstadoReserva == "Disponible");
            }

            // Aplicar filtros
            if (!string.IsNullOrEmpty(marca))
            {
                autos = autos.Where(a => a.Marca.Contains(marca));
            }

            if (!string.IsNullOrEmpty(modelo))
            {
                autos = autos.Where(a => a.Modelo.Contains(modelo));
            }

            if (anioMin.HasValue)
            {
                autos = autos.Where(a => a.Anio >= anioMin.Value);
            }

            if (anioMax.HasValue)
            {
                autos = autos.Where(a => a.Anio <= anioMax.Value);
            }

            if (precioMin.HasValue)
            {
                autos = autos.Where(a => a.Precio >= precioMin.Value);
            }

            if (precioMax.HasValue)
            {
                autos = autos.Where(a => a.Precio <= precioMax.Value);
            }

            if (!string.IsNullOrEmpty(color))
            {
                autos = autos.Where(a => a.Color == color);
            }

            if (!string.IsNullOrEmpty(categoria))
            {
                autos = autos.Where(a => a.Categoria == categoria);
            }

            if (sucursalId.HasValue)
            {
                autos = autos.Where(a => a.SucursalId == sucursalId.Value);
            }

            // Cargar datos para los filtros
            ViewBag.Marcas = await _context.Autos.Select(a => a.Marca).Distinct().OrderBy(m => m).ToListAsync();
            ViewBag.Colores = await _context.Autos.Select(a => a.Color).Distinct().OrderBy(c => c).ToListAsync();
            ViewBag.Categorias = await _context.Autos.Select(a => a.Categoria).Distinct().OrderBy(c => c).ToListAsync();
            ViewBag.Sucursales = new SelectList(await _context.Sucursales.Where(s => s.Activa).ToListAsync(), "Id", "Nombre");

            // Verificar el rol del usuario para mostrar acciones específicas
            if (User.Identity.IsAuthenticated)
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario != null)
                {
                    ViewBag.UserRole = usuario.Rol;
                    
                    // Si es recepcionista, obtener su sucursal
                    if (usuario.Rol == "Recepcionista")
                    {
                        var sucursal = await _context.UsuariosSucursales
                            .Include(us => us.Sucursal)
                            .FirstOrDefaultAsync(us => us.UsuarioId == usuario.Id && us.Activo);
                        ViewBag.SucursalId = sucursal?.SucursalId;
                    }
                }
            }

            return View(await autos.ToListAsync());
        }

        // GET: Autos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auto = await _context.Autos
                .Include(a => a.Sucursal)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (auto == null)
            {
                return NotFound();
            }

            // Verificar el rol del usuario para mostrar acciones específicas
            if (User.Identity.IsAuthenticated)
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario != null)
                {
                    ViewBag.UserRole = usuario.Rol;
                }
            }

            return View(auto);
        }

        // GET: Autos/Create
        [Authorize(Roles = "Administrador,Gerente")]
        public IActionResult Create()
        {
            ViewData["SucursalId"] = new SelectList(_context.Sucursales.Where(s => s.Activa), "Id", "Nombre");
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
                _context.Add(auto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SucursalId"] = new SelectList(_context.Sucursales.Where(s => s.Activa), "Id", "Nombre", auto.SucursalId);
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

            var auto = await _context.Autos.FindAsync(id);
            if (auto == null)
            {
                return NotFound();
            }
            ViewData["SucursalId"] = new SelectList(_context.Sucursales.Where(s => s.Activa), "Id", "Nombre", auto.SucursalId);
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
                    _context.Update(auto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AutoExists(auto.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SucursalId"] = new SelectList(_context.Sucursales.Where(s => s.Activa), "Id", "Nombre", auto.SucursalId);
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

            var auto = await _context.Autos
                .Include(a => a.Sucursal)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (auto == null)
            {
                return NotFound();
            }

            return View(auto);
        }

        // POST: Autos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var auto = await _context.Autos.FindAsync(id);
            if (auto != null)
            {
                _context.Autos.Remove(auto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AutoExists(int id)
        {
            return _context.Autos.Any(e => e.Id == id);
        }

        // GET: Autos/Reservar/5
        [Authorize(Roles = "Recepcionista,Vendedor")]
        public async Task<IActionResult> Reservar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auto = await _context.Autos
                .Include(a => a.Sucursal)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (auto == null)
            {
                return NotFound();
            }

            // Verificar si el auto pertenece a la sucursal del usuario
            var usuario = await _userManager.GetUserAsync(User);
            var sucursal = await _context.UsuariosSucursales
                .FirstOrDefaultAsync(us => us.UsuarioId == usuario.Id && us.Activo);

            if (sucursal == null || sucursal.SucursalId != auto.SucursalId)
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
            var auto = await _context.Autos
                .Include(a => a.Sucursal)
                .FirstOrDefaultAsync(m => m.Id == AutoId);

            if (auto == null)
            {
                TempData["Error"] = "Auto no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar si el auto sigue disponible
            if (!auto.Disponibilidad || auto.EstadoReserva != "Disponible")
            {
                TempData["Error"] = "El auto ya no está disponible para reserva.";
                return RedirectToAction(nameof(Index));
            }

            // Crear la reserva
            var usuario = await _userManager.GetUserAsync(User);
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

            _context.Reservas.Add(reserva);

            // Actualizar el estado del auto
            auto.EstadoReserva = "Reservado";
            auto.FechaFinReserva = reserva.FechaExpiracion;
            _context.Autos.Update(auto);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Reserva creada con éxito.";
            return RedirectToAction(nameof(Index));
        }
    }
}
