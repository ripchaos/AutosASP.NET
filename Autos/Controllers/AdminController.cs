using Autos.Data;
using Autos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Autos.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // 🔹 Vista para el Panel de Administración
        public IActionResult Dashboard()
        {
            return View();
        }

        // 🔹 Vista para Crear Usuarios
        public IActionResult CrearUsuario()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearUsuario(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var rolesPermitidos = new[] { "Recepcionista", "Vendedor", "Gerente" };

                if (!rolesPermitidos.Contains(model.Rol))
                {
                    ModelState.AddModelError(string.Empty, "El rol seleccionado no es válido.");
                    return View(model);
                }

                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "El correo ya está registrado.");
                    return View(model);
                }

                var user = new Usuario
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Nombre = model.Nombre,
                    Rol = model.Rol
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync(model.Rol))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(model.Rol));
                    }

                    await _userManager.AddToRoleAsync(user, model.Rol);

                    TempData["Success"] = $"Usuario {model.Email} creado con éxito y asignado al rol {model.Rol}.";
                    return RedirectToAction("Usuarios");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // 🔹 Listar Usuarios
        public IActionResult Usuarios()
        {
            var usuarios = _userManager.Users.ToList();
            return View(usuarios);
        }

        // 🔹 Asignar Roles a Usuarios
        [HttpPost]
        public async Task<IActionResult> AsignarRol(string userId, string nuevoRol)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(nuevoRol))
            {
                TempData["Error"] = "Debe seleccionar un usuario y un rol válido.";
                return RedirectToAction("Usuarios");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("Usuarios");
            }

            var rolesPermitidos = new[] { "Recepcionista", "Vendedor", "Gerente", "Cliente" };

            if (!rolesPermitidos.Contains(nuevoRol))
            {
                TempData["Error"] = "El rol seleccionado no es válido.";
                return RedirectToAction("Usuarios");
            }

            var rolesActuales = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, rolesActuales);
            await _userManager.AddToRoleAsync(user, nuevoRol);

            user.Rol = nuevoRol;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"Rol actualizado a {nuevoRol} correctamente.";
            return RedirectToAction("Usuarios");
        }

        // 🔹 Vista para Configurar Margen de Descuento
        public async Task<IActionResult> ConfigurarDescuento()
        {
            var config = await _context.DescuentosConfig.FirstOrDefaultAsync();
            if (config == null) config = new DescuentoConfig();
            return View(config);
        }

        [HttpPost]
        public async Task<IActionResult> ConfigurarDescuento(DescuentoConfig config)
        {
            if (!ModelState.IsValid)
                return View(config);

            var existente = await _context.DescuentosConfig.FirstOrDefaultAsync();
            if (existente == null)
            {
                _context.DescuentosConfig.Add(config);
            }
            else
            {
                existente.PorcentajeMaximo = config.PorcentajeMaximo;
                _context.DescuentosConfig.Update(existente);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Margen de descuento actualizado correctamente.";
            return RedirectToAction("Dashboard");


        }
    }
}
    