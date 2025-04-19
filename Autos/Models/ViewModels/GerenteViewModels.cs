using System;
using System.Collections.Generic;

namespace Autos.Models
{
    public class GerenteDashboardViewModel
    {
        // Sucursal administrada por el gerente
        public Sucursal Sucursal { get; set; } = null!;

        // Métricas generales
        public int TotalVentas { get; set; }
        public decimal TotalIngresos { get; set; }
        public int TotalVendedores { get; set; }
        public int TotalInventario { get; set; }

        // Listas para gráficos y tablas
        public List<object> VentasPorMes { get; set; } = new();
        public List<object> VentasPorVendedor { get; set; } = new();
        public List<object> AutosMasVendidos { get; set; } = new();
        public List<Venta> UltimasVentas { get; set; } = new();
    }

    public class VendedorVentasViewModel
    {
        public Usuario? Vendedor { get; set; }
        public int TotalVentas { get; set; }
        public decimal MontoTotal { get; set; }
        public Venta? UltimaVenta { get; set; }
        public bool EsPrincipal { get; set; }
    }

    public class AdministrarVendedoresViewModel
    {
        public Sucursal Sucursal { get; set; } = null!;
        public List<UsuarioSucursal> VendedoresAsignados { get; set; } = new();
        public List<Usuario> VendedoresDisponibles { get; set; } = new();
    }

    public class VendedorDetalleViewModel
    {
        public Usuario Vendedor { get; set; } = null!;
        public int TotalVentas { get; set; }
        public decimal MontoTotal { get; set; }
        public List<Venta> VentasRecientes { get; set; } = new();
        public DateTime FechaAsignacion { get; set; }
        public bool EsPrincipal { get; set; }
        public bool Activo { get; set; }
    }
} 