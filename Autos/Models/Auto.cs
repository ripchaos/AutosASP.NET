using System.ComponentModel.DataAnnotations;

namespace Autos.Models
{
    public class Auto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Marca { get; set; }

        [Required]
        [StringLength(50)]
        public required string Modelo { get; set; }

        [Required]
        public int Anio { get; set; }

        [Required]
        public decimal Precio { get; set; }

        public bool Disponibilidad { get; set; } = true;

        public int SucursalId { get; set; }
        public Sucursal? Sucursal { get; set; }
    }
}
