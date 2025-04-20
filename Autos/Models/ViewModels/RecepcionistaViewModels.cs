using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Autos.Models;

namespace Autos.Models.ViewModels
{
    public class RecepcionistaDashboardViewModel
    {
        public Usuario? Recepcionista { get; set; }
        public Sucursal? Sucursal { get; set; }
        public int AutosDisponibles { get; set; }
        public int VendedoresTotales { get; set; }
        public int ClientesRegistradosHoy { get; set; }
        public int ReservasHoy { get; set; }
        public List<Auto> AutosRecientes { get; set; }
        public List<Usuario> Vendedores { get; set; }

        public RecepcionistaDashboardViewModel()
        {
            AutosRecientes = new List<Auto>();
            Vendedores = new List<Usuario>();
            AutosDisponibles = 0;
            VendedoresTotales = 0;
            ClientesRegistradosHoy = 0;
            ReservasHoy = 0;
        }
    }

    public class RegistrarClienteViewModel
    {
        [Display(Name = "Nombre completo")]
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;
        
        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public string Email { get; set; } = string.Empty;
        
        [Display(Name = "Teléfono")]
        [Required(ErrorMessage = "El teléfono es obligatorio")]
        public string Telefono { get; set; } = string.Empty;
        
        [Display(Name = "Dirección")]
        [Required(ErrorMessage = "La dirección es obligatoria")]
        public string Direccion { get; set; } = string.Empty;
        
        [Display(Name = "Identificación (DNI/Pasaporte)")]
        [Required(ErrorMessage = "La identificación es obligatoria")]
        public string Identificacion { get; set; } = string.Empty;
        
        [Display(Name = "Vendedor asignado")]
        public string VendedorId { get; set; } = string.Empty;
        
        public List<Usuario> Vendedores { get; set; }
        
        public int? AutoId { get; set; }
        public Auto? Auto { get; set; }
        
        [Display(Name = "Crear reserva para este vehículo")]
        public bool CrearReserva { get; set; }
        
        public RegistrarClienteViewModel()
        {
            Vendedores = new List<Usuario>();
        }
    }

    public class VendedoresClientesViewModel
    {
        public Sucursal? Sucursal { get; set; }
        public List<VendedorConClientes> Vendedores { get; set; }
        
        public VendedoresClientesViewModel()
        {
            Vendedores = new List<VendedorConClientes>();
        }
    }
    
    public class VendedorConClientes
    {
        public Usuario Vendedor { get; set; } = null!;
        public List<Usuario> Clientes { get; set; }
        
        public VendedorConClientes()
        {
            Clientes = new List<Usuario>();
        }
    }
} 