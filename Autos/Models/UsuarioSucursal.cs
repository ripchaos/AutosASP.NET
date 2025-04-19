using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Autos.Models
{
    // Tabla de relación muchos a muchos entre Usuario y Sucursal
    public class UsuarioSucursal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        public int SucursalId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        [ForeignKey("SucursalId")]
        public Sucursal? Sucursal { get; set; }

        // Define si el usuario es el vendedor principal de la sucursal
        [Display(Name = "Es Principal")]
        public bool EsPrincipal { get; set; } = false;

        // Fecha cuando el usuario fue asignado a la sucursal
        [Display(Name = "Fecha de Asignación")]
        public DateTime FechaAsignacion { get; set; } = DateTime.Now;

        // Indica si la asignación está activa
        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;
    }
} 