using System.ComponentModel.DataAnnotations;

namespace Autos.Models
{
    public class Sucursal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Nombre { get; set; }

        [Required]
        [StringLength(255)]
        public required string Direccion { get; set; }
    }
}
