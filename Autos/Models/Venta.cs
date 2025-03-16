using System;
using System.ComponentModel.DataAnnotations;

namespace Autos.Models
{
    public class Venta
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
        public DateTime FechaVenta { get; set; } = DateTime.Now;

        [Required]
        public decimal Monto { get; set; }
    }
}

