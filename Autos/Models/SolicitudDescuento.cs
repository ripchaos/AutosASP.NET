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

        public DateTime? FechaResolucion { get; set; }

        public string? ComentarioGerente { get; set; }

        // Estados posibles: "Pendiente", "Aprobada", "Rechazada"
        public string Estado => Aprobada.HasValue 
            ? (Aprobada.Value ? "Aprobada" : "Rechazada") 
            : "Pendiente";
    }
} 