using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Autos.Models.ViewModels
{
    public class ReasignarVendedorViewModel
    {
        public required string ClienteId { get; set; }
        public required string NombreCliente { get; set; }
        public required string EmailCliente { get; set; }
        
        public string? VendedorActualId { get; set; }
        public required string VendedorActualNombre { get; set; } = string.Empty;
        
        [Display(Name = "Nuevo Vendedor")]
        public string? NuevoVendedorId { get; set; }
        
        public List<SelectListItem> Vendedores { get; set; } = new List<SelectListItem>();
    }
} 