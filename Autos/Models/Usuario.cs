using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Autos.Models
{
    public class Usuario : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public required string Nombre { get; set; }

        public string Rol { get; set; } = "Cliente"; // Por defecto, el usuario es Cliente
    }
}
