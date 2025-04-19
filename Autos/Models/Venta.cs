using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Autos.Models
{
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AutoId { get; set; }
        public Auto? Auto { get; set; }

        [Required]
        public required string VendedorId { get; set; }
        public Usuario? Vendedor { get; set; }

        [Required]
        public required string ClienteId { get; set; }
        public Usuario? Cliente { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        [Range(0, 9999999.99)]
        [Display(Name = "Monto en Colones")]
        public decimal Monto { get; set; }

        [Range(0, 100)]
        [Display(Name = "Porcentaje de Descuento")]
        public decimal PorcentajeDescuento { get; set; } = 0;

        /// <summary>
        /// Monto final después de aplicar descuento
        /// </summary>
        public decimal MontoFinal => Monto - (Monto * PorcentajeDescuento / 100);

        /// <summary>
        /// Devuelve el monto formateado en colones costarricenses
        /// </summary>
        [NotMapped]
        public string MontoFormateado => $"₡{MontoFinal:N0}";

        /// <summary>
        /// Devuelve el monto original formateado en colones costarricenses
        /// </summary>
        [NotMapped]
        public string MontoOriginalFormateado => $"₡{Monto:N0}";

        /// <summary>
        /// Estado de la venta: Pendiente, Completada, Cancelada
        /// Este campo está en el modelo pero podría no estar en la base de datos
        /// </summary>
        [StringLength(20)]
        [NotMapped] // Marcamos esta propiedad como no mapeada a la base de datos
        public string Estado { get; set; } = "Completada";
    }
}


