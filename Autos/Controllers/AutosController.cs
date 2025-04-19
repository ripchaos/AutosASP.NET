using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Autos.Data;
using Autos.Models;

namespace Autos.Controllers
{
    public class AutosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AutosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Autos
        public async Task<IActionResult> Index(string marca, string modelo, int? anioMin, int? anioMax, decimal? precioMin, decimal? precioMax, string color, string categoria, int? sucursalId, bool mostrarNoDisponibles = false)
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
                autos = autos.Where(a => a.Color.Contains(color));
            }

            if (!string.IsNullOrEmpty(categoria))
            {
                autos = autos.Where(a => a.Categoria.Contains(categoria));
            }

            if (sucursalId.HasValue)
            {
                autos = autos.Where(a => a.SucursalId == sucursalId.Value);
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

            return View(auto);
        }

        // GET: Autos/Create
        public IActionResult Create()
        {
            ViewData["SucursalId"] = new SelectList(_context.Sucursales, "Id", "Direccion");
            return View();
        }

        // POST: Autos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Marca,Modelo,Anio,Precio,Disponibilidad,SucursalId")] Auto auto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(auto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SucursalId"] = new SelectList(_context.Sucursales, "Id", "Direccion", auto.SucursalId);
            return View(auto);
        }

        // GET: Autos/Edit/5
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
            ViewData["SucursalId"] = new SelectList(_context.Sucursales, "Id", "Direccion", auto.SucursalId);
            return View(auto);
        }

        // POST: Autos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Marca,Modelo,Anio,Precio,Disponibilidad,SucursalId")] Auto auto)
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
            ViewData["SucursalId"] = new SelectList(_context.Sucursales, "Id", "Direccion", auto.SucursalId);
            return View(auto);
        }

        // GET: Autos/Delete/5
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var auto = await _context.Autos.FindAsync(id);
            if (auto != null)
            {
                _context.Autos.Remove(auto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AutoExists(int id)
        {
            return _context.Autos.Any(e => e.Id == id);
        }
    }
}
