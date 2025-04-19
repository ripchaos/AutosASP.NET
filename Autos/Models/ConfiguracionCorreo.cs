using System.ComponentModel.DataAnnotations;

namespace Autos.Models
{
    public class ConfiguracionCorreo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El servidor SMTP es requerido")]
        [Display(Name = "Servidor SMTP")]
        public required string Servidor { get; set; }

        [Required(ErrorMessage = "El puerto es requerido")]
        [Range(1, 65535, ErrorMessage = "El puerto debe estar entre 1 y 65535")]
        [Display(Name = "Puerto")]
        public int Puerto { get; set; } = 587; // Puerto por defecto para TLS

        [Required(ErrorMessage = "El usuario es requerido")]
        [Display(Name = "Usuario")]
        public required string Usuario { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [Display(Name = "Contraseña")]
        public required string Password { get; set; }

        [Display(Name = "Utilizar SSL/TLS")]
        public bool RequiereSSL { get; set; } = true;

        [Required(ErrorMessage = "El email del remitente es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Email del Remitente")]
        public required string EmailRemitente { get; set; }

        [Required(ErrorMessage = "El nombre del remitente es requerido")]
        [Display(Name = "Nombre del Remitente")]
        public required string NombreRemitente { get; set; }

        // Opciones de notificación para clientes
        [Display(Name = "Notificar reservas")]
        public bool NotificarReservas { get; set; } = true;

        [Display(Name = "Notificar ventas")]
        public bool NotificarVentas { get; set; } = true;

        [Display(Name = "Notificar descuentos")]
        public bool NotificarDescuentos { get; set; } = true;

        // Opciones de notificación interna
        [Display(Name = "Notificar nuevas reservas al personal")]
        public bool NotificarNuevasReservas { get; set; } = true;

        [Display(Name = "Notificar nuevas ventas al personal")]
        public bool NotificarNuevasVentas { get; set; } = true;

        [Display(Name = "Notificar solicitudes de descuento al personal")]
        public bool NotificarSolicitudesDescuento { get; set; } = true;

        [Display(Name = "Emails para notificaciones internas")]
        public string? EmailsNotificacionesInternas { get; set; }
    }
} 