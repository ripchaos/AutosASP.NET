using System.ComponentModel.DataAnnotations;

namespace Autos.Models
{
    public class RegisterViewModel
    {
        [Required]
        public required string Nombre { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Required]
        public required string Rol { get; set; } // 📌 Se agrega el campo Rol para seleccionar el rol del usuario
    }
}
