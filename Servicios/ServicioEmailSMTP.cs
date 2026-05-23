using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NSIE.Servicios
{
    public interface IServicioEmailSMTP
    {
        Task EnviarCorreo(string destinatario, string asunto, string cuerpo);
    }

    public class ServicioEmailSmtp : IServicioEmailSMTP
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServicioEmailSmtp> _logger;

        public ServicioEmailSmtp(IConfiguration configuration, ILogger<ServicioEmailSmtp> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task EnviarCorreo(string destinatario, string asunto, string cuerpo)
        {
            try
            {
                string tipoCuenta = _configuration["EmailSettings:TipoCuenta"];
                string username = _configuration["EmailSettings:Username"];
                string password = _configuration["EmailSettings:Password"];

                _logger.LogInformation("Tipo de cuenta configurada: {TipoCuenta}", tipoCuenta);
                _logger.LogInformation("Usuario de envío: {Usuario}", username);
                Console.WriteLine($"Tipo de cuenta configurada: {tipoCuenta}");
                Console.WriteLine($"Usuario de envío: {username}");

                var configuracionesPrueba = new List<(string nombre, string host, int port, bool ssl)>();

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

                    configuracionesPrueba.Add(("Outlook",
                        _configuration["EmailSettings:SmtpOutlook:Host"],
                        int.Parse(_configuration["EmailSettings:SmtpOutlook:Port"]),
                        bool.Parse(_configuration["EmailSettings:SmtpOutlook:EnableSsl"])));

                    configuracionesPrueba.Add(("Exchange",
                        _configuration["EmailSettings:SmtpExchange:Host"],
                        int.Parse(_configuration["EmailSettings:SmtpExchange:Port"]),
                        bool.Parse(_configuration["EmailSettings:SmtpExchange:EnableSsl"])));
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

                if (!configuracionesPrueba.Any())
                {
                    _logger.LogWarning("No se encontró configuración SMTP para tipo de cuenta: {TipoCuenta}", tipoCuenta);
                    Console.WriteLine($"⚠️ No se encontró configuración SMTP para tipo de cuenta: {tipoCuenta}");
                    throw new InvalidOperationException($"No existe configuración SMTP para EmailSettings:TipoCuenta='{tipoCuenta}'");
                }

                Exception ultimoError = null;
                string? sendGridError = null;

                var sendGridApiKey = _configuration["EmailSettings:SendGrid:ApiKey"]
                                     ?? _configuration["SEND_GRID_API_KEY"];
                var sendGridFrom = _configuration["EmailSettings:SendGrid:From"]
                                   ?? _configuration["SEND_GRID_FROM"]
                                   ?? username;
                var sendGridConfigured = !string.IsNullOrWhiteSpace(sendGridApiKey)
                                         && !string.IsNullOrWhiteSpace(sendGridFrom);

                if (sendGridConfigured)
                {
                    _logger.LogInformation("Intentando envío primero con SendGrid para reducir latencia de timeouts SMTP.");
                    var sendGridFirst = await TrySendWithSendGridAsync(destinatario, asunto, cuerpo, username);
                    if (sendGridFirst.Success)
                    {
                        _logger.LogInformation("Email enviado exitosamente usando SendGrid (prioritario).");
                        return;
                    }

                    sendGridError = sendGridFirst.Error;
                    _logger.LogWarning("SendGrid prioritario no pudo enviar. Se intentará SMTP. Detalle: {Detalle}", sendGridError);
                }

                foreach (var config in configuracionesPrueba)
                {
                    _logger.LogInformation("Configuración registrada: {Nombre} - {Host}:{Port} SSL={Ssl}", config.nombre, config.host, config.port, config.ssl);
                    Console.WriteLine($"Configuración registrada: {config.nombre} - {config.host}:{config.port} SSL={config.ssl}");

                    try
                    {
                        _logger.LogInformation("Intentando envío con {Nombre} ({Host}:{Port})", config.nombre, config.host, config.port);
                        Console.WriteLine($"Intentando envío con: {config.nombre} ({config.host}:{config.port})");

                        var message = new MimeMessage();
                        message.From.Add(MailboxAddress.Parse(username));
                        message.To.Add(MailboxAddress.Parse(destinatario));
                        message.Subject = asunto;
                        message.Body = new BodyBuilder
                        {
                            HtmlBody = cuerpo
                        }.ToMessageBody();

                        using var client = new MailKit.Net.Smtp.SmtpClient();
                        client.Timeout = 30000;
                        client.ServerCertificateValidationCallback = (_, _, _, _) => true;

                        var secureSocket = config.ssl
                            ? SecureSocketOptions.StartTls
                            : SecureSocketOptions.None;

                        await ConnectWithAddressFallbackAsync(client, config.host, config.port, secureSocket);
                        client.AuthenticationMechanisms.Remove("XOAUTH2");
                        await client.AuthenticateAsync(username, password);
                        await client.SendAsync(message);
                        await client.DisconnectAsync(true);

                        _logger.LogInformation("Email enviado exitosamente usando {Nombre}", config.nombre);
                        Console.WriteLine($"✅ Email enviado exitosamente usando {config.nombre}");
                        return;
                    }
                    catch (Exception ex)
                    {
                        ultimoError = ex;
                        _logger.LogError(ex, "Error con {Nombre}: {Mensaje}", config.nombre, ex.Message);
                        Console.WriteLine($"❌ Error con {config.nombre}: {ex.Message}");

                        if (ex.Message.Contains("authentication") || ex.Message.Contains("5.7."))
                        {
                            continue;
                        }

                        continue;
                    }
                }

                if (!sendGridConfigured)
                {
                    var sendGridResult = await TrySendWithSendGridAsync(destinatario, asunto, cuerpo, username);
                    if (sendGridResult.Success)
                    {
                        _logger.LogInformation("Email enviado exitosamente usando SendGrid fallback.");
                        return;
                    }

                    sendGridError = sendGridResult.Error;
                }

                var smtpError = ultimoError?.Message ?? "Error SMTP desconocido";
                var sendGridErrorFinal = string.IsNullOrWhiteSpace(sendGridError)
                    ? "SendGrid no configurado o rechazado."
                    : sendGridError;

                throw new Exception($"No se pudo enviar el email. SMTP: {smtpError}. SendGrid: {sendGridErrorFinal}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR AL ENVIAR CORREO");
                Console.WriteLine($"ERROR AL ENVIAR CORREO: {ex.Message}");
                throw;
            }
        }

        private async Task ConnectWithAddressFallbackAsync(
            MailKit.Net.Smtp.SmtpClient client,
            string host,
            int port,
            SecureSocketOptions secureSocket)
        {
            Exception? lastError = null;

            IPAddress[] resolved;
            try
            {
                resolved = await Dns.GetHostAddressesAsync(host);
            }
            catch
            {
                resolved = Array.Empty<IPAddress>();
            }

            var targets = new List<string>();
            targets.AddRange(resolved
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .Select(ip => ip.ToString()));
            targets.AddRange(resolved
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetworkV6)
                .Select(ip => ip.ToString()));

            if (!targets.Any())
            {
                targets.Add(host);
            }

            foreach (var target in targets.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    _logger.LogInformation("Intentando conexión SMTP a {Target}:{Port}", target, port);
                    await client.ConnectAsync(target, port, secureSocket);
                    return;
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    _logger.LogWarning(ex, "Fallo de conexión SMTP a {Target}:{Port}", target, port);
                }
            }

            throw lastError ?? new Exception($"No fue posible conectar a {host}:{port}");
        }

        private async Task<(bool Success, string Error)> TrySendWithSendGridAsync(string destinatario, string asunto, string cuerpoHtml, string defaultFrom)
        {
            var apiKey = _configuration["EmailSettings:SendGrid:ApiKey"]
                         ?? _configuration["SEND_GRID_API_KEY"];
            var from = _configuration["EmailSettings:SendGrid:From"]
                       ?? _configuration["SEND_GRID_FROM"]
                       ?? defaultFrom;
            var fromName = _configuration["EmailSettings:SendGrid:FromName"]
                           ?? _configuration["SEND_GRID_NOMBRE"]
                           ?? "SNIER";

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(from))
            {
                _logger.LogWarning("SendGrid fallback no configurado. Falta ApiKey o remitente.");
                return (false, "Falta EmailSettings:SendGrid:ApiKey o EmailSettings:SendGrid:From.");
            }

            try
            {
                var client = new SendGridClient(apiKey);
                var fromEmail = new EmailAddress(from, fromName);
                var toEmail = new EmailAddress(destinatario);
                var plainText = "Notificación institucional SNIER.";
                var msg = MailHelper.CreateSingleEmail(fromEmail, toEmail, asunto, plainText, cuerpoHtml);
                var response = await client.SendEmailAsync(msg);

                if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
                {
                    return (true, string.Empty);
                }

                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("SendGrid fallback falló con status {StatusCode}. Body: {Body}", response.StatusCode, responseBody);
                return (false, $"Status {(int)response.StatusCode}: {responseBody}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en fallback SendGrid.");
                return (false, ex.Message);
            }
        }
    }
}
