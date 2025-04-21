using System;
using System.ComponentModel.DataAnnotations;

namespace Autos.Models
{
    public class SolicitudDescuento
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
        [Range(0, 100)]
        public decimal PorcentajeSolicitado { get; set; }

        public string? Justificacion { get; set; }

        [Required]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        public string? GerenteId { get; set; }
        public Usuario? Gerente { get; set; }

        public bool? Aprobada { get; set; }

        // Fecha de respuesta (cuando se aprueba o rechaza)
        public DateTime? FechaRespuesta { get; set; }
        
        // Fecha de resolución existente (para mantener compatibilidad)
        public DateTime? FechaResolucion { get; set; }

        // Motivo de rechazo (si fue rechazada)
        [StringLength(500)]
        public string? MotivoRechazo { get; set; }

        public string? ComentarioGerente { get; set; }

        // Estados posibles: "Pendiente", "Aprobada", "Rechazada"
        public string Estado => Aprobada.HasValue 
            ? (Aprobada.Value ? "Aprobada" : "Rechazada") 
            : "Pendiente";
    }
} 