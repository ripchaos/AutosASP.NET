using System;
using System.ComponentModel.DataAnnotations;

namespace Autos.Models
{
    public class ListaEspera
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AutoId { get; set; }
        public Auto? Auto { get; set; }

        [Required]
        public required string UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Required]
        [Display(Name = "Fecha de solicitud")]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        [Display(Name = "Notificado")]
        public bool Notificado { get; set; } = false;
        
        [Display(Name = "Fecha de notificación")]
        public DateTime? FechaNotificacion { get; set; }
        
        [StringLength(500)]
        [Display(Name = "Comentarios")]
        public string? Comentarios { get; set; }
    }
} 