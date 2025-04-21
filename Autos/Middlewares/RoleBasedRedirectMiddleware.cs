using Microsoft.AspNetCore.Identity;
using Autos.Models;

namespace Autos.Middlewares
{
    public class RoleBasedRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleBasedRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<Usuario> userManager)
        {
            // Solo aplicar en la ruta de inicio y si el usuario está autenticado
            if (context.Request.Path.Value == "/" && context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user != null)
                {
                    var roles = await userManager.GetRolesAsync(user);

                    // Redireccionar según el rol
                    if (roles.Contains("Cliente"))
                    {
                        context.Response.Redirect("/Cliente/Dashboard");
                        return;
                    }
                    else if (roles.Contains("Vendedor"))
                    {
                        context.Response.Redirect("/Vendedor/Dashboard");
                        return;
                    }
                    else if (roles.Contains("Gerente"))
                    {
                        context.Response.Redirect("/Gerente/Dashboard");
                        return;
                    }
                    else if (roles.Contains("Recepcionista"))
                    {
                        context.Response.Redirect("/Recepcionista/Dashboard");
                        return;
                    }
                    else if (roles.Contains("Administrador"))
                    {
                        context.Response.Redirect("/Admin/Dashboard");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

    // Extension method para facilitar la configuración del middleware
    public static class RoleBasedRedirectMiddlewareExtensions
    {
        public static IApplicationBuilder UseRoleBasedRedirect(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RoleBasedRedirectMiddleware>();
        }
    }
} 