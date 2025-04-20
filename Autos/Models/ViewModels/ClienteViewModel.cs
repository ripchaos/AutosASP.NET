using System;
using System.Collections.Generic;

namespace Autos.Models.ViewModels
{
    public class ClienteViewModel
    {
        public Usuario Cliente { get; set; }
        public string VendedorNombre { get; set; }
        public int ReservasActivas { get; set; }
        public int ComprasRealizadas { get; set; }
    }
} 