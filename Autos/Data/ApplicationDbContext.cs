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
        public DbSet<DescuentoConfig> DescuentosConfig { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar precisión de decimales
            modelBuilder.Entity<Auto>()
                .Property(a => a.Precio)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Venta>()
                .Property(v => v.Monto)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DescuentoConfig>()
                .Property(d => d.PorcentajeMaximo)
                .HasPrecision(5, 2); // Porcentaje máximo como 99.99%
        }
    }
}
