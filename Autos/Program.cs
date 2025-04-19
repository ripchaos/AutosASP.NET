using Autos.Data;
using Autos.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Autos.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Configurar la cultura de Costa Rica para la aplicación
var cultureInfo = new CultureInfo("es-CR");
cultureInfo.NumberFormat.CurrencySymbol = "₡";
cultureInfo.NumberFormat.CurrencyDecimalSeparator = ",";
cultureInfo.NumberFormat.CurrencyGroupSeparator = ".";
cultureInfo.NumberFormat.CurrencyDecimalDigits = 0;

// Establecer la cultura como predeterminada para toda la aplicación
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

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

// Agregar servicios al contenedor.
builder.Services.AddControllersWithViews();

// Registrar el servicio de correo electrónico
builder.Services.AddScoped<IEmailService, EmailService>();

// Registrar el servicio de plantillas de correo electrónico
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();

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

// Inicializar datos de ejemplo si es necesario
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<Usuario>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    
    await Autos.Data.Migrations.SeedData.SeedDataAsync(context, userManager, roleManager);
}

// Agregar datos de prueba si es necesario
if (args.Length > 0 && args[0].ToLower() == "seedvendedores" && int.TryParse(args[1], out int cantidadVendedores))
{
    var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    // Asegurarse de que el rol Vendedor existe
    if (!await roleManager.RoleExistsAsync("Vendedor"))
    {
        await roleManager.CreateAsync(new IdentityRole("Vendedor"));
    }
    
    // Crear vendedores de prueba
    for (int i = 1; i <= cantidadVendedores; i++)
    {
        var email = $"vendedor{i}@autoscr.com";
        var user = await userManager.FindByEmailAsync(email);
        
        if (user == null)
        {
            user = new Usuario
            {
                UserName = email,
                Email = email,
                Nombre = $"Vendedor Prueba {i}",
                Identificacion = $"V{i.ToString().PadLeft(8, '0')}",
                Direccion = "Dirección de prueba",
                Rol = "Vendedor",
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(user, "Vendedor123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Vendedor");
                Console.WriteLine($"Vendedor {i} creado correctamente.");
            }
            else
            {
                Console.WriteLine($"Error al crear vendedor {i}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            Console.WriteLine($"El vendedor {i} ya existe.");
        }
    }
    
    Console.WriteLine($"Proceso de creación de {cantidadVendedores} vendedores completado.");
}

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
