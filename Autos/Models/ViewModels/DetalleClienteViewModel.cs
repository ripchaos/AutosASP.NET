using System;
using System.Collections.Generic;

namespace Autos.Models.ViewModels
{
    public class DetalleClienteViewModel
    {
        public Usuario Cliente { get; set; }
        public Usuario? VendedorAsignado { get; set; }
        public List<Reserva> Reservas { get; set; } = new List<Reserva>();
        public List<Venta> Compras { get; set; } = new List<Venta>();
    }
} 