using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Autos.Models
{
    public class Sucursal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public required string Nombre { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Dirección")]
        public required string Direccion { get; set; }
        
        [Display(Name = "Ubicación")]
        [StringLength(100)]
        public string? Ubicacion { get; set; }
        
        [Display(Name = "Gerente")]
        public string? GerenteId { get; set; }
        
        [ForeignKey("GerenteId")]
        public Usuario? Gerente { get; set; }
        
        [Display(Name = "Activa")]
        public bool Activa { get; set; } = true;

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(200)]
        [Display(Name = "Horario de Atención")]
        public string? HorarioAtencion { get; set; }

        // Vendedores asignados a esta sucursal
        [Display(Name = "Vendedores Asignados")]
        public List<UsuarioSucursal>? VendedoresAsignados { get; set; }

        // Autos disponibles en esta sucursal
        [Display(Name = "Inventario de Autos")]
        public List<Auto>? Autos { get; set; }
    }
}
