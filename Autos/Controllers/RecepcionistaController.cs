using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Autos.Data;
using Autos.Models;
using Autos.Models.ViewModels;
using Autos.Services.Interfaces;

namespace Autos.Controllers
{
    [Authorize(Roles = "Recepcionista")]
    public class RecepcionistaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IUsuarioService _usuarioService;
        private readonly ISucursalService _sucursalService;
        private readonly IAutoService _autoService;
        private readonly IReservaService _reservaService;

        public RecepcionistaController(
            ApplicationDbContext context,
            UserManager<Usuario> userManager,
            IUsuarioService usuarioService,
            ISucursalService sucursalService,
            IAutoService autoService,
            IReservaService reservaService)
        {
            _context = context;
            _userManager = userManager;
            _usuarioService = usuarioService;
            _sucursalService = sucursalService;
            _autoService = autoService;
            _reservaService = reservaService;
        }

        // GET: Recepcionista/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Mensaje de diagnóstico inicial
                Console.WriteLine("Iniciando carga del Dashboard de Recepcionista");
                TempData["Info"] = "Dashboard de Recepcionista cargado correctamente. " + DateTime.Now.ToString();
                
                // Obtener la recepcionista actual
                var recepcionista = await _usuarioService.ObtenerUsuarioActualAsync();
                Console.WriteLine($"Usuario obtenido: {(recepcionista == null ? "null" : recepcionista.Email)}");
                
                if (recepcionista == null)
                {
                    Console.WriteLine("Error: Usuario no encontrado");
                    TempData["Error"] = "No se pudo encontrar el usuario en el sistema.";
                    return NotFound();
                }

                // Verificar el rol del usuario
                var roles = await _usuarioService.ObtenerRolesDeUsuarioAsync(recepcionista);
                Console.WriteLine($"Roles del usuario: {string.Join(", ", roles)}");
                
                if (!roles.Contains("Recepcionista"))
                {
                    Console.WriteLine("Error: Usuario no tiene rol de Recepcionista");
                    TempData["Error"] = "No tienes permisos de recepcionista.";
                    return RedirectToAction("Index", "Home");
                }

                // Obtener la sucursal de la recepcionista
                Console.WriteLine($"Buscando asignación de sucursal para usuario ID: {recepcionista.Id}");
                var asignacionSucursal = await _sucursalService.ObtenerAsignacionUsuarioSucursalAsync(recepcionista.Id);

                Console.WriteLine($"Asignación de sucursal encontrada: {(asignacionSucursal == null ? "null" : "encontrada")}");
                Console.WriteLine($"Sucursal en asignación: {(asignacionSucursal?.Sucursal == null ? "null" : asignacionSucursal.Sucursal.Nombre)}");

                if (asignacionSucursal == null || asignacionSucursal.Sucursal == null)
                {
                    Console.WriteLine("Error: No hay asignación de sucursal activa");
                    TempData["Error"] = "No estás asignado a ninguna sucursal activa.";
                    return View(new RecepcionistaDashboardViewModel { Recepcionista = recepcionista });
                }

                // Verificar si la sucursal está activa
                if (!asignacionSucursal.Sucursal.Activa)
                {
                    Console.WriteLine($"Error: Sucursal {asignacionSucursal.Sucursal.Nombre} no está activa");
                    TempData["Error"] = "La sucursal asignada no está activa.";
                    return View(new RecepcionistaDashboardViewModel { Recepcionista = recepcionista });
                }

                var sucursal = asignacionSucursal.Sucursal;
                Console.WriteLine($"Sucursal obtenida correctamente: {sucursal.Nombre}");

                // Preparar el modelo para la vista con información básica
                var viewModel = new RecepcionistaDashboardViewModel
                {
                    Recepcionista = recepcionista,
                    Sucursal = sucursal,
                    AutosDisponibles = 0,
                    VendedoresTotales = 0,
                    ClientesRegistradosHoy = 0,
                    ReservasHoy = 0,
                    AutosRecientes = new List<Auto>(),
                    Vendedores = new List<Usuario>()
                };

                // Obtener estadísticas
                var autosDisponibles = await _autoService.ObtenerAutosDisponiblesAsync();
                viewModel.AutosDisponibles = autosDisponibles.Count(a => a.SucursalId == sucursal.Id);

                // Usar _userManager.Users en lugar de _context.Users para mantener consistencia
                var vendedoresIds = await _context.UsuariosSucursales
                    .Where(us => us.SucursalId == sucursal.Id && us.Activo)
                    .Select(us => us.UsuarioId)
                    .ToListAsync();

                viewModel.VendedoresTotales = await _userManager.Users
                    .Where(u => vendedoresIds.Contains(u.Id) && u.Rol == "Vendedor")
                    .CountAsync();

                // Reemplazar con el siguiente código
                // Obtener vendedores de la sucursal usando el servicio
                var vendedores = await _sucursalService.ObtenerVendedoresDeSucursalAsync(sucursal.Id);
                viewModel.VendedoresTotales = vendedores.Count();

                viewModel.ClientesRegistradosHoy = await _userManager.Users
                    .Where(u => u.Rol == "Cliente" && u.FechaRegistro.HasValue && 
                           u.FechaRegistro.Value.Date == DateTime.Today)
                    .CountAsync();

                viewModel.ReservasHoy = await _context.Reservas
                    .Include(r => r.Auto)
                    .Where(r => r.Auto != null && r.Auto.SucursalId == sucursal.Id && 
                           r.FechaReserva.Date == DateTime.Today)
                    .CountAsync();

                // Obtener los autos disponibles de la sucursal
                viewModel.AutosRecientes = await _context.Autos
                    .Where(a => a.SucursalId == sucursal.Id && a.Disponibilidad)
                    .OrderByDescending(a => a.Id)
                    .Take(5)
                    .ToListAsync();

                // Obtener los vendedores de la sucursal
                var vendedoresSucursal = await _context.UsuariosSucursales
                    .Where(us => us.SucursalId == sucursal.Id && us.Activo)
                    .Select(us => us.UsuarioId)
                    .ToListAsync();

                viewModel.Vendedores = await _userManager.Users
                    .Where(u => vendedoresSucursal.Contains(u.Id) && u.Rol == "Vendedor")
                    .ToListAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Registrar el error
                Console.WriteLine($"Error en Dashboard de Recepcionista: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                // Proporcionar datos básicos para evitar errores en la vista
                var recepcionista = await _userManager.GetUserAsync(User);
                var viewModel = new RecepcionistaDashboardViewModel
                {
                    Recepcionista = recepcionista,
                    AutosDisponibles = 0,
                    VendedoresTotales = 0,
                    ClientesRegistradosHoy = 0,
                    ReservasHoy = 0,
                    AutosRecientes = new List<Auto>(),
                    Vendedores = new List<Usuario>()
                };
                
                TempData["Error"] = "Ha ocurrido un error al cargar el dashboard. Por favor, contacte al administrador del sistema.";
                return View(viewModel);
            }
        }

        // GET: Recepcionista/CatalogoAutos
        public IActionResult CatalogoAutos()
        {
            // Redirigir al catálogo general de autos
            return RedirectToAction("Index", "Autos");
        }

        // GET: Recepcionista/ReservarAuto/5
        public IActionResult ReservarAuto(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Redirigir a la acción Reservar del controlador de Autos
            return RedirectToAction("Reservar", "Autos", new { id = id });
        }

        // GET: Recepcionista/BuscarClientes
        public IActionResult BuscarClientes()
        {
            return View();
        }

        // POST: Recepcionista/BuscarClientes
        [HttpPost]
        [ValidateAntiForgeryToken(Order = 1000)]
        [AllowAnonymous]
        public async Task<IActionResult> BuscarClientes(string termino)
        {
            try
            {
                Console.WriteLine($"BuscarClientes - Término recibido: {termino}");
                
                if (string.IsNullOrWhiteSpace(termino))
                {
                    Console.WriteLine("BuscarClientes - Término vacío, retornando lista vacía");
                    return Json(new List<object>());
                }

                // Obtener la sucursal de la recepcionista actual
                var sucursal = await ObtenerSucursalRecepcionista();
                if (sucursal == null)
                {
                    return Json(new { error = "No estás asignado a ninguna sucursal." });
                }

                // Obtener los vendedores de la sucursal
                var vendedores = await _sucursalService.ObtenerVendedoresDeSucursalAsync(sucursal.Id);
                var vendedoresIds = vendedores.Select(v => v.Id).ToList();
                
                // Crear un diccionario de vendedores para obtener sus nombres
                var vendedoresDict = vendedores.ToDictionary(v => v.Id, v => v.Nombre);

                // Obtener todos los clientes
                var todosLosClientes = await _usuarioService.ObtenerUsuariosPorRolAsync("Cliente");
                
                // Filtrar clientes por término de búsqueda y asignación a vendedores de esta sucursal
                var clientes = todosLosClientes
                    .Where(u => (u.Nombre?.Contains(termino) == true ||
                                 u.Email?.Contains(termino) == true ||
                                 u.Identificacion?.Contains(termino) == true) &&
                                u.VendedorAsignadoId != null && 
                                vendedoresIds.Contains(u.VendedorAsignadoId))
                    .Select(c => new
                    {
                        id = c.Id,
                        nombre = c.Nombre,
                        email = c.Email ?? string.Empty,
                        identificacion = c.Identificacion ?? string.Empty,
                        vendedorId = c.VendedorAsignadoId ?? string.Empty,
                        vendedorNombre = c.VendedorAsignadoId != null && vendedoresDict.ContainsKey(c.VendedorAsignadoId) 
                            ? vendedoresDict[c.VendedorAsignadoId] 
                            : "Sin vendedor asignado"
                    })
                    .Take(10)
                    .ToList();
                    
                Console.WriteLine($"BuscarClientes - Clientes encontrados en sucursal {sucursal.Nombre}: {clientes.Count}");
                return Json(clientes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BuscarClientes - Error: {ex.Message}");
                return Json(new { error = "Error en la búsqueda", message = ex.Message });
            }
        }

        // POST: Recepcionista/BuscarClientesForm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuscarClientesForm(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino) || termino.Length < 2)
            {
                TempData["Error"] = "Debe ingresar al menos 2 caracteres para buscar.";
                return View("BuscarClientes");
            }

            // Obtener la sucursal de la recepcionista actual
            var sucursal = await ObtenerSucursalRecepcionista();
            if (sucursal == null)
            {
                TempData["Error"] = "No estás asignado a ninguna sucursal.";
                return View("BuscarClientes");
            }

            // Obtener los vendedores de la sucursal
            var vendedores = await _sucursalService.ObtenerVendedoresDeSucursalAsync(sucursal.Id);
            var vendedoresIds = vendedores.Select(v => v.Id).ToList();
            
            // Crear un diccionario de vendedores para obtener sus nombres
            var vendedoresDict = vendedores.ToDictionary(v => v.Id, v => v.Nombre);
            
            ViewBag.NombresVendedores = vendedoresDict;
            ViewBag.Sucursal = sucursal.Nombre;

            // Obtener todos los clientes
            var todosLosClientes = await _usuarioService.ObtenerUsuariosPorRolAsync("Cliente");
            
            // Filtrar clientes por término de búsqueda y asignación a vendedores de esta sucursal
            var clientes = todosLosClientes
                .Where(u => (u.Nombre?.Contains(termino) == true ||
                             u.Email?.Contains(termino) == true ||
                             u.Identificacion?.Contains(termino) == true) &&
                            u.VendedorAsignadoId != null && 
                            vendedoresIds.Contains(u.VendedorAsignadoId))
                .Take(20)
                .ToList();

            ViewBag.ClientesEncontrados = clientes;
            ViewBag.TerminoBusqueda = termino;
            
            if (!clientes.Any())
            {
                TempData["Info"] = $"No se encontraron clientes en la sucursal {sucursal.Nombre} con el término de búsqueda indicado.";
            }
            
            return View("BuscarClientes");
        }

        // GET: Recepcionista/RegistrarCliente
        public IActionResult RegistrarCliente(int? autoId)
        {
            // Redirigir a la acción CrearUsuario del AccountController
            if (autoId.HasValue)
            {
                return RedirectToAction("CrearUsuario", "Account", new { autoId = autoId });
            }
            return RedirectToAction("CrearUsuario", "Account");
        }

        // GET: Recepcionista/VendedoresClientes
        public async Task<IActionResult> VendedoresClientes()
        {
            // Obtener la sucursal de la recepcionista
            var sucursal = await ObtenerSucursalRecepcionista();
            if (sucursal == null)
            {
                TempData["Error"] = "No estás asignado a ninguna sucursal.";
                return RedirectToAction("Dashboard");
            }
            
            // Crear el modelo
            var viewModel = new VendedoresClientesViewModel
            {
                Sucursal = sucursal
            };
            
            // Obtener los vendedores de la sucursal
            var vendedores = await _sucursalService.ObtenerVendedoresDeSucursalAsync(sucursal.Id);
            
            // Para cada vendedor, obtener sus clientes asignados
            foreach (var vendedor in vendedores)
            {
                var vendedorConClientes = new VendedorConClientes
                {
                    Vendedor = vendedor
                };
                
                // Obtener clientes asignados al vendedor
                var clientes = await _usuarioService.ObtenerUsuariosPorRolAsync("Cliente");
                vendedorConClientes.Clientes = clientes
                    .Where(u => u.VendedorAsignadoId == vendedor.Id)
                    .ToList();
                
                viewModel.Vendedores.Add(vendedorConClientes);
            }
            
            return View(viewModel);
        }
        
        // GET: Recepcionista/DetalleVendedor/id
        public async Task<IActionResult> DetalleVendedor(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            
            // Obtener la sucursal de la recepcionista
            var sucursal = await ObtenerSucursalRecepcionista();
            if (sucursal == null)
            {
                TempData["Error"] = "No estás asignado a ninguna sucursal.";
                return RedirectToAction("Dashboard");
            }
            
            // Verificar que el vendedor pertenece a la sucursal de la recepcionista
            var asignaciones = await _sucursalService.ObtenerAsignacionesDeSucursalAsync(sucursal.Id);
            var vendedorAsignado = asignaciones.Any(us => us.UsuarioId == id && us.Activo);
                
            if (!vendedorAsignado)
            {
                TempData["Error"] = "El vendedor no pertenece a tu sucursal.";
                return RedirectToAction("VendedoresClientes");
            }
            
            // Obtener el vendedor y sus clientes
            var vendedor = await _usuarioService.ObtenerUsuarioPorIdAsync(id);
            if (vendedor == null)
            {
                return NotFound("Vendedor no encontrado");
            }
            
            // Verificar que es un vendedor
            var roles = await _usuarioService.ObtenerRolesDeUsuarioAsync(vendedor);
            if (!roles.Contains("Vendedor"))
            {
                return NotFound("El usuario no es un vendedor");
            }
            
            var clientes = await _usuarioService.ObtenerUsuariosPorRolAsync("Cliente");
            var clientesDelVendedor = clientes.Where(u => u.VendedorAsignadoId == vendedor.Id).ToList();
            
            var vendedorConClientes = new VendedorConClientes
            {
                Vendedor = vendedor,
                Clientes = clientesDelVendedor
            };
            
            return View(vendedorConClientes);
        }

        // GET: Recepcionista/Diagnostico
        [AllowAnonymous]
        public IActionResult Diagnostico()
        {
            return Content("El controlador RecepcionistaController está funcionando correctamente.");
        }

        // Métodos privados para reducir duplicación de código

        /// <summary>
        /// Obtiene la sucursal asignada al usuario actual (recepcionista)
        /// </summary>
        private async Task<Sucursal?> ObtenerSucursalRecepcionista()
        {
            try
            {
                // Obtener el usuario recepcionista actual
                var recepcionista = await _usuarioService.ObtenerUsuarioActualAsync();
                if (recepcionista == null)
                {
                    return null;
                }

                // Obtener la asignación de sucursal
                var asignacionSucursal = await _sucursalService.ObtenerAsignacionUsuarioSucursalAsync(recepcionista.Id);
                return asignacionSucursal?.Sucursal;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener sucursal: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene los vendedores asignados a una sucursal específica
        /// </summary>
        private async Task<List<Usuario>> ObtenerVendedoresDeSucursal(int sucursalId)
        {
            var vendedores = await _sucursalService.ObtenerVendedoresDeSucursalAsync(sucursalId);
            return vendedores.ToList();
        }

        /// <summary>
        /// Recarga los datos necesarios para la vista de reservar auto
        /// </summary>
        private async Task RecargarDatosParaVistaReservarAuto(ReservarAutoViewModel model)
        {
            if (model.Auto != null && model.Auto.Id > 0)
            {
                var auto = await _autoService.ObtenerAutoPorIdAsync(model.Auto.Id);
                
                if (auto != null)
                {
                    model.Auto = auto;
                    
                    // Obtener la sucursal si no está incluida
                    if (auto.Sucursal == null && auto.SucursalId > 0)
                    {
                        auto.Sucursal = await _sucursalService.ObtenerSucursalPorIdAsync(auto.SucursalId);
                    }
                    
                    if (auto.Sucursal != null)
                    {
                        model.Vendedores = await ObtenerVendedoresDeSucursal(auto.SucursalId);
                    }
                }
            }
        }

        /// <summary>
        /// Recarga los datos necesarios para la vista de registrar cliente
        /// </summary>
        private async Task RecargarDatosParaVistaRegistrarCliente(RegistrarClienteViewModel model)
        {
            var sucursal = await ObtenerSucursalRecepcionista();
            if (sucursal != null)
            {
                model.Vendedores = await ObtenerVendedoresDeSucursal(sucursal.Id);

                // Si hay un auto seleccionado, recargarlo
                if (model.AutoId.HasValue)
                {
                    model.Auto = await _autoService.ObtenerAutoPorIdAsync(model.AutoId.Value);
                }
            }
            else
            {
                model.Vendedores = new List<Usuario>();
            }
        }
    }
} 