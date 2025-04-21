using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Autos.Models
{
    public class Reserva
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Required]
        public int AutoId { get; set; }
        public Auto? Auto { get; set; }

        // Vendedor asociado a la reserva
        public string? VendedorId { get; set; }
        public Usuario? Vendedor { get; set; }

        [Required]
        public DateTime FechaReserva { get; set; } = DateTime.Now;

        // Fecha de expiración de la reserva (por defecto 7 días)
        public DateTime FechaExpiracion { get; set; } = DateTime.Now.AddDays(7);

        // Estado de la reserva: Activa, Vencida, Cancelada, Convertida a Venta
        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Activa";

        // Comentarios sobre la reserva
        [StringLength(500)]
        public string? Comentarios { get; set; }

        // Motivo de cancelación (si fue cancelada)
        [StringLength(500)]
        public string? MotivoCancelacion { get; set; }

        // Fecha de cancelación (si fue cancelada)
        public DateTime? FechaCancelacion { get; set; }

        // Fecha de concretación de venta (si se convirtió en venta)
        public DateTime? FechaConcretacion { get; set; }

        // Propiedad calculada para determinar si la reserva está vencida
        [NotMapped]
        public bool EstaVencida => FechaExpiracion < DateTime.Now && Estado == "Activa";
    }
}

