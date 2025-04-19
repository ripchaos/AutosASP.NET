using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Autos.Models;
using Microsoft.AspNetCore.Hosting;
using Autos.Models.Utils;

namespace Autos.Services
{
    public interface IEmailTemplateService
    {
        string GenerarPlantillaConfirmacionReserva(Reserva reserva);
        string GenerarPlantillaConfirmacionVenta(Venta venta);
        string GenerarPlantillaSolicitudDescuento(SolicitudDescuento solicitud);
        string GenerarPlantillaDescuentoAprobado(SolicitudDescuento solicitud);
        string GenerarPlantillaDescuentoRechazado(SolicitudDescuento solicitud);
    }

    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _plantillasPath;

        public EmailTemplateService(IWebHostEnvironment env)
        {
            _env = env;
            _plantillasPath = Path.Combine(_env.WebRootPath, "templates", "email");
            
            // Asegurar que el directorio de plantillas existe
            if (!Directory.Exists(_plantillasPath))
            {
                Directory.CreateDirectory(_plantillasPath);
                CrearPlantillasIniciales();
            }
            else if (!File.Exists(Path.Combine(_plantillasPath, "PlantillaBase.html")))
            {
                CrearPlantillasIniciales();
            }
        }
        
        private void CrearPlantillasIniciales()
        {
            // Plantilla base con el layout común para todos los correos
            var plantillaBase = @"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <title>{{TITULO}}</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; }
        .header { background-color: #3498db; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; }
        .footer { background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 12px; color: #777; }
        .button { display: inline-block; background-color: #3498db; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; }
        .info-box { background-color: #f8f9fa; border-left: 4px solid #3498db; padding: 15px; margin-bottom: 20px; }
    </style>
</head>
<body>
    <div class='header'>
        <h1>Concesionario de Autos</h1>
    </div>
    <div class='content'>
        {{CONTENIDO}}
    </div>
    <div class='footer'>
        <p>© {{AÑO}} Concesionario de Autos. Todos los derechos reservados.</p>
        <p>Este es un correo automático, por favor no responda a este mensaje.</p>
    </div>
</body>
</html>";

            // Guardar plantilla base
            File.WriteAllText(Path.Combine(_plantillasPath, "PlantillaBase.html"), plantillaBase);
            
            // Plantilla para confirmación de reserva
            var plantillaReserva = @"<h2>¡Su reserva ha sido confirmada!</h2>
<p>Estimado/a <strong>{{NOMBRE_CLIENTE}}</strong>,</p>
<p>Nos complace confirmar su reserva para el vehículo:</p>
<div class='info-box'>
    <p><strong>Marca:</strong> {{MARCA}}</p>
    <p><strong>Modelo:</strong> {{MODELO}}</p>
    <p><strong>Año:</strong> {{AÑO}}</p>
    <p><strong>Fecha de reserva:</strong> {{FECHA_RESERVA}}</p>
    <p><strong>Fecha de expiración:</strong> {{FECHA_EXPIRACION}}</p>
</div>
<p>Su reserva estará disponible hasta la fecha de expiración. Si desea extender la reserva o realizar la compra, por favor contáctenos.</p>
<p>Su asesor asignado es <strong>{{NOMBRE_VENDEDOR}}</strong>, quien estará disponible para responder cualquier pregunta que pueda tener.</p>
<p>¡Gracias por elegir nuestro concesionario!</p>";

            // Guardar plantilla de reserva
            File.WriteAllText(Path.Combine(_plantillasPath, "ConfirmacionReserva.html"), plantillaReserva);
            
            // Plantilla para confirmación de venta
            var plantillaVenta = @"<h2>¡Confirmación de Compra!</h2>
<p>Estimado/a <strong>{{NOMBRE_CLIENTE}}</strong>,</p>
<p>¡Gracias por su compra! A continuación, encontrará los detalles de su vehículo:</p>
<div class='info-box'>
    <p><strong>Marca:</strong> {{MARCA}}</p>
    <p><strong>Modelo:</strong> {{MODELO}}</p>
    <p><strong>Año:</strong> {{AÑO}}</p>
    <p><strong>Precio:</strong> {{PRECIO}}</p>
    <p><strong>Descuento aplicado:</strong> {{DESCUENTO}}%</p>
    <p><strong>Precio final:</strong> {{PRECIO_FINAL}}</p>
    <p><strong>Fecha de compra:</strong> {{FECHA_COMPRA}}</p>
</div>
<p>Próximamente nuestro equipo se pondrá en contacto con usted para coordinar la entrega del vehículo.</p>
<p>Si tiene alguna pregunta, no dude en contactar a su asesor <strong>{{NOMBRE_VENDEDOR}}</strong>.</p>
<p>¡Le agradecemos su preferencia!</p>";

            // Guardar plantilla de venta
            File.WriteAllText(Path.Combine(_plantillasPath, "ConfirmacionVenta.html"), plantillaVenta);
            
            // Plantilla para solicitud de descuento
            var plantillaSolicitudDescuento = @"<h2>Solicitud de Descuento</h2>
<p>Se ha recibido una nueva solicitud de descuento con los siguientes detalles:</p>
<div class='info-box'>
    <p><strong>Auto:</strong> {{MARCA}} {{MODELO}} ({{AÑO}})</p>
    <p><strong>Precio original:</strong> {{PRECIO}}</p>
    <p><strong>Descuento solicitado:</strong> {{DESCUENTO}}%</p>
    <p><strong>Precio con descuento:</strong> {{PRECIO_DESCUENTO}}</p>
    <p><strong>Vendedor:</strong> {{NOMBRE_VENDEDOR}}</p>
    <p><strong>Cliente interesado:</strong> {{NOMBRE_CLIENTE}}</p>
    <p><strong>Fecha de solicitud:</strong> {{FECHA_SOLICITUD}}</p>
</div>
<p><strong>Justificación:</strong> {{JUSTIFICACION}}</p>
<p>Por favor, revise esta solicitud en el sistema para aprobarla o rechazarla.</p>";

            // Guardar plantilla de solicitud de descuento
            File.WriteAllText(Path.Combine(_plantillasPath, "SolicitudDescuento.html"), plantillaSolicitudDescuento);
            
            // Plantilla para descuento aprobado
            var plantillaDescuentoAprobado = @"<h2>¡Descuento Aprobado!</h2>
<p>Estimado/a <strong>{{NOMBRE_CLIENTE}}</strong>,</p>
<p>Nos complace informarle que su solicitud de descuento ha sido <strong style='color:green;'>APROBADA</strong>.</p>
<div class='info-box'>
    <p><strong>Auto:</strong> {{MARCA}} {{MODELO}} ({{AÑO}})</p>
    <p><strong>Precio original:</strong> {{PRECIO}}</p>
    <p><strong>Descuento aprobado:</strong> {{DESCUENTO}}%</p>
    <p><strong>Precio final:</strong> {{PRECIO_DESCUENTO}}</p>
</div>
<p>Su asesor, <strong>{{NOMBRE_VENDEDOR}}</strong>, se pondrá en contacto con usted para finalizar la compra.</p>
<p>Este descuento es válido por 7 días a partir de la fecha de aprobación.</p>
<p>¡Gracias por elegir nuestro concesionario!</p>";

            // Guardar plantilla de descuento aprobado
            File.WriteAllText(Path.Combine(_plantillasPath, "DescuentoAprobado.html"), plantillaDescuentoAprobado);
            
            // Plantilla para descuento rechazado
            var plantillaDescuentoRechazado = @"<h2>Respuesta a Solicitud de Descuento</h2>
<p>Estimado/a <strong>{{NOMBRE_CLIENTE}}</strong>,</p>
<p>Lamentamos informarle que su solicitud de descuento no ha podido ser aprobada en esta ocasión.</p>
<div class='info-box'>
    <p><strong>Auto:</strong> {{MARCA}} {{MODELO}} ({{AÑO}})</p>
    <p><strong>Precio:</strong> {{PRECIO}}</p>
    <p><strong>Descuento solicitado:</strong> {{DESCUENTO}}%</p>
</div>
<p>Comentarios del gerente: <em>{{COMENTARIO_GERENTE}}</em></p>
<p>Su asesor, <strong>{{NOMBRE_VENDEDOR}}</strong>, se pondrá en contacto con usted para discutir otras alternativas que puedan ajustarse mejor a sus necesidades.</p>
<p>Agradecemos su interés en nuestros vehículos y esperamos poder servirle en un futuro próximo.</p>";

            // Guardar plantilla de descuento rechazado
            File.WriteAllText(Path.Combine(_plantillasPath, "DescuentoRechazado.html"), plantillaDescuentoRechazado);
        }

        public string GenerarPlantillaConfirmacionReserva(Reserva reserva)
        {
            // Cargar las plantillas
            string plantillaBase = File.ReadAllText(Path.Combine(_plantillasPath, "PlantillaBase.html"));
            string plantillaReserva = File.ReadAllText(Path.Combine(_plantillasPath, "ConfirmacionReserva.html"));
            
            // Reemplazar variables en la plantilla de reserva
            plantillaReserva = plantillaReserva
                .Replace("{{NOMBRE_CLIENTE}}", reserva.Usuario?.Nombre ?? "Cliente")
                .Replace("{{MARCA}}", reserva.Auto?.Marca ?? "")
                .Replace("{{MODELO}}", reserva.Auto?.Modelo ?? "")
                .Replace("{{AÑO}}", reserva.Auto?.Anio.ToString() ?? "")
                .Replace("{{FECHA_RESERVA}}", reserva.FechaReserva.ToString("dd/MM/yyyy"))
                .Replace("{{FECHA_EXPIRACION}}", reserva.FechaReserva.AddDays(7).ToString("dd/MM/yyyy")) // Default a 7 días
                .Replace("{{NOMBRE_VENDEDOR}}", "Su asesor asignado");
            
            // Integrar plantilla de reserva en plantilla base
            string correoCompleto = plantillaBase
                .Replace("{{TITULO}}", "Confirmación de Reserva")
                .Replace("{{CONTENIDO}}", plantillaReserva)
                .Replace("{{AÑO}}", DateTime.Now.Year.ToString());
            
            return correoCompleto;
        }

        public string GenerarPlantillaConfirmacionVenta(Venta venta)
        {
            // Cargar las plantillas
            string plantillaBase = File.ReadAllText(Path.Combine(_plantillasPath, "PlantillaBase.html"));
            string plantillaVenta = File.ReadAllText(Path.Combine(_plantillasPath, "ConfirmacionVenta.html"));
            
            // Reemplazar variables en la plantilla de venta
            plantillaVenta = plantillaVenta
                .Replace("{{NOMBRE_CLIENTE}}", venta.Cliente?.Nombre ?? "Cliente")
                .Replace("{{MARCA}}", venta.Auto?.Marca ?? "")
                .Replace("{{MODELO}}", venta.Auto?.Modelo ?? "")
                .Replace("{{AÑO}}", venta.Auto?.Anio.ToString() ?? "")
                .Replace("{{PRECIO}}", CurrencyFormatter.FormatCRC(venta.Monto))
                .Replace("{{DESCUENTO}}", venta.PorcentajeDescuento.ToString("0.##"))
                .Replace("{{PRECIO_FINAL}}", CurrencyFormatter.FormatCRC(venta.MontoFinal))
                .Replace("{{FECHA_COMPRA}}", venta.Fecha.ToString("dd/MM/yyyy"))
                .Replace("{{NOMBRE_VENDEDOR}}", venta.Vendedor?.Nombre ?? "Su asesor");
            
            // Integrar plantilla de venta en plantilla base
            string correoCompleto = plantillaBase
                .Replace("{{TITULO}}", "Confirmación de Compra")
                .Replace("{{CONTENIDO}}", plantillaVenta)
                .Replace("{{AÑO}}", DateTime.Now.Year.ToString());
            
            return correoCompleto;
        }

        public string GenerarPlantillaSolicitudDescuento(SolicitudDescuento solicitud)
        {
            // Cargar las plantillas
            string plantillaBase = File.ReadAllText(Path.Combine(_plantillasPath, "PlantillaBase.html"));
            string plantillaSolicitud = File.ReadAllText(Path.Combine(_plantillasPath, "SolicitudDescuento.html"));
            
            // Calcular precio con descuento
            decimal precioOriginal = solicitud.Auto?.Precio ?? 0;
            decimal precioConDescuento = precioOriginal - (precioOriginal * solicitud.PorcentajeSolicitado / 100);
            
            // Reemplazar variables en la plantilla de solicitud de descuento
            plantillaSolicitud = plantillaSolicitud
                .Replace("{{MARCA}}", solicitud.Auto?.Marca ?? "")
                .Replace("{{MODELO}}", solicitud.Auto?.Modelo ?? "")
                .Replace("{{AÑO}}", solicitud.Auto?.Anio.ToString() ?? "")
                .Replace("{{PRECIO}}", CurrencyFormatter.FormatCRC(precioOriginal))
                .Replace("{{DESCUENTO}}", solicitud.PorcentajeSolicitado.ToString("0.##"))
                .Replace("{{PRECIO_DESCUENTO}}", CurrencyFormatter.FormatCRC(precioConDescuento))
                .Replace("{{NOMBRE_VENDEDOR}}", solicitud.Vendedor?.Nombre ?? "Vendedor")
                .Replace("{{NOMBRE_CLIENTE}}", "Cliente Interesado") // Aquí podríamos incluir datos del cliente si se añaden a la solicitud
                .Replace("{{FECHA_SOLICITUD}}", solicitud.FechaSolicitud.ToString("dd/MM/yyyy HH:mm"))
                .Replace("{{JUSTIFICACION}}", solicitud.Justificacion ?? "No se proporcionó justificación.");
            
            // Integrar plantilla de solicitud en plantilla base
            string correoCompleto = plantillaBase
                .Replace("{{TITULO}}", "Nueva Solicitud de Descuento")
                .Replace("{{CONTENIDO}}", plantillaSolicitud)
                .Replace("{{AÑO}}", DateTime.Now.Year.ToString());
            
            return correoCompleto;
        }

        public string GenerarPlantillaDescuentoAprobado(SolicitudDescuento solicitud)
        {
            // Cargar las plantillas
            string plantillaBase = File.ReadAllText(Path.Combine(_plantillasPath, "PlantillaBase.html"));
            string plantillaAprobado = File.ReadAllText(Path.Combine(_plantillasPath, "DescuentoAprobado.html"));
            
            // Calcular precio con descuento
            decimal precioOriginal = solicitud.Auto?.Precio ?? 0;
            decimal precioConDescuento = precioOriginal - (precioOriginal * solicitud.PorcentajeSolicitado / 100);
            
            // Reemplazar variables en la plantilla de descuento aprobado
            plantillaAprobado = plantillaAprobado
                .Replace("{{NOMBRE_CLIENTE}}", "Cliente") // Aquí podríamos incluir datos del cliente si se añaden a la solicitud
                .Replace("{{MARCA}}", solicitud.Auto?.Marca ?? "")
                .Replace("{{MODELO}}", solicitud.Auto?.Modelo ?? "")
                .Replace("{{AÑO}}", solicitud.Auto?.Anio.ToString() ?? "")
                .Replace("{{PRECIO}}", CurrencyFormatter.FormatCRC(precioOriginal))
                .Replace("{{DESCUENTO}}", solicitud.PorcentajeSolicitado.ToString("0.##"))
                .Replace("{{PRECIO_DESCUENTO}}", CurrencyFormatter.FormatCRC(precioConDescuento))
                .Replace("{{NOMBRE_VENDEDOR}}", solicitud.Vendedor?.Nombre ?? "Su asesor");
            
            // Integrar plantilla de aprobación en plantilla base
            string correoCompleto = plantillaBase
                .Replace("{{TITULO}}", "Descuento Aprobado")
                .Replace("{{CONTENIDO}}", plantillaAprobado)
                .Replace("{{AÑO}}", DateTime.Now.Year.ToString());
            
            return correoCompleto;
        }

        public string GenerarPlantillaDescuentoRechazado(SolicitudDescuento solicitud)
        {
            // Cargar las plantillas
            string plantillaBase = File.ReadAllText(Path.Combine(_plantillasPath, "PlantillaBase.html"));
            string plantillaRechazado = File.ReadAllText(Path.Combine(_plantillasPath, "DescuentoRechazado.html"));
            
            // Reemplazar variables en la plantilla de descuento rechazado
            plantillaRechazado = plantillaRechazado
                .Replace("{{NOMBRE_CLIENTE}}", "Cliente") // Aquí podríamos incluir datos del cliente si se añaden a la solicitud
                .Replace("{{MARCA}}", solicitud.Auto?.Marca ?? "")
                .Replace("{{MODELO}}", solicitud.Auto?.Modelo ?? "")
                .Replace("{{AÑO}}", solicitud.Auto?.Anio.ToString() ?? "")
                .Replace("{{PRECIO}}", CurrencyFormatter.FormatCRC(solicitud.Auto?.Precio ?? 0))
                .Replace("{{DESCUENTO}}", solicitud.PorcentajeSolicitado.ToString("0.##"))
                .Replace("{{COMENTARIO_GERENTE}}", solicitud.ComentarioGerente ?? "Sin comentarios.")
                .Replace("{{NOMBRE_VENDEDOR}}", solicitud.Vendedor?.Nombre ?? "Su asesor");
            
            // Integrar plantilla de rechazo en plantilla base
            string correoCompleto = plantillaBase
                .Replace("{{TITULO}}", "Respuesta a Solicitud de Descuento")
                .Replace("{{CONTENIDO}}", plantillaRechazado)
                .Replace("{{AÑO}}", DateTime.Now.Year.ToString());
            
            return correoCompleto;
        }
    }
} 