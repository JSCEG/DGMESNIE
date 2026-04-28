
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace NSIE.Servicios
{

    public interface IServicioEmailSMTP
    {
        Task EnviarCorreo(string destinatario, string asunto, string cuerpo);
    }

    // public class ServicioEmailSmtp : IServicioEmailSMTP
    // {
    //     private readonly IConfiguration _configuration;

    //     public ServicioEmailSmtp(IConfiguration configuration)
    //     {
    //         _configuration = configuration;
    //     }

    //     public async Task EnviarCorreo(string destinatario, string asunto, string cuerpo)
    //     {
    //         var client = new SmtpClient("smtp.office365.com", 587)
    //         {
    //             Credentials = new NetworkCredential(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"]),
    //             EnableSsl = true
    //         };

    //         var mailMessage = new MailMessage
    //         {
    //             From = new MailAddress(_configuration["EmailSettings:Username"]),
    //             Subject = asunto,
    //             Body = cuerpo,
    //             IsBodyHtml = true,
    //         };
    //         mailMessage.To.Add(destinatario);

    //         await client.SendMailAsync(mailMessage);
    //     }
    // }
    public class ServicioEmailSmtp : IServicioEmailSMTP
    {
        private readonly IConfiguration _configuration;

        public ServicioEmailSmtp(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarCorreo(string destinatario, string asunto, string cuerpo)
        {
            try
            {
                string tipoCuenta = _configuration["EmailSettings:TipoCuenta"];
                string username = _configuration["EmailSettings:Username"];
                string password = _configuration["EmailSettings:Password"];

                Console.WriteLine($"Tipo de cuenta configurada: {tipoCuenta}");
                Console.WriteLine($"Usuario de envío: {username}");

                // Lista de configuraciones a probar en orden de prioridad
                var configuracionesPrueba = new List<(string nombre, string host, int port, bool ssl)>();

                if (!configuracionesPrueba.Any())
                {
                    Console.WriteLine("⚠️ No se encontró ninguna configuración de SMTP para el tipo de cuenta proporcionado.");
                }

                if (tipoCuenta == "Proton")
                {
                    configuracionesPrueba.Add(("Proton",
                        _configuration["EmailSettings:SmtpProton:Host"],
                        int.Parse(_configuration["EmailSettings:SmtpProton:Port"]),
                        bool.Parse(_configuration["EmailSettings:SmtpProton:EnableSsl"])));
                }
                else if (tipoCuenta == "Gmail")
                {
                    configuracionesPrueba.Add(("Gmail",
                        _configuration["EmailSettings:SmtpGmail:Host"],
                        int.Parse(_configuration["EmailSettings:SmtpGmail:Port"]),
                        bool.Parse(_configuration["EmailSettings:SmtpGmail:EnableSsl"])));
                }
                else if (tipoCuenta == "Office365")
                {
                    configuracionesPrueba.Add(("Office365",
                        _configuration["EmailSettings:SmtpOffice365:Host"],
                        int.Parse(_configuration["EmailSettings:SmtpOffice365:Port"]),
                        bool.Parse(_configuration["EmailSettings:SmtpOffice365:EnableSsl"])));

                    // Fallback a Exchange interno sin SSL
                    configuracionesPrueba.Add(("Exchange-NoSSL",
                        _configuration["EmailSettings:SmtpExchange:Host"],
                        25, false));
                }
                else if (tipoCuenta == "Exchange")
                {
                    configuracionesPrueba.Add(("Exchange",
                        _configuration["EmailSettings:SmtpExchange:Host"],
                        int.Parse(_configuration["EmailSettings:SmtpExchange:Port"]),
                        bool.Parse(_configuration["EmailSettings:SmtpExchange:EnableSsl"])));
                }
                else if (tipoCuenta == "OutlookBasic")
                {
                    configuracionesPrueba.Add(("Outlook",
                        _configuration["EmailSettings:SmtpOutlook:Host"],
                        int.Parse(_configuration["EmailSettings:SmtpOutlook:Port"]),
                        bool.Parse(_configuration["EmailSettings:SmtpOutlook:EnableSsl"])));
                }

                Exception ultimoError = null;

                foreach (var config in configuracionesPrueba)
                {
                    Console.WriteLine($"Configuración registrada: {config.nombre} - {config.host}:{config.port} SSL={config.ssl}");
                    try
                    {
                        Console.WriteLine($"Intentando envío con: {config.nombre} ({config.host}:{config.port})");

                        using var client = new SmtpClient(config.host, config.port)
                        {
                            Credentials = new NetworkCredential(username, password),
                            EnableSsl = config.ssl,
                            Timeout = 30000,
                            DeliveryMethod = SmtpDeliveryMethod.Network
                        };

                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress(username),
                            Subject = asunto,
                            Body = cuerpo,
                            IsBodyHtml = true
                        };
                        mailMessage.To.Add(destinatario);

                        await client.SendMailAsync(mailMessage);
                        Console.WriteLine($"✅ Email enviado exitosamente usando {config.nombre}");
                        return; // Éxito, salir del bucle
                    }
                    catch (Exception ex)
                    {
                        ultimoError = ex;
                        Console.WriteLine($"❌ Error con {config.nombre}: {ex.Message}");

                        // Si es un error de autenticación, intentar siguiente configuración
                        if (ex.Message.Contains("authentication") || ex.Message.Contains("5.7."))
                        {
                            continue;
                        }
                        // Para otros errores, también continuar
                        continue;
                    }
                }

                // Si llegamos aquí, todas las configuraciones fallaron
                throw new Exception($"No se pudo enviar el email con ninguna configuración. Último error: {ultimoError?.ToString() ?? "Error desconocido"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR AL ENVIAR CORREO: {ex.Message}");
                throw;
            }
        }

    }

}
