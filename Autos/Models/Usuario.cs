using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Autos.Models
{
    public class Usuario : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public required string Nombre { get; set; }

        [StringLength(20)]
        public string? Identificacion { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        public string Rol { get; set; } = "Cliente"; // Por defecto, el usuario es Cliente

        // Fecha de registro del usuario
        public DateTime? FechaRegistro { get; set; } = DateTime.Now;

        // Para clientes: auto en el que está interesado
        public int? AutoInteresadoId { get; set; }
        public Auto? AutoInteresado { get; set; }

        // Para clientes: vendedor asignado
        public string? VendedorAsignadoId { get; set; }
        public Usuario? VendedorAsignado { get; set; }

        // Para vendedores: sucursales asignadas
        [Display(Name = "Sucursales Asignadas")]
        public List<UsuarioSucursal>? SucursalesAsignadas { get; set; }

        // Para gerentes: sucursal que administra (relación inversa, se maneja desde Sucursal)
        [Display(Name = "Sucursal Administrada")]
        [NotMapped]
        public Sucursal? SucursalAdministrada { get; set; }
    }
}
