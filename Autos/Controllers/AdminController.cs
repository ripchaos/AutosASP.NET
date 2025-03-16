using Autos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Autos.Controllers
{
    [Authorize(Roles = "Administrador")] // Solo los administradores pueden acceder
    public class AdminController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 🔹 Vista para Crear Usuarios
        public IActionResult CrearUsuario()
        {
            return View();
        }

        // 🔹 Método para Crear Usuarios con el Rol seleccionado
        [HttpPost]
        public async Task<IActionResult> CrearUsuario(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 📌 Lista de roles permitidos para asignar (excluye "Administrador")
                var rolesPermitidos = new[] { "Recepcionista", "Vendedor", "Gerente" };

                if (!rolesPermitidos.Contains(model.Rol))
                {
                    ModelState.AddModelError(string.Empty, "El rol seleccionado no es válido.");
                    return View(model);
                }

                // 📌 Verificar si el usuario ya existe
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "El correo ya está registrado.");
                    return View(model);
                }

                // 📌 Crear el nuevo usuario
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
                    // 📌 Verificar si el rol existe, si no, crearlo
                    if (!await _roleManager.RoleExistsAsync(model.Rol))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(model.Rol));
                    }

                    // 📌 Asignar el rol al usuario
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
    }
}

