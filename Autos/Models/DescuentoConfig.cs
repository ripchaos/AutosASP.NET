using System.ComponentModel.DataAnnotations;

namespace Autos.Models
{
    public class DescuentoConfig
    {
        public int Id { get; set; }

        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100.")]
        [Display(Name = "Máximo % de descuento permitido")]
        public int PorcentajeMaximo { get; set; }
    }
}
