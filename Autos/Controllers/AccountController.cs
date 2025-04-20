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

                // Generar contraseña aleatoria
                var password = GenerarPasswordAleatoria();
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    // 📌 Asegurar que el rol "Cliente" existe antes de asignarlo
                    if (!await _roleManager.RoleExistsAsync("Cliente"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Cliente"));
                    }

                    // Asignar rol de Cliente
                    await _userManager.AddToRoleAsync(user, "Cliente");
                    
                    // Preparar mensaje con credenciales
                    var credencialesHtml = $@"
                        <div class='row mb-2'>
                            <div class='col-4 fw-bold'>Usuario:</div>
                            <div class='col-8'>{user.Email}</div>
                        </div>
                        <div class='row'>
                            <div class='col-4 fw-bold'>Contraseña:</div>
                            <div class='col-8'>{password}</div>
                        </div>";
                    
                    TempData["CredencialesCliente"] = credencialesHtml;
                    
                    // Obtener nombre del vendedor asignado
                    string vendedorNombre = "ningún vendedor";
                    if (user.VendedorAsignadoId != null)
                    {
                        var vendedor = await _userManager.FindByIdAsync(user.VendedorAsignadoId);
                        if (vendedor != null)
                        {
                            vendedorNombre = vendedor.Nombre;
                        }
                    }
                    TempData["VendedorAsignado"] = vendedorNombre;

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
                        TempData["Success"] = $"Cliente {model.Nombre} registrado exitosamente y asignado a {vendedorNombre}.";
                    }
                    
                    // Redirigir a una acción diferente para evitar problemas con el reenvío del formulario
                    return RedirectToAction("Index", "Home");
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

        /// <summary>
        /// Genera una contraseña aleatoria segura
        /// </summary>
        private string GenerarPasswordAleatoria()
        {
            const string caracteresPermitidos = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            const int longitud = 12;
            var random = new Random();
            var password = new char[longitud];

            // Asegurar al menos una mayúscula, una minúscula, un número y un carácter especial
            password[0] = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[random.Next(26)];
            password[1] = "abcdefghijklmnopqrstuvwxyz"[random.Next(26)];
            password[2] = "0123456789"[random.Next(10)];
            password[3] = "!@#$%^&*"[random.Next(8)];

            // Llenar el resto con caracteres aleatorios
            for (int i = 4; i < longitud; i++)
            {
                password[i] = caracteresPermitidos[random.Next(caracteresPermitidos.Length)];
            }

            // Mezclar la contraseña
            for (int i = password.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = password[i];
                password[i] = password[j];
                password[j] = temp;
            }

            return new string(password);
        }
    }
}
