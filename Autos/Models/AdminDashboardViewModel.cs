using System.Collections.Generic;
using Autos.Models;

namespace Autos.Models
{
    public class AdminDashboardViewModel
    {
        public List<Usuario> UsuariosRecientes { get; set; } = new List<Usuario>();
        public List<Venta> VentasRecientes { get; set; } = new List<Venta>();
        
        public int TotalUsuarios { get; set; }
        public int TotalAutos { get; set; }
        public int TotalReservas { get; set; }
        public int TotalVentas { get; set; }
        
        public int VentasDelMes { get; set; }
        public decimal IngresosTotales { get; set; }
        public decimal IngresosMensuales { get; set; }
        public int AutosDisponibles { get; set; }
        
        public int TotalAdministradores { get; set; }
        public int TotalGerentes { get; set; }
        public int TotalVendedores { get; set; }
        public int TotalRecepcionistas { get; set; }
        public int TotalClientes { get; set; }
    }
} 