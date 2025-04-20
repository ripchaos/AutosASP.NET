using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Autos.Models.ViewModels
{
    public class ReasignarVendedorViewModel
    {
        public string ClienteId { get; set; }
        public string NombreCliente { get; set; }
        public string EmailCliente { get; set; }
        
        public string? VendedorActualId { get; set; }
        public string VendedorActualNombre { get; set; }
        
        [Display(Name = "Nuevo Vendedor")]
        public string? NuevoVendedorId { get; set; }
        
        public List<SelectListItem> Vendedores { get; set; } = new List<SelectListItem>();
    }
} 