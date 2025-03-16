using System;
using System.ComponentModel.DataAnnotations;

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

        [Required]
        public DateTime FechaReserva { get; set; } = DateTime.Now;
    }
}

