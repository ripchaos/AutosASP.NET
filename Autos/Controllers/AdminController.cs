using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autos.Data;
using Autos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Autos.Services;
using Autos.Models.ViewModels;
using Autos.Services.Interfaces;

namespace Autos.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IUsuarioService _usuarioService;
        private readonly IReservaService _reservaService;
        private readonly IVentaService _ventaService;
        private readonly ISucursalService _sucursalService;
        private readonly IAutoService _autoService;

        public AdminController(
            UserManager<Usuario> userManager, 
            RoleManager<IdentityRole> roleManager, 
            ApplicationDbContext context, 
            IEmailService emailService,
            IUsuarioService usuarioService,
            IReservaService reservaService,
            IVentaService ventaService,
            ISucursalService sucursalService,
            IAutoService autoService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _emailService = emailService;
            _usuarioService = usuarioService;
            _reservaService = reservaService;
            _ventaService = ventaService;
            _sucursalService = sucursalService;
            _autoService = autoService;
        }

        // 🔹 Vista para el Panel de Administración
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Verificar y actualizar los autos existentes que no tienen el estado de reserva
                var autosConEstadoReservaVacio = await _context.Autos
                    .Where(a => a.EstadoReserva == "")
                    .ToListAsync();

                foreach (var auto in autosConEstadoReservaVacio)
                {
                    await _autoService.ActualizarEstadoReservaAsync(auto.Id, "Disponible");
                }

                // Obtener estadísticas para el panel de administración
                var totalUsuarios = await _userManager.Users.CountAsync();
                var totalAutos = await _context.Autos.CountAsync();
                var totalReservas = await _context.Reservas.CountAsync();
                var totalVentas = await _context.Ventas.CountAsync();
                
                // Obtener usuarios recientes
                var usuariosRecientes = await _userManager.Users
                    .OrderByDescending(u => u.Id)
                    .Take(5)
                    .ToListAsync();
                
                // Obtener ventas recientes
                var ventasRecientes = (await _ventaService.ObtenerVentasReciententesAsync(5)).ToList();
                
                // Calcular estadísticas adicionales
                var fechaInicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var ventasDelMes = await _ventaService.ContarVentasMesActualAsync();
                
                // Estadísticas de ventas
                var ingresosTotales = await _ventaService.CalcularIngresosTotalesAsync();
                var ingresosMensuales = await _ventaService.CalcularIngresosMensualesAsync(DateTime.Now.Month, DateTime.Now.Year);
                
                var autosDisponibles = (await _autoService.ObtenerAutosDisponiblesAsync()).Count();
                
                // Obtener distribución de usuarios por rol
                var totalAdministradores = (await _usuarioService.ObtenerUsuariosPorRolAsync("Administrador")).Count();
                var totalGerentes = (await _usuarioService.ObtenerUsuariosPorRolAsync("Gerente")).Count();
                var totalVendedores = (await _usuarioService.ObtenerUsuariosPorRolAsync("Vendedor")).Count();
                var totalRecepcionistas = (await _usuarioService.ObtenerUsuariosPorRolAsync("Recepcionista")).Count();
                var totalClientes = (await _usuarioService.ObtenerUsuariosPorRolAsync("Cliente")).Count();
                
                // Crear el viewmodel
                var viewModel = new AdminDashboardViewModel
                {
                    TotalUsuarios = totalUsuarios,
                    TotalAutos = totalAutos,
                    TotalReservas = totalReservas,
                    TotalVentas = totalVentas,
                    UsuariosRecientes = usuariosRecientes,
                    VentasRecientes = ventasRecientes,
                    VentasDelMes = ventasDelMes,
                    IngresosTotales = ingresosTotales,
                    IngresosMensuales = ingresosMensuales,
                    AutosDisponibles = autosDisponibles,
                    TotalAdministradores = totalAdministradores,
                    TotalGerentes = totalGerentes,
                    TotalVendedores = totalVendedores,
                    TotalRecepcionistas = totalRecepcionistas,
                    TotalClientes = totalClientes
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Registrar el error
                Console.WriteLine($"Error en Dashboard: {ex.Message}");
                
                // Proporcionar datos vacíos para evitar errores en la vista
                var viewModel = new AdminDashboardViewModel
                {
                    TotalUsuarios = 0,
                    TotalAutos = 0,
                    TotalReservas = 0,
                    TotalVentas = 0,
                    UsuariosRecientes = new List<Usuario>(),
                    VentasRecientes = new List<Venta>(),
                    VentasDelMes = 0,
                    IngresosTotales = 0,
                    IngresosMensuales = 0,
                    AutosDisponibles = 0,
                    TotalAdministradores = 0,
                    TotalGerentes = 0,
                    TotalVendedores = 0,
                    TotalRecepcionistas = 0,
                    TotalClientes = 0
                };
                
                return View(viewModel);
            }
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
                    Rol = model.Rol,
                    Identificacion = model.Identificacion,
                    Direccion = model.Direccion
                };

                // Asegurar que password no sea nulo antes de crear el usuario
                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError(string.Empty, "La contraseña no puede estar vacía.");
                    return View(model);
                }
                
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

        // 🔹 Listar Clientes
        public async Task<IActionResult> Clientes()
        {
            var clientes = await _usuarioService.ObtenerUsuariosPorRolAsync("Cliente");
            var clientesOrdenados = clientes.OrderBy(u => u.Nombre).ToList();

            // Agregar información del vendedor para cada cliente
            var clientesViewModel = new List<ClienteViewModel>();
            
            foreach (var cliente in clientesOrdenados)
            {
                // Obtener el vendedor asignado si existe
                Usuario? vendedor = null;
                if (!string.IsNullOrEmpty(cliente.VendedorAsignadoId))
                {
                    vendedor = await _usuarioService.ObtenerUsuarioPorIdAsync(cliente.VendedorAsignadoId);
                }
                
                clientesViewModel.Add(new ClienteViewModel
                {
                    Cliente = cliente,
                    VendedorNombre = vendedor?.Nombre ?? "No asignado",
                    ReservasActivas = await _context.Reservas
                        .CountAsync(r => r.UsuarioId == cliente.Id && r.Estado == "Activa"),
                    ComprasRealizadas = await _context.Ventas
                        .CountAsync(v => v.ClienteId == cliente.Id)
                });
            }
            
            return View(clientesViewModel);
        }

        // 🔹 Ver Detalle de Cliente
        public async Task<IActionResult> DetalleCliente(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID de cliente inválido";
                return RedirectToAction("Clientes");
            }

            var cliente = await _usuarioService.ObtenerUsuarioPorIdAsync(id);
            if (cliente == null)
            {
                TempData["Error"] = "Cliente no encontrado";
                return RedirectToAction("Clientes");
            }

            // Obtener el vendedor asignado si existe
            Usuario? vendedor = null;
            if (!string.IsNullOrEmpty(cliente.VendedorAsignadoId))
            {
                vendedor = await _usuarioService.ObtenerUsuarioPorIdAsync(cliente.VendedorAsignadoId);
            }

            // Obtener reservas del cliente
            var reservas = await _reservaService.ObtenerReservasPorClienteAsync(cliente.Id);

            // Obtener compras del cliente
            var compras = await _ventaService.ObtenerVentasPorClienteAsync(cliente.Id);

            var viewModel = new DetalleClienteViewModel
            {
                Cliente = cliente,
                VendedorAsignado = vendedor,
                Reservas = reservas.ToList(),
                Compras = compras.ToList()
            };

            return View(viewModel);
        }

        // 🔹 Vista para Reasignar Vendedor a Cliente
        public async Task<IActionResult> ReasignarVendedor(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID de cliente inválido";
                return RedirectToAction("Clientes");
            }

            var cliente = await _usuarioService.ObtenerUsuarioPorIdAsync(id);
            if (cliente == null)
            {
                TempData["Error"] = "Cliente no encontrado";
                return RedirectToAction("Clientes");
            }

            // Obtener el vendedor actual asignado
            var vendedorActual = cliente.VendedorAsignadoId != null 
                ? await _usuarioService.ObtenerUsuarioPorIdAsync(cliente.VendedorAsignadoId) 
                : null;

            // Obtener la lista de vendedores disponibles
            var vendedores = await _usuarioService.ObtenerUsuariosPorRolAsync("Vendedor");
            var vendedoresSelectList = vendedores
                .OrderBy(v => v.Nombre)
                .Select(v => new SelectListItem
                {
                    Value = v.Id,
                    Text = v.Nombre,
                    Selected = v.Id == cliente.VendedorAsignadoId
                })
                .ToList();

            var viewModel = new ReasignarVendedorViewModel
            {
                ClienteId = cliente.Id,
                NombreCliente = cliente.Nombre,
                EmailCliente = cliente.Email ?? string.Empty,
                VendedorActualId = cliente.VendedorAsignadoId,
                VendedorActualNombre = vendedorActual?.Nombre ?? "No asignado",
                Vendedores = vendedoresSelectList
            };

            return View(viewModel);
        }

        // 🔹 POST: Reasignar Vendedor a Cliente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReasignarVendedor(ReasignarVendedorViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener el cliente
                    var cliente = await _usuarioService.ObtenerUsuarioPorIdAsync(model.ClienteId);
                    if (cliente == null)
                    {
                        TempData["Error"] = "Cliente no encontrado";
                        return RedirectToAction("Clientes");
                    }

                    // Verificar si se seleccionó un vendedor y si existe
                    if (string.IsNullOrEmpty(model.NuevoVendedorId))
                    {
                        ModelState.AddModelError("NuevoVendedorId", "Debe seleccionar un vendedor");
                        
                        // Recargar la lista de vendedores
                        var vendedores = await _usuarioService.ObtenerUsuariosPorRolAsync("Vendedor");
                        model.Vendedores = vendedores
                            .OrderBy(v => v.Nombre)
                            .Select(v => new SelectListItem
                            {
                                Value = v.Id,
                                Text = v.Nombre
                            })
                            .ToList();
                        
                        return View(model);
                    }
                    
                    var vendedor = await _usuarioService.ObtenerUsuarioPorIdAsync(model.NuevoVendedorId);
                    if (vendedor == null)
                    {
                        ModelState.AddModelError("NuevoVendedorId", "El vendedor seleccionado no existe");
                        
                        // Recargar la lista de vendedores
                        var vendedores = await _usuarioService.ObtenerUsuariosPorRolAsync("Vendedor");
                        model.Vendedores = vendedores
                            .OrderBy(v => v.Nombre)
                            .Select(v => new SelectListItem
                            {
                                Value = v.Id,
                                Text = v.Nombre
                            })
                            .ToList();
                        
                        return View(model);
                    }

                    // Verificar si es el mismo vendedor
                    if (cliente.VendedorAsignadoId == model.NuevoVendedorId)
                    {
                        // El vendedor ya está asignado, redirigir sin cambios
                        TempData["Info"] = "No se realizaron cambios, el vendedor seleccionado ya estaba asignado";
                        return RedirectToAction("DetalleCliente", new { id = model.ClienteId });
                    }

                    // Actualizar el vendedor asignado
                    bool resultado = await _usuarioService.AsignarVendedorAClienteAsync(model.ClienteId, model.NuevoVendedorId);
                    
                    if (resultado)
                    {
                        TempData["Exito"] = "Vendedor reasignado correctamente";
                        return RedirectToAction("DetalleCliente", new { id = model.ClienteId });
                    }
                    else
                    {
                        TempData["Error"] = "No se pudo asignar el vendedor";
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error al reasignar vendedor: " + ex.Message;
                }
            }

            // Si llegamos aquí, hubo un error. Recargar lista de vendedores
            var vendedoresLista = await _usuarioService.ObtenerUsuariosPorRolAsync("Vendedor");
            model.Vendedores = vendedoresLista
                .OrderBy(v => v.Nombre)
                .Select(v => new SelectListItem
                {
                    Value = v.Id,
                    Text = v.Nombre
                })
                .ToList();

            return View(model);
        }

        // 🔹 Asignar Roles a Usuarios
        [HttpPost]
        public async Task<IActionResult> AsignarRol(string userId, string nuevoRol)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(nuevoRol))
                {
                    return Json(new { success = false, message = "Datos incompletos" });
                }

                // Verificar si el rol es válido
                if (!await _roleManager.RoleExistsAsync(nuevoRol))
                {
                    return Json(new { success = false, message = "El rol especificado no existe" });
                }

                // Obtener el usuario
                var usuario = await _usuarioService.ObtenerUsuarioPorIdAsync(userId);
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // Obtener los roles actuales del usuario
                var rolesActuales = await _usuarioService.ObtenerRolesDeUsuarioAsync(usuario);

                // Remover los roles actuales
                foreach (var rol in rolesActuales)
                {
                    await _usuarioService.EliminarRolAsync(usuario, rol);
                }

                // Asignar el nuevo rol
                var resultado = await _usuarioService.AsignarRolAsync(usuario, nuevoRol);

                if (resultado)
                {
                    // Actualizar el campo Rol del usuario para mantener consistencia
                    usuario.Rol = nuevoRol;
                    await _userManager.UpdateAsync(usuario);

                    return Json(new { success = true, message = "Rol asignado correctamente" });
                }
                else
                {
                    return Json(new { success = false, message = "Error al asignar rol" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
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

        // 🔹 Vista para Configuración Global del Sistema
        public IActionResult Configuracion()
        {
            return View();
        }

        // 🔹 Vista para Configuración de Roles del Sistema
        public async Task<IActionResult> ConfiguracionRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        // 🔹 Vista para Configuración de Parámetros de Ventas
        public async Task<IActionResult> ConfiguracionVentas()
        {
            var configuracionVentas = await _context.DescuentosConfig.FirstOrDefaultAsync() 
                ?? new DescuentoConfig();
            return View(configuracionVentas);
        }
        
        // 🔹 Vista para Configuración de Sucursales
        public async Task<IActionResult> ConfiguracionSucursales()
        {
            var sucursales = await _sucursalService.ObtenerTodasLasSucursalesAsync();
            return View(sucursales);
        }
        
        // GET: Admin/CrearSucursal
        public async Task<IActionResult> CrearSucursal()
        {
            // Obtener todos los gerentes
            var todosGerentes = await _userManager.GetUsersInRoleAsync("Gerente");
            
            // Obtener los IDs de gerentes ya asignados a sucursales
            var gerentesAsignados = await _context.Sucursales
                .Where(s => !string.IsNullOrEmpty(s.GerenteId))
                .Select(s => s.GerenteId)
                .ToListAsync();
                
            // Filtrar para obtener solo gerentes no asignados
            var gerentesDisponibles = todosGerentes
                .Where(g => !gerentesAsignados.Contains(g.Id))
                .Select(g => new SelectListItem { Value = g.Id, Text = g.Nombre })
                .ToList();
                
            ViewBag.Gerentes = gerentesDisponibles;
                
            return View(new Sucursal { Nombre = "", Direccion = "" });
        }
        
        // POST: Admin/CrearSucursal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearSucursal(Sucursal sucursal)
        {
            if (ModelState.IsValid)
            {
                // Validar que el gerente no esté asignado a otra sucursal
                if (!string.IsNullOrEmpty(sucursal.GerenteId))
                {
                    var gerenteExistente = await _context.Sucursales
                        .AnyAsync(s => s.GerenteId == sucursal.GerenteId);
                        
                    if (gerenteExistente)
                    {
                        ModelState.AddModelError("GerenteId", "Este gerente ya está asignado a otra sucursal.");
                        
                        // Recargar lista de gerentes
                        var gerentes = await _userManager.GetUsersInRoleAsync("Gerente");
                        ViewBag.Gerentes = gerentes
                            .Select(g => new SelectListItem { Value = g.Id, Text = g.Nombre })
                            .ToList();
                            
                        return View(sucursal);
                    }
                }
                
                _context.Sucursales.Add(sucursal);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Sucursal '{sucursal.Nombre}' creada correctamente.";
                return RedirectToAction(nameof(ConfiguracionSucursales));
            }
            
            // Si no es válido, volver a cargar la lista de gerentes
            var gerentesDisponibles = await _userManager.GetUsersInRoleAsync("Gerente");
            ViewBag.Gerentes = gerentesDisponibles
                .Select(g => new SelectListItem { Value = g.Id, Text = g.Nombre })
                .ToList();
                
            return View(sucursal);
        }
        
        // GET: Admin/EditarSucursal/5
        public async Task<IActionResult> EditarSucursal(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var sucursal = await _context.Sucursales.FindAsync(id);
            if (sucursal == null)
            {
                return NotFound();
            }
            
            // Obtener todos los gerentes
            var todosGerentes = await _userManager.GetUsersInRoleAsync("Gerente");
            
            // Obtener los IDs de gerentes ya asignados a otras sucursales
            var gerentesAsignados = await _context.Sucursales
                .Where(s => s.Id != id && !string.IsNullOrEmpty(s.GerenteId))
                .Select(s => s.GerenteId)
                .ToListAsync();
                
            // Filtrar para obtener solo gerentes no asignados o el actual de esta sucursal
            var gerentesDisponibles = todosGerentes
                .Where(g => !gerentesAsignados.Contains(g.Id))
                .Select(g => new SelectListItem 
                { 
                    Value = g.Id, 
                    Text = g.Nombre,
                    Selected = g.Id == sucursal.GerenteId
                })
                .ToList();
                
            ViewBag.Gerentes = gerentesDisponibles;
                
            return View(sucursal);
        }
        
        // POST: Admin/EditarSucursal/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarSucursal(int id, Sucursal sucursal)
        {
            if (id != sucursal.Id)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                // Validar que el gerente no esté asignado a otra sucursal
                if (!string.IsNullOrEmpty(sucursal.GerenteId))
                {
                    var gerenteExistente = await _context.Sucursales
                        .AnyAsync(s => s.Id != sucursal.Id && s.GerenteId == sucursal.GerenteId);
                        
                    if (gerenteExistente)
                    {
                        ModelState.AddModelError("GerenteId", "Este gerente ya está asignado a otra sucursal.");
                        
                        // Recargar lista de gerentes
                        var gerentes = await _userManager.GetUsersInRoleAsync("Gerente");
                        ViewBag.Gerentes = gerentes
                            .Select(g => new SelectListItem 
                            { 
                                Value = g.Id, 
                                Text = g.Nombre,
                                Selected = g.Id == sucursal.GerenteId
                            })
                            .ToList();
                            
                        return View(sucursal);
                    }
                }
                
                try
                {
                    _context.Update(sucursal);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"Sucursal '{sucursal.Nombre}' actualizada correctamente.";
                    return RedirectToAction(nameof(ConfiguracionSucursales));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SucursalExists(sucursal.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            // Si no es válido, volver a cargar la lista de gerentes
            var gerentesDisponibles = await _userManager.GetUsersInRoleAsync("Gerente");
            ViewBag.Gerentes = gerentesDisponibles
                .Select(g => new SelectListItem 
                { 
                    Value = g.Id, 
                    Text = g.Nombre,
                    Selected = g.Id == sucursal.GerenteId
                })
                .ToList();
                
            return View(sucursal);
        }
        
        // GET: Admin/EliminarSucursal/5
        public async Task<IActionResult> EliminarSucursal(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var sucursal = await _context.Sucursales
                .Include(s => s.Gerente)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (sucursal == null)
            {
                return NotFound();
            }
            
            return View(sucursal);
        }
        
        // POST: Admin/EliminarSucursal/5
        [HttpPost, ActionName("EliminarSucursal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminarSucursal(int id)
        {
            var sucursal = await _context.Sucursales.FindAsync(id);
            
            if (sucursal != null)
            {
                // Verificar si hay autos asociados a esta sucursal
                var autosEnSucursal = await _context.Autos.AnyAsync(a => a.SucursalId == id);
                if (autosEnSucursal)
                {
                    TempData["Error"] = $"No se puede eliminar la sucursal '{sucursal.Nombre}' porque tiene autos asociados.";
                    return RedirectToAction(nameof(ConfiguracionSucursales));
                }
                
                _context.Sucursales.Remove(sucursal);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Sucursal '{sucursal.Nombre}' eliminada correctamente.";
            }
            
            return RedirectToAction(nameof(ConfiguracionSucursales));
        }
        
        // Método auxiliar para verificar si existe una sucursal
        private bool SucursalExists(int id)
        {
            return _context.Sucursales.Any(e => e.Id == id);
        }
        
        // 🔹 Vista para Métricas y Estadísticas del Sistema
        public async Task<IActionResult> ConfiguracionMetricas()
        {
            try
            {
                // Estadísticas básicas
                ViewBag.TotalUsuarios = await _userManager.Users.CountAsync();
                ViewBag.TotalVentas = await _context.Ventas.CountAsync();
                ViewBag.TotalAutos = await _context.Autos.CountAsync();
                ViewBag.TotalReservas = await _context.Reservas.CountAsync();
                
                // Contadores para usuarios por rol
                ViewBag.TotalAdministradores = (await _usuarioService.ObtenerUsuariosPorRolAsync("Administrador")).Count();
                ViewBag.TotalGerentes = (await _usuarioService.ObtenerUsuariosPorRolAsync("Gerente")).Count();
                ViewBag.TotalVendedores = (await _usuarioService.ObtenerUsuariosPorRolAsync("Vendedor")).Count();
                ViewBag.TotalRecepcionistas = (await _usuarioService.ObtenerUsuariosPorRolAsync("Recepcionista")).Count();
                ViewBag.TotalClientes = (await _usuarioService.ObtenerUsuariosPorRolAsync("Cliente")).Count();
    
                // Datos para gráficos
                
                // 1. Ventas por vendedor (últimos 30 días)
                var fechaInicio30Dias = DateTime.Now.AddDays(-30);
                
                // Modificar la consulta para evitar el error de agrupamiento
                var todasLasVentas = await _context.Ventas
                    .Where(v => v.Fecha >= fechaInicio30Dias)
                    .Include(v => v.Vendedor)
                    .Where(v => v.Vendedor != null)
                    .ToListAsync();
                    
                // Agrupar manualmente en memoria 
                var ventasPorVendedor = todasLasVentas
                    .GroupBy(v => v.Vendedor!.Nombre)
                    .Select(g => new { 
                        Vendedor = g.Key, 
                        TotalVentas = g.Count(), 
                        MontoTotal = g.Sum(v => v.MontoFinal) 
                    })
                    .OrderByDescending(v => v.MontoTotal)
                    .Take(10)
                    .ToList();
                    
                ViewBag.VendedoresLabels = ventasPorVendedor.Select(v => v.Vendedor).ToList();
                ViewBag.VendedoresCantidad = ventasPorVendedor.Select(v => v.TotalVentas).ToList();
                ViewBag.VendedoresMonto = ventasPorVendedor.Select(v => (double)v.MontoTotal).ToList();
                
                // 2. Ventas por mes (últimos 6 meses)
                var fechaInicioSeisMeses = DateTime.Now.AddMonths(-6);
                
                // Consultar todas las ventas y agrupar en memoria
                var ventasUltimosSeisMeses = await _context.Ventas
                    .Where(v => v.Fecha >= fechaInicioSeisMeses)
                    .ToListAsync();
                    
                // Preparar las listas para los meses
                var ultimosSeisMeses = new List<string>();
                var ventasPorMesData = new List<int>();
                var montosPorMesData = new List<double>();
                
                // Asegurar que tenemos datos para todos los meses, incluso si no hay ventas
                for (int i = 6; i > 0; i--)
                {
                    var fecha = DateTime.Now.AddMonths(-i + 1);
                    var nombreMes = new DateTime(fecha.Year, fecha.Month, 1).ToString("MMM yyyy");
                    ultimosSeisMeses.Add(nombreMes);
                    
                    // Filtrar y agrupar en memoria
                    var ventasMes = ventasUltimosSeisMeses
                        .Where(v => v.Fecha.Year == fecha.Year && v.Fecha.Month == fecha.Month)
                        .ToList();
                        
                    ventasPorMesData.Add(ventasMes.Count);
                    montosPorMesData.Add(ventasMes.Any() ? (double)ventasMes.Sum(v => v.MontoFinal) : 0);
                }
                
                ViewBag.MesesLabels = ultimosSeisMeses;
                ViewBag.VentasPorMes = ventasPorMesData;
                ViewBag.MontosPorMes = montosPorMesData;
                
                // 3. Reservas por mes (últimos 6 meses)
                var reservasUltimosSeisMeses = await _context.Reservas
                    .Where(r => r.FechaReserva >= fechaInicioSeisMeses)
                    .ToListAsync();
                    
                var reservasPorMesData = new List<int>();
                
                // Asegurar que tenemos datos para todos los meses
                for (int i = 6; i > 0; i--)
                {
                    var fecha = DateTime.Now.AddMonths(-i + 1);
                    var reservasMes = reservasUltimosSeisMeses
                        .Where(r => r.FechaReserva.Year == fecha.Year && r.FechaReserva.Month == fecha.Month)
                        .ToList();
                        
                    reservasPorMesData.Add(reservasMes.Count);
                }
                
                ViewBag.ReservasPorMes = reservasPorMesData;
                
                // 4. Clientes nuevos por mes (últimos 6 meses)
                var todosLosClientes = await _userManager.Users
                    .Where(u => u.Rol == "Cliente")
                    .ToListAsync();
                    
                var clientesNuevosPorMesData = new List<int>();
                
                // Asegurar que tenemos datos para todos los meses
                for (int i = 6; i > 0; i--)
                {
                    var fecha = DateTime.Now.AddMonths(-i + 1);
                    var clientesMes = todosLosClientes
                        .Where(u => u.FechaRegistro.HasValue && 
                               u.FechaRegistro.Value.Year == fecha.Year && 
                               u.FechaRegistro.Value.Month == fecha.Month)
                        .ToList();
                        
                    clientesNuevosPorMesData.Add(clientesMes.Count);
                }
                
                ViewBag.ClientesNuevosPorMes = clientesNuevosPorMesData;
                
                // 5. Ventas por sucursal
                var todasLasVentasConSucursal = await _context.Ventas
                    .Include(v => v.Auto)
                    .ThenInclude(a => a!.Sucursal)
                    .Where(v => v.Auto != null && v.Auto.Sucursal != null)
                    .ToListAsync();
                    
                // Agrupar manualmente
                var ventasPorSucursal = todasLasVentasConSucursal
                    .GroupBy(v => v.Auto!.Sucursal!.Nombre)
                    .Select(g => new {
                        Sucursal = g.Key,
                        TotalVentas = g.Count(),
                        MontoTotal = g.Sum(v => v.MontoFinal)
                    })
                    .OrderByDescending(v => v.MontoTotal)
                    .ToList();
                    
                ViewBag.SucursalesLabels = ventasPorSucursal.Select(v => v.Sucursal).ToList();
                ViewBag.VentasPorSucursal = ventasPorSucursal.Select(v => v.TotalVentas).ToList();
                ViewBag.MontosPorSucursal = ventasPorSucursal.Select(v => (double)v.MontoTotal).ToList();
                
                return View();
            }
            catch (Exception ex)
            {
                // Registrar el error
                Console.WriteLine($"Error en ConfiguracionMetricas: {ex.Message}");
                
                // Proporcionar datos vacíos para evitar errores en la vista
                ViewBag.TotalUsuarios = 0;
                ViewBag.TotalVentas = 0;
                ViewBag.TotalAutos = 0;
                ViewBag.TotalReservas = 0;
                ViewBag.TotalAdministradores = 0; 
                ViewBag.TotalGerentes = 0;
                ViewBag.TotalVendedores = 0;
                ViewBag.TotalRecepcionistas = 0;
                ViewBag.TotalClientes = 0;
                
                ViewBag.VendedoresLabels = new List<string>();
                ViewBag.VendedoresCantidad = new List<int>();
                ViewBag.VendedoresMonto = new List<double>();
                
                ViewBag.MesesLabels = new List<string>();
                ViewBag.VentasPorMes = new List<int>();
                ViewBag.MontosPorMes = new List<double>();
                
                ViewBag.ReservasPorMes = new List<int>();
                ViewBag.ClientesNuevosPorMes = new List<int>();
                
                ViewBag.SucursalesLabels = new List<string>();
                ViewBag.VentasPorSucursal = new List<int>();
                ViewBag.MontosPorSucursal = new List<double>();
                
                return View();
            }
        }

        // 🔹 Vista para Configuración de Notificaciones del Sistema
        public async Task<IActionResult> ConfiguracionNotificaciones()
        {
            var configuracion = await _context.ConfiguracionCorreo.FirstOrDefaultAsync();
            
            if (configuracion == null)
            {
                // Crear configuración por defecto
                configuracion = new ConfiguracionCorreo
                {
                    Servidor = "smtp.gmail.com",
                    Puerto = 587,
                    RequiereSSL = true,
                    Usuario = "correo@tuempresa.com",
                    Password = "",
                    EmailRemitente = "correo@tuempresa.com",
                    NombreRemitente = "Concesionario de Autos",
                    NotificarReservas = true,
                    NotificarVentas = true,
                    NotificarDescuentos = true,
                    NotificarNuevasReservas = true,
                    NotificarNuevasVentas = true,
                    NotificarSolicitudesDescuento = true
                };
            }
            
            return View(configuracion);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarConfiguracionCorreo(ConfiguracionCorreo configuracion)
        {
            if (!ModelState.IsValid)
            {
                return View("ConfiguracionNotificaciones", configuracion);
            }

            var configExistente = await _context.ConfiguracionCorreo.FirstOrDefaultAsync();
            
            if (configExistente == null)
            {
                // Crear nueva configuración
                _context.ConfiguracionCorreo.Add(configuracion);
            }
            else
            {
                // Actualizar configuración existente
                configExistente.Servidor = configuracion.Servidor;
                configExistente.Puerto = configuracion.Puerto;
                configExistente.RequiereSSL = configuracion.RequiereSSL;
                configExistente.Usuario = configuracion.Usuario;
                configExistente.Password = configuracion.Password;
                configExistente.EmailRemitente = configuracion.EmailRemitente;
                configExistente.NombreRemitente = configuracion.NombreRemitente;
                configExistente.NotificarReservas = configuracion.NotificarReservas;
                configExistente.NotificarVentas = configuracion.NotificarVentas;
                configExistente.NotificarDescuentos = configuracion.NotificarDescuentos;
                configExistente.NotificarNuevasReservas = configuracion.NotificarNuevasReservas;
                configExistente.NotificarNuevasVentas = configuracion.NotificarNuevasVentas;
                configExistente.NotificarSolicitudesDescuento = configuracion.NotificarSolicitudesDescuento;
                configExistente.EmailsNotificacionesInternas = configuracion.EmailsNotificacionesInternas;
                
                _context.ConfiguracionCorreo.Update(configExistente);
            }
            
            await _context.SaveChangesAsync();
            TempData["Success"] = "Configuración de correo guardada correctamente.";
            
            return RedirectToAction(nameof(ConfiguracionNotificaciones));
        }
        
        [HttpPost]
        public async Task<IActionResult> ProbarConexionSMTP([FromForm] ConfiguracionCorreo configuracion)
        {
            try
            {
                // Validar datos mínimos necesarios
                if (string.IsNullOrEmpty(configuracion.Servidor) || 
                    string.IsNullOrEmpty(configuracion.Usuario) || 
                    string.IsNullOrEmpty(configuracion.Password) ||
                    string.IsNullOrEmpty(configuracion.EmailRemitente))
                {
                    return Json(new { success = false, message = "Faltan datos para la conexión SMTP." });
                }

                // Probar la conexión usando el servicio de correo
                var resultado = await _emailService.ProbarConexionAsync(configuracion);
                
                if (resultado)
                {
                    return Json(new { success = true, message = "Conexión exitosa. Se ha enviado un correo de prueba." });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo establecer conexión con el servidor SMTP." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        #region Gestión de Roles

        [HttpPost]
        public async Task<IActionResult> CrearRol(string nombreRol)
        {
            try
            {
                if (string.IsNullOrEmpty(nombreRol))
                {
                    TempData["Error"] = "El nombre del rol no puede estar vacío";
                    return RedirectToAction(nameof(ConfiguracionRoles));
                }

                if (await _roleManager.RoleExistsAsync(nombreRol))
                {
                    TempData["Error"] = $"El rol '{nombreRol}' ya existe";
                    return RedirectToAction(nameof(ConfiguracionRoles));
                }

                var resultado = await _roleManager.CreateAsync(new IdentityRole(nombreRol));
                
                if (resultado.Succeeded)
                {
                    TempData["Exito"] = $"Rol '{nombreRol}' creado correctamente";
                }
                else
                {
                    TempData["Error"] = $"Error al crear el rol: {string.Join(", ", resultado.Errors.Select(e => e.Description))}";
                }

                return RedirectToAction(nameof(ConfiguracionRoles));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error inesperado al crear el rol: {ex.Message}";
                return RedirectToAction(nameof(ConfiguracionRoles));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditarRol(string rolId, string nombreRol)
        {
            if (string.IsNullOrEmpty(nombreRol) || string.IsNullOrEmpty(rolId))
            {
                TempData["Error"] = "El nombre del rol y el ID del rol no pueden estar vacíos";
                return RedirectToAction(nameof(ConfiguracionRoles));
            }

            var rol = await _roleManager.FindByIdAsync(rolId);
            if (rol == null)
            {
                TempData["Error"] = "Rol no encontrado";
                return RedirectToAction(nameof(ConfiguracionRoles));
            }

            // Asignar el nuevo nombre normalizado también
            rol.Name = nombreRol;
            rol.NormalizedName = nombreRol.ToUpper();
            
            var resultado = await _roleManager.UpdateAsync(rol);
            
            if (resultado.Succeeded)
            {
                TempData["Exito"] = $"Rol actualizado correctamente a '{nombreRol}'";
            }
            else
            {
                TempData["Error"] = $"Error al actualizar el rol: {string.Join(", ", resultado.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction(nameof(ConfiguracionRoles));
        }

        [HttpPost]
        public async Task<IActionResult> EliminarRol(string rolId)
        {
            if (string.IsNullOrEmpty(rolId))
            {
                TempData["Error"] = "ID de rol no válido";
                return RedirectToAction(nameof(ConfiguracionRoles));
            }
            
            var rol = await _roleManager.FindByIdAsync(rolId);
            if (rol == null)
            {
                TempData["Error"] = "Rol no encontrado";
                return RedirectToAction(nameof(ConfiguracionRoles));
            }

            // Verificar si hay usuarios con este rol
            var usuariosEnRol = await _userManager.GetUsersInRoleAsync(rol.Name ?? string.Empty);
            if (usuariosEnRol.Any())
            {
                TempData["Error"] = $"No se puede eliminar el rol '{rol.Name ?? "desconocido"}' porque hay {usuariosEnRol.Count} usuarios asignados a este rol";
                return RedirectToAction(nameof(ConfiguracionRoles));
            }

            var resultado = await _roleManager.DeleteAsync(rol);
            
            if (resultado.Succeeded)
            {
                TempData["Exito"] = $"Rol '{rol.Name ?? "desconocido"}' eliminado correctamente";
            }
            else
            {
                TempData["Error"] = $"Error al eliminar el rol: {string.Join(", ", resultado.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction(nameof(ConfiguracionRoles));
        }

        #endregion

        #region Estadísticas del Sistema

        public IActionResult Estadisticas()
        {
            // Estadísticas generales
            var totalVentas = _context.Ventas.Any() 
                ? _context.Ventas.Sum(v => v.MontoFinal)
                : 0;
                
            var ventasPorMes = _context.Ventas
                .GroupBy(v => new { v.Fecha.Year, v.Fecha.Month })
                .Select(g => new {
                    Periodo = $"{g.Key.Year}-{g.Key.Month}",
                    Total = g.Sum(v => v.MontoFinal)
                })
                .OrderBy(x => x.Periodo)
                .Take(12)
                .ToList();

            // Ventas por sucursal
            var ventasPorSucursal = _context.Ventas
                .Include(v => v.Auto)
                .ThenInclude(a => a!.Sucursal)
                .Where(v => v.Auto != null && v.Auto.Sucursal != null)
                .GroupBy(v => v.Auto!.Sucursal!.Nombre ?? "Sin Sucursal")
                .Select(g => new {
                    Sucursal = g.Key,
                    Total = g.Sum(v => v.MontoFinal),
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            // Vendedores con más ventas
            var mejoresVendedores = _context.Ventas
                .Include(v => v.Vendedor)
                .Where(v => v.Vendedor != null)
                .GroupBy(v => new { v.VendedorId, Nombre = v.Vendedor!.Nombre ?? "Sin Nombre" })
                .Select(g => new {
                    NombreCompleto = g.Key.Nombre,
                    TotalVentas = g.Sum(v => v.MontoFinal),
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.TotalVentas)
                .Take(5)
                .ToList();

            ViewBag.TotalVentas = totalVentas;
            ViewBag.VentasPorMes = ventasPorMes;
            ViewBag.VentasPorSucursal = ventasPorSucursal;
            ViewBag.MejoresVendedores = mejoresVendedores;

            return View();
        }

        #endregion

        // Método para obtener usuarios por rol (para AJAX)
        [HttpGet]
        public async Task<IActionResult> ObtenerUsuariosPorRol(string rolId)
        {
            try
            {
                var rol = await _roleManager.FindByIdAsync(rolId);
                if (rol == null || string.IsNullOrEmpty(rol.Name))
                {
                    return Json(new { success = false, message = "Rol no encontrado o inválido" });
                }

                var usuarios = await _usuarioService.ObtenerUsuariosPorRolAsync(rol.Name ?? "");

                return Json(new { success = true, usuarios = usuarios.Select(u => new { u.Id, u.UserName, u.Email }) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener usuarios: " + ex.Message });
            }
        }

        // 🔹 Restablecer Contraseña de Usuario
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> RestablecerPassword(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "ID de usuario inválido";
                return RedirectToAction("Usuarios");
            }

            var usuario = await _usuarioService.ObtenerUsuarioPorIdAsync(userId);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado";
                return RedirectToAction("Usuarios");
            }

            var modelo = new RestablecerPasswordViewModel
            {
                UserId = userId,
                Email = usuario.Email ?? string.Empty,
                NombreUsuario = usuario.Nombre ?? usuario.UserName ?? "Usuario"
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> RestablecerPassword(RestablecerPasswordViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = await _usuarioService.ObtenerUsuarioPorIdAsync(modelo.UserId);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado";
                return RedirectToAction("Usuarios");
            }

            if (usuario.Email != modelo.Email)
            {
                ModelState.AddModelError("Email", "El correo electrónico no coincide con el usuario");
                return View(modelo);
            }

            // Restablecer contraseña
            var resultado = await _usuarioService.RestablecerPasswordAsync(usuario, modelo.NuevaPassword);

            if (resultado)
            {
                TempData["Success"] = $"Contraseña restablecida correctamente para {usuario.Nombre ?? usuario.UserName}";
                return RedirectToAction("Usuarios");
            }

            ModelState.AddModelError(string.Empty, "No se pudo restablecer la contraseña. Intente nuevamente.");
            return View(modelo);
        }

        // 🔹 Vista para Administrar Vendedores (desde vista de Admin)
        public async Task<IActionResult> AdministrarVendedoresSucursal(int? sucursalId)
        {
            // Si no se proporciona un ID de sucursal, redirigir a la lista de sucursales
            if (sucursalId == null)
            {
                return RedirectToAction("ConfiguracionSucursales");
            }

            // Obtener la sucursal
            var sucursal = await _sucursalService.ObtenerSucursalPorIdAsync(sucursalId.Value);
            if (sucursal == null)
            {
                TempData["Error"] = "Sucursal no encontrada";
                return RedirectToAction("ConfiguracionSucursales");
            }

            // Obtener los vendedores asignados a esta sucursal
            var vendedoresDeSucursal = await _sucursalService.ObtenerVendedoresDeSucursalAsync(sucursalId.Value);

            // Obtener las asignaciones
            var asignaciones = await _context.UsuariosSucursales
                .Include(us => us.Usuario)
                .Where(us => us.SucursalId == sucursalId.Value)
                .ToListAsync();

            // Obtener todos los vendedores disponibles (que no estén ya asignados a esta sucursal)
            var vendedoresDisponibles = await _usuarioService.ObtenerUsuariosPorRolAsync("Vendedor");
            
            // Filtrar los vendedores que no están asignados a esta sucursal
            var vendedoresNoAsignados = vendedoresDisponibles
                .Where(v => !asignaciones.Any(a => a.UsuarioId == v.Id))
                .OrderBy(v => v.Nombre)
                .ToList();

            // Preparar el modelo para la vista
            var viewModel = new AdministrarVendedoresViewModel
            {
                Sucursal = sucursal,
                VendedoresAsignados = asignaciones,
                VendedoresDisponibles = vendedoresNoAsignados
            };

            return View(viewModel);
        }

        // POST: Admin/AsignarVendedor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarVendedor(string vendedorId, int sucursalId, bool esPrincipal = false)
        {
            try
            {
                if (string.IsNullOrEmpty(vendedorId) || sucursalId <= 0)
                {
                    TempData["Error"] = "Datos inválidos para asignar vendedor";
                    return RedirectToAction("AdministrarVendedoresSucursal", new { sucursalId });
                }

                // Verificar que la sucursal existe
                var sucursal = await _sucursalService.ObtenerSucursalPorIdAsync(sucursalId);
                if (sucursal == null)
                {
                    TempData["Error"] = "Sucursal no encontrada";
                    return RedirectToAction("ConfiguracionSucursales");
                }

                // Verificar que el vendedor existe
                var vendedor = await _usuarioService.ObtenerUsuarioPorIdAsync(vendedorId);
                if (vendedor == null)
                {
                    TempData["Error"] = "Vendedor no encontrado";
                    return RedirectToAction("AdministrarVendedoresSucursal", new { sucursalId });
                }

                // Verificar que el vendedor no está ya asignado a esta sucursal
                bool yaAsignado = await _context.UsuariosSucursales
                    .AnyAsync(us => us.UsuarioId == vendedorId && us.SucursalId == sucursalId);

                if (yaAsignado)
                {
                    TempData["Info"] = "El vendedor ya está asignado a esta sucursal";
                    return RedirectToAction("AdministrarVendedoresSucursal", new { sucursalId });
                }

                // Asignar el vendedor a la sucursal
                bool resultado = await _sucursalService.AsignarVendedorASucursalAsync(vendedorId, sucursalId, esPrincipal);

                if (resultado)
                {
                    TempData["Exito"] = $"Vendedor {vendedor.Nombre} asignado correctamente a la sucursal {sucursal.Nombre}";
                }
                else
                {
                    TempData["Error"] = "No se pudo asignar el vendedor a la sucursal";
                }

                return RedirectToAction("AdministrarVendedoresSucursal", new { sucursalId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al asignar vendedor: " + ex.Message;
                return RedirectToAction("AdministrarVendedoresSucursal", new { sucursalId });
            }
        }

        // POST: Admin/DesasignarVendedor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesasignarVendedor(int asignacionId, int sucursalId)
        {
            try
            {
                if (asignacionId <= 0 || sucursalId <= 0)
                {
                    TempData["Error"] = "Datos inválidos para desasignar vendedor";
                    return RedirectToAction("AdministrarVendedoresSucursal", new { sucursalId });
                }

                // Asegurarse de que la sucursal existe
                var sucursal = await _sucursalService.ObtenerSucursalPorIdAsync(sucursalId);
                if (sucursal == null)
                {
                    TempData["Error"] = "Sucursal no encontrada";
                    return RedirectToAction("ConfiguracionSucursales");
                }

                // Intentar desasignar el vendedor
                bool resultado = await _sucursalService.DesasignarVendedorDeSucursalAsync(asignacionId);

                if (resultado)
                {
                    TempData["Exito"] = "Vendedor desasignado correctamente de la sucursal";
                }
                else
                {
                    TempData["Error"] = "No se pudo desasignar el vendedor de la sucursal";
                }

                return RedirectToAction("AdministrarVendedoresSucursal", new { sucursalId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al desasignar vendedor: " + ex.Message;
                return RedirectToAction("AdministrarVendedoresSucursal", new { sucursalId });
            }
        }
    }
}
    