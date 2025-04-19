using System;
using System.Globalization;

namespace Autos.Models.Utils
{
    /// <summary>
    /// Clase utilitaria para formatear montos en colones costarricenses (₡)
    /// </summary>
    public static class CurrencyFormatter
    {
        /// <summary>
        /// Formatea un valor decimal como moneda en colones costarricenses
        /// </summary>
        /// <param name="amount">Monto a formatear</param>
        /// <returns>Cadena formateada con el símbolo de colón y separadores de miles</returns>
        public static string FormatCRC(decimal amount)
        {
            return $"₡{amount.ToString("N0")}";
        }

        /// <summary>
        /// Obtiene una cultura personalizada para colones costarricenses
        /// </summary>
        /// <returns>Objeto CultureInfo configurado para colones</returns>
        public static CultureInfo GetCRCCulture()
        {
            // Crear una copia de la cultura actual
            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            
            // Configurar el formato de moneda
            culture.NumberFormat.CurrencySymbol = "₡";
            culture.NumberFormat.CurrencyDecimalSeparator = ",";
            culture.NumberFormat.CurrencyGroupSeparator = ".";
            culture.NumberFormat.CurrencyDecimalDigits = 0;
            
            return culture;
        }

        /// <summary>
        /// Formatea un valor decimal como moneda usando la configuración de colones costarricenses
        /// </summary>
        /// <param name="amount">Monto a formatear</param>
        /// <returns>Cadena formateada como moneda</returns>
        public static string FormatCurrency(decimal amount)
        {
            return amount.ToString("C", GetCRCCulture());
        }
    }
} 