using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Autos.Models;
using System.Security.Claims;
using System.Text;

namespace Autos.Controllers
{
    [AllowAnonymous]
    public class DiagnosticoController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DiagnosticoController(
            UserManager<Usuario> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            StringBuilder info = new StringBuilder();
            
            info.AppendLine("<h2>Información de Diagnóstico</h2>");
            
            info.AppendLine("<h3>Estado de autenticación</h3>");
            info.AppendLine($"<p>¿Usuario autenticado? <strong>{User.Identity?.IsAuthenticated}</strong></p>");
            
            if (User.Identity?.IsAuthenticated == true)
            {
                info.AppendLine("<h3>Información de usuario</h3>");
                info.AppendLine($"<p>Nombre: <strong>{User.Identity?.Name}</strong></p>");
                
                info.AppendLine("<h3>Roles</h3>");
                info.AppendLine("<ul>");
                string[] roles = { "Administrador", "Recepcionista", "Vendedor", "Gerente", "Cliente" };
                foreach (var role in roles)
                {
                    info.AppendLine($"<li>¿{role}? <strong>{User.IsInRole(role)}</strong></li>");
                }
                info.AppendLine("</ul>");
                
                info.AppendLine("<h3>Claims</h3>");
                info.AppendLine("<ul>");
                foreach (var claim in User.Claims)
                {
                    info.AppendLine($"<li><strong>{claim.Type}</strong>: {claim.Value}</li>");
                }
                info.AppendLine("</ul>");
            }
            
            info.AppendLine("<h3>Enlaces de diagnóstico</h3>");
            info.AppendLine("<ul>");
            info.AppendLine("<li><a href='/Recepcionista/Diagnostico'>Probar RecepcionistaController</a></li>");
            info.AppendLine("<li><a href='/Recepcionista/Dashboard'>Dashboard de Recepcionista</a></li>");
            info.AppendLine("</ul>");
            
            return Content(info.ToString(), "text/html");
        }
        
        public async Task<IActionResult> Roles()
        {
            StringBuilder info = new StringBuilder();
            
            info.AppendLine("<h2>Diagnóstico de Roles</h2>");
            
            info.AppendLine("<h3>Roles en la base de datos</h3>");
            info.AppendLine("<ul>");
            var roles = _roleManager.Roles.ToList();
            foreach (var role in roles)
            {
                info.AppendLine($"<li><strong>{role.Name}</strong> (ID: {role.Id})</li>");
            }
            info.AppendLine("</ul>");
            
            info.AppendLine("<h3>Usuarios recepcionistas</h3>");
            info.AppendLine("<ul>");
            var recepcionistas = await _userManager.GetUsersInRoleAsync("Recepcionista");
            if (recepcionistas.Any())
            {
                foreach (var user in recepcionistas)
                {
                    info.AppendLine($"<li><strong>{user.UserName}</strong> - {user.Email}</li>");
                }
            }
            else
            {
                info.AppendLine("<li>No hay usuarios con rol Recepcionista</li>");
            }
            info.AppendLine("</ul>");
            
            return Content(info.ToString(), "text/html");
        }
    }
} 