using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.AspNetCore.Identity;
using Autos.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Autos.Data.Migrations
{
    public partial class SeedData : Migration
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedData(UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Semillas para usuarios, roles, sucursales, etc.
            // Las semillas se ejecutarán en el método OnModelCreating del ApplicationDbContext
            
            // Esta migración es para documentación, la inserción real se hará mediante programación
            // al iniciar la aplicación en el Program.cs
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Método para revertir las semillas en caso de rollback
            // No implementado porque no es fácil revertir inserciones en Identity
        }

        /// <summary>
        /// Método auxiliar para ser llamado desde Program.cs y hacer seed de datos iniciales
        /// </summary>
        public static async Task SeedDataAsync(ApplicationDbContext context, UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Si ya existen sucursales, asumimos que la inicialización ya se hizo
            if (await context.Sucursales.AnyAsync())
            {
                return;
            }

            // Asegurarse de que los roles existan
            string[] roles = { "Administrador", "Recepcionista", "Vendedor", "Gerente", "Cliente" };
            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            // Crear gerentes para las sucursales
            var gerentes = new List<Usuario>
            {
                new Usuario
                {
                    UserName = "gerente.sanjose@autoscr.com",
                    Email = "gerente.sanjose@autoscr.com",
                    Nombre = "Rodrigo Hernández",
                    Rol = "Gerente",
                    Identificacion = "108450123",
                    Direccion = "San José, Costa Rica",
                    FechaRegistro = DateTime.Now,
                    EmailConfirmed = true
                },
                new Usuario
                {
                    UserName = "gerente.cartago@autoscr.com",
                    Email = "gerente.cartago@autoscr.com",
                    Nombre = "Ana Jiménez",
                    Rol = "Gerente",
                    Identificacion = "207890345",
                    Direccion = "Cartago, Costa Rica",
                    FechaRegistro = DateTime.Now,
                    EmailConfirmed = true
                },
                new Usuario
                {
                    UserName = "gerente.heredia@autoscr.com",
                    Email = "gerente.heredia@autoscr.com",
                    Nombre = "Carlos Mora",
                    Rol = "Gerente",
                    Identificacion = "305670234",
                    Direccion = "Heredia, Costa Rica",
                    FechaRegistro = DateTime.Now,
                    EmailConfirmed = true
                },
                new Usuario
                {
                    UserName = "gerente.liberia@autoscr.com",
                    Email = "gerente.liberia@autoscr.com",
                    Nombre = "Mariana Cordero",
                    Rol = "Gerente",
                    Identificacion = "504320987",
                    Direccion = "Liberia, Guanacaste, Costa Rica",
                    FechaRegistro = DateTime.Now,
                    EmailConfirmed = true
                },
                new Usuario
                {
                    UserName = "gerente.limon@autoscr.com",
                    Email = "gerente.limon@autoscr.com",
                    Nombre = "Daniel Cascante",
                    Rol = "Gerente",
                    Identificacion = "702150678",
                    Direccion = "Limón, Costa Rica",
                    FechaRegistro = DateTime.Now,
                    EmailConfirmed = true
                }
            };

            // Contraseña para todos los gerentes
            string password = "Gerente123*";
            var gerenteIds = new List<string>();

            foreach (var gerente in gerentes)
            {
                var result = await userManager.CreateAsync(gerente, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(gerente, "Gerente");
                    gerenteIds.Add(gerente.Id);
                }
            }

            // Crear recepcionistas para las sucursales
            var recepcionistas = new List<Usuario>
            {
                new Usuario
                {
                    UserName = "recepcion.sanjose@autoscr.com",
                    Email = "recepcion.sanjose@autoscr.com",
                    Nombre = "María Fernández",
                    Rol = "Recepcionista",
                    Identificacion = "115430789",
                    Direccion = "San José, Costa Rica",
                    FechaRegistro = DateTime.Now,
                    EmailConfirmed = true
                },
                new Usuario
                {
                    UserName = "recepcion.cartago@autoscr.com",
                    Email = "recepcion.cartago@autoscr.com",
                    Nombre = "José Campos",
                    Rol = "Recepcionista",
                    Identificacion = "215340978",
                    Direccion = "Cartago, Costa Rica",
                    FechaRegistro = DateTime.Now,
                    EmailConfirmed = true
                },
                new Usuario
                {
                    UserName = "recepcion.heredia@autoscr.com",
                    Email = "recepcion.heredia@autoscr.com",
                    Nombre = "Laura Vargas",
                    Rol = "Recepcionista",
                    Identificacion = "315670234",
                    Direccion = "Heredia, Costa Rica",
                    FechaRegistro = DateTime.Now,
                    EmailConfirmed = true
                },
                new Usuario
                {
                    UserName = "recepcion.liberia@autoscr.com",
                    Email = "recepcion.liberia@autoscr.com",
                    Nombre = "Gabriel Solano",
                    Rol = "Recepcionista",
                    Identificacion = "514320987",
                    Direccion = "Liberia, Guanacaste, Costa Rica",
                    FechaRegistro = DateTime.Now,
                    EmailConfirmed = true
                },
                new Usuario
                {
                    UserName = "recepcion.limon@autoscr.com",
                    Email = "recepcion.limon@autoscr.com",
                    Nombre = "Carla Mejías",
                    Rol = "Recepcionista",
                    Identificacion = "712150678",
                    Direccion = "Limón, Costa Rica",
                    FechaRegistro = DateTime.Now,
                    EmailConfirmed = true
                }
            };

            // Contraseña para todos los recepcionistas
            string passwordRecepcionista = "Recepcion123*";
            var recepcionistaIds = new List<string>();

            foreach (var recepcionista in recepcionistas)
            {
                var result = await userManager.CreateAsync(recepcionista, passwordRecepcionista);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(recepcionista, "Recepcionista");
                    recepcionistaIds.Add(recepcionista.Id);
                }
            }

            // Crear algunos vendedores de prueba
            var vendedores = new List<Usuario>();
            for (int i = 1; i <= 5; i++)
            {
                var vendedor = new Usuario
                {
                    UserName = $"vendedor{i}@autoscr.com",
                    Email = $"vendedor{i}@autoscr.com",
                    Nombre = $"Vendedor Prueba {i}",
                    Identificacion = $"V{i.ToString().PadLeft(8, '0')}",
                    Direccion = "Dirección de prueba",
                    Rol = "Vendedor",
                    EmailConfirmed = true,
                    FechaRegistro = DateTime.Now
                };
                vendedores.Add(vendedor);

                var result = await userManager.CreateAsync(vendedor, "Vendedor123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(vendedor, "Vendedor");
                }
            }

            // Crear sucursales y asignar gerentes
            var sucursales = new List<Sucursal>
            {
                new Sucursal
                {
                    Nombre = "Automotriz San José",
                    Direccion = "Avenida Central, Paseo Colón, San José, Costa Rica",
                    GerenteId = gerenteIds[0],
                    Telefono = "+506 2222-1111",
                    HorarioAtencion = "Lunes a Viernes: 8:00 AM - 6:00 PM, Sábados: 9:00 AM - 2:00 PM",
                    Activa = true
                },
                new Sucursal
                {
                    Nombre = "Automotriz Cartago",
                    Direccion = "Avenida 6, Calle 2, Cartago, Costa Rica",
                    GerenteId = gerenteIds[1],
                    Telefono = "+506 2550-2222",
                    HorarioAtencion = "Lunes a Viernes: 8:30 AM - 5:30 PM, Sábados: 9:00 AM - 1:00 PM",
                    Activa = true
                },
                new Sucursal
                {
                    Nombre = "Automotriz Heredia",
                    Direccion = "Avenida 3, Calle Central, Heredia, Costa Rica",
                    GerenteId = gerenteIds[2],
                    Telefono = "+506 2237-3333",
                    HorarioAtencion = "Lunes a Viernes: 8:00 AM - 6:00 PM, Sábados: 8:00 AM - 3:00 PM",
                    Activa = true
                },
                new Sucursal
                {
                    Nombre = "Automotriz Guanacaste",
                    Direccion = "Carretera a Playas del Coco, Liberia, Guanacaste, Costa Rica",
                    GerenteId = gerenteIds[3],
                    Telefono = "+506 2665-4444",
                    HorarioAtencion = "Lunes a Viernes: 8:00 AM - 5:00 PM, Sábados: 9:00 AM - 12:00 PM",
                    Activa = true
                },
                new Sucursal
                {
                    Nombre = "Automotriz Limón",
                    Direccion = "Avenida Principal, Limón Centro, Costa Rica",
                    GerenteId = gerenteIds[4],
                    Telefono = "+506 2798-5555",
                    HorarioAtencion = "Lunes a Viernes: 8:30 AM - 5:30 PM, Sábados: 9:00 AM - 1:00 PM",
                    Activa = true
                }
            };

            context.Sucursales.AddRange(sucursales);
            await context.SaveChangesAsync();
            
            // Asignar vendedores a sucursales
            var todasSucursales = await context.Sucursales.ToListAsync();
            var todosVendedores = await userManager.GetUsersInRoleAsync("Vendedor");
            var todosRecepcionistas = await userManager.GetUsersInRoleAsync("Recepcionista");
            
            // Asignar recepcionistas a cada sucursal
            for (int i = 0; i < Math.Min(todasSucursales.Count, todosRecepcionistas.Count); i++)
            {
                var recepcionistaActual = todosRecepcionistas.ElementAt(i);
                
                // Encontrar la sucursal correcta basada en el email del recepcionista
                Sucursal sucursalCorrecta;
                
                if (recepcionistaActual.Email?.Contains("sanjose") == true)
                {
                    sucursalCorrecta = todasSucursales.FirstOrDefault(s => s.Nombre.Contains("San José")) 
                        ?? todasSucursales.ElementAt(0);
                }
                else if (recepcionistaActual.Email?.Contains("cartago") == true)
                {
                    sucursalCorrecta = todasSucursales.FirstOrDefault(s => s.Nombre.Contains("Cartago")) 
                        ?? todasSucursales.ElementAt(1);
                }
                else if (recepcionistaActual.Email?.Contains("heredia") == true)
                {
                    sucursalCorrecta = todasSucursales.FirstOrDefault(s => s.Nombre.Contains("Heredia")) 
                        ?? todasSucursales.ElementAt(2);
                }
                else if (recepcionistaActual.Email?.Contains("liberia") == true)
                {
                    sucursalCorrecta = todasSucursales.FirstOrDefault(s => s.Nombre.Contains("Guanacaste")) 
                        ?? todasSucursales.ElementAt(3);
                }
                else if (recepcionistaActual.Email?.Contains("limon") == true)
                {
                    sucursalCorrecta = todasSucursales.FirstOrDefault(s => s.Nombre.Contains("Limón")) 
                        ?? todasSucursales.ElementAt(4);
                }
                else
                {
                    // Si no hay coincidencia, usar la sucursal según el índice
                    sucursalCorrecta = todasSucursales.ElementAt(i % todasSucursales.Count);
                }
                
                context.UsuariosSucursales.Add(new UsuarioSucursal
                {
                    UsuarioId = recepcionistaActual.Id,
                    SucursalId = sucursalCorrecta.Id,
                    FechaAsignacion = DateTime.Now,
                    Activo = true,
                    EsPrincipal = true
                });
            }
            
            // Asignar el primer vendedor a la primera sucursal
            if (todosVendedores.Count > 0 && todasSucursales.Count > 0)
            {
                context.UsuariosSucursales.Add(new UsuarioSucursal
                {
                    UsuarioId = todosVendedores.First().Id,
                    SucursalId = todasSucursales.First().Id,
                    FechaAsignacion = DateTime.Now,
                    Activo = true,
                    EsPrincipal = true
                });
                
                await context.SaveChangesAsync();
            }
            
            // Añadir configuración para descuentos
            if (!await context.DescuentosConfig.AnyAsync())
            {
                context.DescuentosConfig.Add(new DescuentoConfig
                {
                    PorcentajeMaximo = 15 // 15% de descuento máximo
                });
                
                await context.SaveChangesAsync();
            }
            
            // Crear 30 vehículos de prueba para las sucursales
            if (!await context.Autos.AnyAsync())
            {
                var sucursalesRegistradas = await context.Sucursales.ToListAsync();
                var autos = new List<Auto>();
                
                // Sucursal San José
                var sucursalSanJose = sucursalesRegistradas.FirstOrDefault(s => s.Nombre.Contains("San José"));
                if (sucursalSanJose != null)
                {
                    autos.AddRange(new List<Auto>
                    {
                        new Auto { 
                            Marca = "Toyota", 
                            Modelo = "Corolla", 
                            Anio = 2023, 
                            Precio = 18500000, 
                            Kilometraje = 0,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Blanco",
                            Categoria = "Sedán",
                            SucursalId = sucursalSanJose.Id
                        },
                        new Auto { 
                            Marca = "Honda", 
                            Modelo = "CR-V", 
                            Anio = 2023, 
                            Precio = 23500000, 
                            Kilometraje = 0,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Gris",
                            Categoria = "SUV",
                            SucursalId = sucursalSanJose.Id
                        },
                        new Auto { 
                            Marca = "Nissan", 
                            Modelo = "Sentra", 
                            Anio = 2022, 
                            Precio = 15800000, 
                            Kilometraje = 12000,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Azul",
                            Categoria = "Sedán",
                            SucursalId = sucursalSanJose.Id
                        },
                        new Auto { 
                            Marca = "BMW", 
                            Modelo = "X3", 
                            Anio = 2022, 
                            Precio = 32000000, 
                            Kilometraje = 5000,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Negro",
                            Categoria = "SUV Premium",
                            SucursalId = sucursalSanJose.Id
                        },
                        new Auto { 
                            Marca = "Hyundai", 
                            Modelo = "Tucson", 
                            Anio = 2023, 
                            Precio = 21500000, 
                            Kilometraje = 0,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Rojo",
                            Categoria = "SUV",
                            SucursalId = sucursalSanJose.Id
                        },
                        new Auto { 
                            Marca = "Kia", 
                            Modelo = "Sportage", 
                            Anio = 2022, 
                            Precio = 20800000, 
                            Kilometraje = 8000,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Blanco",
                            Categoria = "SUV",
                            SucursalId = sucursalSanJose.Id
                        }
                    });
                }
                
                // Sucursal Cartago
                var sucursalCartago = sucursalesRegistradas.FirstOrDefault(s => s.Nombre.Contains("Cartago"));
                if (sucursalCartago != null)
                {
                    autos.AddRange(new List<Auto>
                    {
                        new Auto { 
                            Marca = "Toyota", 
                            Modelo = "Hilux", 
                            Anio = 2023, 
                            Precio = 28500000, 
                            Kilometraje = 0,
                            Transmision = "Manual", 
                            Combustible = "Diésel", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Blanco",
                            Categoria = "Pick-up",
                            SucursalId = sucursalCartago.Id
                        },
                        new Auto { 
                            Marca = "Suzuki", 
                            Modelo = "Swift", 
                            Anio = 2023, 
                            Precio = 12500000, 
                            Kilometraje = 0,
                            Transmision = "Manual", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Amarillo",
                            Categoria = "Hatchback",
                            SucursalId = sucursalCartago.Id
                        },
                        new Auto { 
                            Marca = "Mitsubishi", 
                            Modelo = "Montero Sport", 
                            Anio = 2022, 
                            Precio = 26800000, 
                            Kilometraje = 15000,
                            Transmision = "Automática", 
                            Combustible = "Diésel", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Gris",
                            Categoria = "SUV",
                            SucursalId = sucursalCartago.Id
                        },
                        new Auto { 
                            Marca = "Audi", 
                            Modelo = "A3", 
                            Anio = 2022, 
                            Precio = 27500000, 
                            Kilometraje = 8000,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Blanco",
                            Categoria = "Sedán Premium",
                            SucursalId = sucursalCartago.Id
                        },
                        new Auto { 
                            Marca = "Hyundai", 
                            Modelo = "Accent", 
                            Anio = 2023, 
                            Precio = 16900000, 
                            Kilometraje = 0,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Azul",
                            Categoria = "Sedán",
                            SucursalId = sucursalCartago.Id
                        },
                        new Auto { 
                            Marca = "Ford", 
                            Modelo = "EcoSport", 
                            Anio = 2022, 
                            Precio = 19500000, 
                            Kilometraje = 10000,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Rojo",
                            Categoria = "SUV Compacto",
                            SucursalId = sucursalCartago.Id
                        }
                    });
                }
                
                // Sucursal Heredia
                var sucursalHeredia = sucursalesRegistradas.FirstOrDefault(s => s.Nombre.Contains("Heredia"));
                if (sucursalHeredia != null)
                {
                    autos.AddRange(new List<Auto>
                    {
                        new Auto { 
                            Marca = "Nissan", 
                            Modelo = "Frontier", 
                            Anio = 2022, 
                            Precio = 26800000, 
                            Kilometraje = 12000,
                            Transmision = "Automática", 
                            Combustible = "Diésel", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Negro",
                            Categoria = "Pick-up",
                            SucursalId = sucursalHeredia.Id
                        },
                        new Auto { 
                            Marca = "Mazda", 
                            Modelo = "CX-5", 
                            Anio = 2023, 
                            Precio = 24500000, 
                            Kilometraje = 0,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Rojo",
                            Categoria = "SUV",
                            SucursalId = sucursalHeredia.Id
                        },
                        new Auto { 
                            Marca = "Volkswagen", 
                            Modelo = "Tiguan", 
                            Anio = 2022, 
                            Precio = 25800000, 
                            Kilometraje = 8000,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Plata",
                            Categoria = "SUV",
                            SucursalId = sucursalHeredia.Id
                        },
                        new Auto { 
                            Marca = "Mercedes-Benz", 
                            Modelo = "GLA 200", 
                            Anio = 2022, 
                            Precio = 33000000, 
                            Kilometraje = 6000,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Negro",
                            Categoria = "SUV Premium",
                            SucursalId = sucursalHeredia.Id
                        },
                        new Auto { 
                            Marca = "Hyundai", 
                            Modelo = "Elantra", 
                            Anio = 2023, 
                            Precio = 17800000, 
                            Kilometraje = 0,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Gris",
                            Categoria = "Sedán",
                            SucursalId = sucursalHeredia.Id
                        },
                        new Auto { 
                            Marca = "Honda", 
                            Modelo = "HR-V", 
                            Anio = 2022, 
                            Precio = 22500000, 
                            Kilometraje = 9000,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Azul",
                            Categoria = "SUV Compacto",
                            SucursalId = sucursalHeredia.Id
                        }
                    });
                }
                
                // Sucursal Guanacaste
                var sucursalGuanacaste = sucursalesRegistradas.FirstOrDefault(s => s.Nombre.Contains("Guanacaste"));
                if (sucursalGuanacaste != null)
                {
                    autos.AddRange(new List<Auto>
                    {
                        new Auto { 
                            Marca = "Toyota", 
                            Modelo = "RAV4", 
                            Anio = 2023, 
                            Precio = 25800000, 
                            Kilometraje = 0,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Verde",
                            Categoria = "SUV",
                            SucursalId = sucursalGuanacaste.Id
                        },
                        new Auto { 
                            Marca = "Ford", 
                            Modelo = "Ranger", 
                            Anio = 2022, 
                            Precio = 27500000, 
                            Kilometraje = 15000,
                            Transmision = "Automática", 
                            Combustible = "Diésel", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Blanco",
                            Categoria = "Pick-up",
                            SucursalId = sucursalGuanacaste.Id
                        },
                        new Auto { 
                            Marca = "Suzuki", 
                            Modelo = "Jimny", 
                            Anio = 2023, 
                            Precio = 18500000, 
                            Kilometraje = 0,
                            Transmision = "Manual", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Verde",
                            Categoria = "4x4",
                            SucursalId = sucursalGuanacaste.Id
                        },
                        new Auto { 
                            Marca = "Isuzu", 
                            Modelo = "D-Max", 
                            Anio = 2022, 
                            Precio = 24800000, 
                            Kilometraje = 12000,
                            Transmision = "Manual", 
                            Combustible = "Diésel", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Blanco",
                            Categoria = "Pick-up",
                            SucursalId = sucursalGuanacaste.Id
                        },
                        new Auto { 
                            Marca = "Kia", 
                            Modelo = "Seltos", 
                            Anio = 2023, 
                            Precio = 21500000, 
                            Kilometraje = 0,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Naranja",
                            Categoria = "SUV Compacto",
                            SucursalId = sucursalGuanacaste.Id
                        },
                        new Auto { 
                            Marca = "Subaru", 
                            Modelo = "Forester", 
                            Anio = 2022, 
                            Precio = 26500000, 
                            Kilometraje = 7000,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Azul",
                            Categoria = "SUV",
                            SucursalId = sucursalGuanacaste.Id
                        }
                    });
                }
                
                // Sucursal Limón
                var sucursalLimon = sucursalesRegistradas.FirstOrDefault(s => s.Nombre.Contains("Limón"));
                if (sucursalLimon != null)
                {
                    autos.AddRange(new List<Auto>
                    {
                        new Auto { 
                            Marca = "Toyota", 
                            Modelo = "Land Cruiser Prado", 
                            Anio = 2022, 
                            Precio = 42800000, 
                            Kilometraje = 5000,
                            Transmision = "Automática", 
                            Combustible = "Diésel", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Negro",
                            Categoria = "SUV Premium",
                            SucursalId = sucursalLimon.Id
                        },
                        new Auto { 
                            Marca = "Mitsubishi", 
                            Modelo = "L200", 
                            Anio = 2023, 
                            Precio = 26900000, 
                            Kilometraje = 0,
                            Transmision = "Automática", 
                            Combustible = "Diésel", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Gris",
                            Categoria = "Pick-up",
                            SucursalId = sucursalLimon.Id
                        },
                        new Auto { 
                            Marca = "Nissan", 
                            Modelo = "X-Trail", 
                            Anio = 2022, 
                            Precio = 24500000, 
                            Kilometraje = 10000,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Azul",
                            Categoria = "SUV",
                            SucursalId = sucursalLimon.Id
                        },
                        new Auto { 
                            Marca = "Great Wall", 
                            Modelo = "Wingle 7", 
                            Anio = 2023, 
                            Precio = 21500000, 
                            Kilometraje = 0,
                            Transmision = "Manual", 
                            Combustible = "Diésel", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Blanco",
                            Categoria = "Pick-up",
                            SucursalId = sucursalLimon.Id
                        },
                        new Auto { 
                            Marca = "Renault", 
                            Modelo = "Duster", 
                            Anio = 2022, 
                            Precio = 18500000, 
                            Kilometraje = 12000,
                            Transmision = "Manual", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Rojo",
                            Categoria = "SUV Compacto",
                            SucursalId = sucursalLimon.Id
                        },
                        new Auto { 
                            Marca = "Chevrolet", 
                            Modelo = "Traverse", 
                            Anio = 2022, 
                            Precio = 32500000, 
                            Kilometraje = 8000,
                            Transmision = "Automática", 
                            Combustible = "Gasolina", 
                            Disponibilidad = true,
                            EstadoReserva = "Disponible",
                            Color = "Negro",
                            Categoria = "SUV Grande",
                            SucursalId = sucursalLimon.Id
                        }
                    });
                }
                
                context.Autos.AddRange(autos);
                await context.SaveChangesAsync();
            }
        }
    }
} 