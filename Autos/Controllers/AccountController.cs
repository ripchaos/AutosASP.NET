using Autos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Autos.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Autos.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<Usuario> userManager, 
            SignInManager<Usuario> signInManager, 
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        // 🚀 Vista de Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                ModelState.AddModelError(string.Empty, "Inicio de sesión inválido.");
            }
            return View(model);
        }

        // 🚫 🔒 Eliminar opción de registro público
        [Authorize(Roles = "Recepcionista,Administrador")] // 🔹 Solo la Recepcionista y Admin pueden registrar usuarios
        public IActionResult CrearUsuario()
        {
            // Cargar los autos disponibles para seleccionar
            var autosDisponibles = _context.Autos
                .Where(a => a.Disponibilidad)
                .Select(a => new { Id = a.Id, Nombre = $"{a.Marca} {a.Modelo} ({a.Anio})" })
                .ToList();
            
            ViewBag.Autos = new SelectList(autosDisponibles, "Id", "Nombre");
            
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Recepcionista,Administrador")] // 🔹 Protección extra en la acción
        public async Task<IActionResult> CrearUsuario(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Crear el nuevo usuario cliente
                var user = new Usuario
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Nombre = model.Nombre,
                    Identificacion = model.Identificacion,
                    Direccion = model.Direccion,
                    Rol = "Cliente", // 🔹 Se asigna automáticamente el rol de Cliente
                    AutoInteresadoId = model.AutoInteresadoId
                };

                // Asignar vendedor aleatorio
                var vendedores = await _userManager.GetUsersInRoleAsync("Vendedor");
                if (vendedores.Any())
                {
                    // Obtener un vendedor aleatorio
                    var random = new Random();
                    int index = random.Next(vendedores.Count);
                    var vendedorAsignado = vendedores.ElementAt(index);
                    
                    user.VendedorAsignadoId = vendedorAsignado.Id;
                }

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // 📌 Asegurar que el rol "Cliente" existe antes de asignarlo
                    if (!await _roleManager.RoleExistsAsync("Cliente"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Cliente"));
                    }

                    // Asignar rol de Cliente
                    await _userManager.AddToRoleAsync(user, "Cliente");
                    
                    // Para administrador: opción de asignar acceso completo (todos los roles)
                    if (User.IsInRole("Administrador") && Request.Form.Keys.Contains("asignarAccesoCompleto"))
                    {
                        var rolesToAdd = new[] { "Vendedor", "Gerente", "Recepcionista" };
                        foreach (var rol in rolesToAdd)
                        {
                            if (!await _roleManager.RoleExistsAsync(rol))
                            {
                                await _roleManager.CreateAsync(new IdentityRole(rol));
                            }
                            await _userManager.AddToRoleAsync(user, rol);
                        }
                        TempData["Success"] = $"Cliente {model.Nombre} registrado exitosamente con ACCESO COMPLETO al sistema.";
                    }
                    else
                    {
                        // Obtener nombre del vendedor asignado para el mensaje de éxito
                        string vendedorNombre = "ningún vendedor";
                        if (user.VendedorAsignadoId != null)
                        {
                            var vendedor = await _userManager.FindByIdAsync(user.VendedorAsignadoId);
                            if (vendedor != null)
                            {
                                vendedorNombre = vendedor.Nombre;
                            }
                        }
                        
                        TempData["Success"] = $"Cliente {model.Nombre} registrado exitosamente y asignado a {vendedorNombre}.";
                    }
                    
                    return RedirectToAction("CrearUsuario");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            
            // Si llegamos aquí, volver a cargar los autos disponibles
            var autosDisponibles = _context.Autos
                .Where(a => a.Disponibilidad)
                .Select(a => new { Id = a.Id, Nombre = $"{a.Marca} {a.Modelo} ({a.Anio})" })
                .ToList();
                
            ViewBag.Autos = new SelectList(autosDisponibles, "Id", "Nombre", model.AutoInteresadoId);
                
            return View(model);
        }

        // 🚀 Cerrar sesión
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // 🚫 Acceso denegado
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
