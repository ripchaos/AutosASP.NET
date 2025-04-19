using System.ComponentModel.DataAnnotations;

namespace Autos.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre Completo")]
        public required string Nombre { get; set; }

        [Required(ErrorMessage = "La identificación es obligatoria")]
        [Display(Name = "Número de Identificación")]
        [StringLength(20, ErrorMessage = "La identificación no puede exceder 20 caracteres")]
        public required string Identificacion { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [Display(Name = "Dirección")]
        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        public required string Direccion { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo es inválido")]
        [Display(Name = "Correo Electrónico")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        [StringLength(100, ErrorMessage = "La contraseña debe tener al menos {2} caracteres", MinimumLength = 6)]
        public required string Password { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        [Display(Name = "Rol")]
        public required string Rol { get; set; }

        // Para clientes: auto en el que está interesado
        [Display(Name = "Auto Interesado")]
        public int? AutoInteresadoId { get; set; }
    }
}
