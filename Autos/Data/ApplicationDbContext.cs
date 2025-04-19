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
        public DbSet<SolicitudDescuento> SolicitudesDescuento { get; set; }
        public DbSet<ConfiguracionCorreo> ConfiguracionCorreo { get; set; }
        public DbSet<ListaEspera> ListaEspera { get; set; }
        public DbSet<UsuarioSucursal> UsuariosSucursales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar precisión de decimales
            modelBuilder.Entity<Auto>()
                .Property(a => a.Precio)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Auto>()
                .Property(a => a.PorcentajeDescuento)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Venta>()
                .Property(v => v.Monto)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DescuentoConfig>()
                .Property(d => d.PorcentajeMaximo)
                .HasPrecision(5, 2); // Porcentaje máximo como 99.99%

            // Configurar precisión para los porcentajes de descuento
            modelBuilder.Entity<Venta>()
                .Property(v => v.PorcentajeDescuento)
                .HasPrecision(5, 2);
                
            modelBuilder.Entity<SolicitudDescuento>()
                .Property(s => s.PorcentajeSolicitado)
                .HasPrecision(5, 2);

            // Configurar relaciones para evitar ciclos de cascada
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Vendedor)
                .WithMany()
                .HasForeignKey(v => v.VendedorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Cliente)
                .WithMany()
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SolicitudDescuento>()
                .HasOne(s => s.Vendedor)
                .WithMany()
                .HasForeignKey(s => s.VendedorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SolicitudDescuento>()
                .HasOne(s => s.Gerente)
                .WithMany()
                .HasForeignKey(s => s.GerenteId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configurar relación de auto interesado para clientes
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.AutoInteresado)
                .WithMany()
                .HasForeignKey(u => u.AutoInteresadoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configurar relación de vendedor asignado para clientes
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.VendedorAsignado)
                .WithMany()
                .HasForeignKey(u => u.VendedorAsignadoId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configurar el modelo para UsuarioSucursal
            modelBuilder.Entity<UsuarioSucursal>()
                .HasOne(us => us.Usuario)
                .WithMany(u => u.SucursalesAsignadas)
                .HasForeignKey(us => us.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UsuarioSucursal>()
                .HasOne(us => us.Sucursal)
                .WithMany(s => s.VendedoresAsignados)
                .HasForeignKey(us => us.SucursalId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
