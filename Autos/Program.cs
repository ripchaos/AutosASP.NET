using Autos.Data;
using Autos.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// 📌 Agregar servicios de base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 📌 Agregar servicios de identidad
builder.Services.AddIdentity<Usuario, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 📌 Configuración de cookies (manejo de login y acceso denegado)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";         // Redirige si no está autenticado
    options.AccessDeniedPath = "/Account/AccessDenied"; // Redirige si no tiene permisos
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 📌 Crear roles y administrador antes de ejecutar la app
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await CrearRolesYAdministradorAsync(services);
}

// 📌 Configuración del pipeline de solicitud HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// 📌 Configurar rutas de controladores
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// 📌 Método para crear roles y usuario administrador automáticamente
async Task CrearRolesYAdministradorAsync(IServiceProvider serviceProvider)
{
    try
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // 📌 Lista de roles requeridos
        string[] roles = { "Administrador", "Recepcionista", "Vendedor", "Gerente", "Cliente" };

        // 📌 Verificar y crear roles si no existen
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 📌 Datos del usuario administrador
        string adminEmail = "admin@aspnetautos.com";
        string password = "Admin123*"; // 📌 Cambiar después de la primera ejecución

        // 📌 Verificar si el usuario administrador ya existe
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new Usuario
            {
                UserName = adminEmail,
                Email = adminEmail,
                Nombre = "Administrador",
                Rol = "Administrador"
            };

            var result = await userManager.CreateAsync(adminUser, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrador");
                Console.WriteLine("🚀 Usuario Administrador creado correctamente.");
            }
            else
            {
                Console.WriteLine("❌ Error al crear el Administrador:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"   - {error.Description}");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error crítico al crear roles/administrador: {ex.Message}");
    }
}
