using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Autos.Data;
using Autos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Autos.Services
{
    public interface IEmailService
    {
        Task<bool> EnviarCorreoAsync(string destinatario, string asunto, string contenido, bool esHtml = true);
        Task<bool> EnviarCorreoAsync(List<string> destinatarios, string asunto, string contenido, bool esHtml = true);
        Task<bool> ProbarConexionAsync(ConfiguracionCorreo config);
        Task<ConfiguracionCorreo?> ObtenerConfiguracionAsync();
    }

    public class EmailService : IEmailService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmailService> _logger;

        public EmailService(ApplicationDbContext context, ILogger<EmailService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> EnviarCorreoAsync(string destinatario, string asunto, string contenido, bool esHtml = true)
        {
            return await EnviarCorreoAsync(new List<string> { destinatario }, asunto, contenido, esHtml);
        }

        public async Task<bool> EnviarCorreoAsync(List<string> destinatarios, string asunto, string contenido, bool esHtml = true)
        {
            try
            {
                var config = await ObtenerConfiguracionAsync();
                if (config == null)
                {
                    _logger.LogError("No se pudo enviar el correo: Configuración SMTP no encontrada");
                    return false;
                }

                using (var mensaje = new MailMessage())
                {
                    mensaje.From = new MailAddress(config.EmailRemitente, config.NombreRemitente);
                    
                    // Agregar cada destinatario
                    foreach (var destinatario in destinatarios)
                    {
                        mensaje.To.Add(destinatario);
                    }
                    
                    mensaje.Subject = asunto;
                    mensaje.Body = contenido;
                    mensaje.IsBodyHtml = esHtml;

                    using (var cliente = new SmtpClient(config.Servidor, config.Puerto))
                    {
                        cliente.EnableSsl = config.RequiereSSL;
                        cliente.Credentials = new NetworkCredential(config.Usuario, config.Password);
                        cliente.DeliveryMethod = SmtpDeliveryMethod.Network;
                        
                        await cliente.SendMailAsync(mensaje);
                    }
                }
                
                _logger.LogInformation("Correo enviado exitosamente a {DestinatariosCount} destinatarios", destinatarios.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo: {ErrorMessage}", ex.Message);
                return false;
            }
        }

        public async Task<bool> ProbarConexionAsync(ConfiguracionCorreo config)
        {
            try
            {
                using (var cliente = new SmtpClient(config.Servidor, config.Puerto))
                {
                    cliente.EnableSsl = config.RequiereSSL;
                    cliente.Credentials = new NetworkCredential(config.Usuario, config.Password);
                    cliente.DeliveryMethod = SmtpDeliveryMethod.Network;

                    // Solo probar la conexión sin enviar correo real
                    using (var mensaje = new MailMessage())
                    {
                        mensaje.From = new MailAddress(config.EmailRemitente, config.NombreRemitente);
                        mensaje.To.Add(config.EmailRemitente); // Enviamos al mismo remitente para prueba
                        mensaje.Subject = "Prueba de Conexión SMTP";
                        mensaje.Body = "<h1>Prueba de conexión exitosa</h1><p>Esta es una prueba automática de la conexión SMTP.</p>";
                        mensaje.IsBodyHtml = true;

                        await cliente.SendMailAsync(mensaje);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al probar conexión SMTP: {ErrorMessage}", ex.Message);
                return false;
            }
        }

        public async Task<ConfiguracionCorreo?> ObtenerConfiguracionAsync()
        {
            return await _context.ConfiguracionCorreo.FirstOrDefaultAsync();
        }
    }
} 