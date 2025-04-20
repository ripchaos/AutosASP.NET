using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Autos.Models.ViewModels
{
    public class VendedorDashboardViewModel
    {
        // Usuario vendedor
        public Usuario Vendedor { get; set; } = null!;

        // Métricas generales
        public int TotalSolicitudesPendientes { get; set; }
        public int TotalReservasActivas { get; set; }
        public int TotalVentas { get; set; }
        public int TotalVentasMes { get; set; }
        public decimal MontoTotalVentas { get; set; }

        // Listas para tablas y resúmenes
        public List<SolicitudDescuento> SolicitudesDescuento { get; set; } = new();
        public List<Reserva> ReservasActivas { get; set; } = new();
        public List<Venta> VentasRecientes { get; set; } = new();
        public List<Usuario> ClientesRecientes { get; set; } = new();
    }

    public class ClienteDetalleViewModel
    {
        // Usuario cliente
        public Usuario Cliente { get; set; } = null!;

        // Métricas
        public int TotalCompras { get; set; }
        public decimal MontoTotal { get; set; }

        // Historial de compras
        public List<Venta> Ventas { get; set; } = new();
    }

    public class ReservarAutoDesdeVendedorViewModel
    {
        public ReservarAutoDesdeVendedorViewModel()
        {
            // Inicializar Auto para evitar referencias nulas
            Auto = new Auto
            {
                Marca = string.Empty,
                Modelo = string.Empty,
                Color = string.Empty,
                Categoria = string.Empty,
                EstadoReserva = "Disponible",
                Kilometraje = 0,
                Transmision = "Manual"
            };
        }

        // Datos del cliente
        [Required]
        public string ClienteId { get; set; } = string.Empty;

        [Display(Name = "Cliente")]
        public string NombreCliente { get; set; } = string.Empty;

        // Datos del auto - inicializada en el constructor
        [Required]
        public Auto Auto { get; set; }

        // Datos del vendedor
        [Required]
        public string VendedorId { get; set; } = string.Empty;

        // Datos de la reserva
        [Required(ErrorMessage = "La duración de la reserva es obligatoria")]
        [Range(1, 30, ErrorMessage = "La duración debe estar entre 1 y 30 días")]
        [Display(Name = "Duración de la reserva (días)")]
        public int DuracionReserva { get; set; } = 3;

        [Display(Name = "Comentarios")]
        public string Comentarios { get; set; } = string.Empty;
    }
} 