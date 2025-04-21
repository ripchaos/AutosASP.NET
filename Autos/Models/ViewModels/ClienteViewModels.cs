using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Autos.Models.ViewModels
{
    public class ClienteDashboardViewModel
    {
        // Usuario cliente
        public Usuario Cliente { get; set; } = null!;

        // Vendedor asignado al cliente
        public Usuario? VendedorAsignado { get; set; }

        // Auto de interés
        public Auto? AutoInteres { get; set; }

        // Reservas activas del cliente
        public List<Reserva> ReservasActivas { get; set; } = new();

        // Historial de compras
        public List<Venta> HistorialCompras { get; set; } = new();
    }

    public class ClientePerfilViewModel
    {
        // Usuario cliente
        public Usuario Cliente { get; set; } = null!;

        // Vendedor asignado al cliente
        public Usuario? VendedorAsignado { get; set; }
    }
} 