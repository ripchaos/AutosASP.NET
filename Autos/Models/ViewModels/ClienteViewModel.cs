using System;
using System.Collections.Generic;

namespace Autos.Models.ViewModels
{
    public class ClienteViewModel
    {
        public required Usuario Cliente { get; set; }
        public required string VendedorNombre { get; set; } = string.Empty;
        public int ReservasActivas { get; set; }
        public int ComprasRealizadas { get; set; }
    }
} 