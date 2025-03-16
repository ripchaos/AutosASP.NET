using Autos.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Autos.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Auto> Autos { get; set; }
        public DbSet<Sucursal> Sucursales { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Venta> Ventas { get; set; }
    }
}
