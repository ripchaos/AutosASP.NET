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
        }
    }
} 