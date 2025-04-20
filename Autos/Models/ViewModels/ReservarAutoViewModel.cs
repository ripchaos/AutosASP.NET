using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Autos.Models;

namespace Autos.Models.ViewModels
{
    public class ReservarAutoViewModel
    {
        public Auto? Auto { get; set; }

        [Required(ErrorMessage = "El nombre del cliente es obligatorio")]
        [Display(Name = "Nombre completo")]
        public string NombreCliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email del cliente es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato de email no es válido")]
        [Display(Name = "Correo electrónico")]
        public string EmailCliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono del cliente es obligatorio")]
        [Phone(ErrorMessage = "El formato de teléfono no es válido")]
        [Display(Name = "Teléfono")]
        public string TelefonoCliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección del cliente es obligatoria")]
        [Display(Name = "Dirección")]
        public string DireccionCliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "La identificación del cliente es obligatoria")]
        [Display(Name = "Número de identificación")]
        public string IdentificacionCliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "La duración de la reserva es obligatoria")]
        [Range(1, 30, ErrorMessage = "La duración debe estar entre 1 y 30 días")]
        [Display(Name = "Duración de la reserva (días)")]
        public int DuracionReserva { get; set; } = 3;

        [Display(Name = "Comentarios adicionales")]
        public string Comentarios { get; set; } = string.Empty;

        [Display(Name = "Vendedor asignado")]
        public string VendedorId { get; set; } = string.Empty;

        public List<Usuario> Vendedores { get; set; } = new List<Usuario>();
        
        [Display(Name = "Cliente existente")]
        public bool ClienteExistente { get; set; }
        
        [Display(Name = "Cliente")]
        public string ClienteId { get; set; } = string.Empty;

        // Propiedades del cliente adicionales para compatibilidad con el controlador
        [Display(Name = "Nombre")]
        public string Nombre 
        { 
            get => NombreCliente;
            set => NombreCliente = value;
        }
        
        [Display(Name = "Email")]
        public string Email 
        { 
            get => EmailCliente;
            set => EmailCliente = value;
        }
        
        [Display(Name = "Teléfono")]
        public string Telefono 
        { 
            get => TelefonoCliente;
            set => TelefonoCliente = value;
        }
        
        [Display(Name = "Dirección")]
        public string Direccion 
        { 
            get => DireccionCliente;
            set => DireccionCliente = value;
        }
        
        [Display(Name = "Identificación")]
        public string Identificacion 
        { 
            get => IdentificacionCliente;
            set => IdentificacionCliente = value;
        }
    }
} 