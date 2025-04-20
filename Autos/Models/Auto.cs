using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Range(0, 9999999.99)]
        [Display(Name = "Precio en Colones")]
        public decimal Precio { get; set; }

        [Range(0, 500000)]
        [Display(Name = "Kilometraje")]
        public int Kilometraje { get; set; } = 0;

        [StringLength(20)]
        [Display(Name = "Transmisión")]
        public string Transmision { get; set; } = "Manual";

        [StringLength(20)]
        [Display(Name = "Combustible")]
        public string Combustible { get; set; } = "Gasolina";

        public bool Disponibilidad { get; set; } = true;

        public int SucursalId { get; set; }
        public Sucursal? Sucursal { get; set; }

        public bool TieneDescuento { get; set; } = false;
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public decimal PorcentajeDescuento { get; set; } = 0;

        // Estado de reserva: "Disponible", "Reservado", "Vendido"
        [StringLength(20)]
        [Display(Name = "Estado")]
        public string EstadoReserva { get; set; } = "Disponible";

        [StringLength(30)]
        public string Color { get; set; } = "";
        
        [StringLength(50)]
        public string Categoria { get; set; } = "";

        // Fecha de finalización de la reserva, si está reservado
        [Display(Name = "Reservado hasta")]
        public DateTime? FechaFinReserva { get; set; }
        
        // Propiedad de solo lectura para la fecha a partir de la cual estará disponible
        [NotMapped]
        [Display(Name = "Disponible a partir de")]
        public DateTime? FechaDisponible => FechaFinReserva?.AddDays(1);

        // Precio con descuento aplicado
        [NotMapped]
        [Display(Name = "Precio con Descuento")]
        public decimal PrecioConDescuento => TieneDescuento ? Precio - (Precio * PorcentajeDescuento / 100) : Precio;

        /// <summary>
        /// Devuelve el precio formateado en colones costarricenses
        /// </summary>
        [NotMapped]
        public string PrecioFormateado => $"₡{Precio:N0}";

        /// <summary>
        /// Devuelve el precio con descuento formateado en colones costarricenses
        /// </summary>
        [NotMapped]
        public string PrecioConDescuentoFormateado => $"₡{PrecioConDescuento:N0}";
    }
}
