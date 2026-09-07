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
        Task EnviarCorreo(string destinatario, string asunto, string cuerpo, byte[] adjunto = null, string nombreAdjunto = null);
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

        public async Task EnviarCorreo(string destinatario, string asunto, string cuerpo, byte[] adjunto = null, string nombreAdjunto = null)
        {
            try
            {
                string tipoCuenta = _configuration["EmailSettings:TipoCuenta"];
                string username = _configuration["EmailSettings:Username"];
                string password = _configuration["EmailSettings:Password"];

                _logger.LogInformation("Tipo de cuenta configurada: {TipoCuenta}", tipoCuenta);
                _logger.LogInformation(
                    "Configuración de correo cargada. SMTPUserPresent={SmtpUserPresent}, SMTPPasswordPresent={SmtpPasswordPresent}, AttachmentBytes={AttachmentBytes}",
                    !string.IsNullOrWhiteSpace(username),
                    !string.IsNullOrWhiteSpace(password),
                    adjunto?.Length ?? 0);

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
                    throw new InvalidOperationException($"No existe configuración SMTP para EmailSettings:TipoCuenta='{tipoCuenta}'");
                }

                Exception ultimoError = null;
                string? sendGridError = null;

                foreach (var config in configuracionesPrueba)
                {
                    try
                    {
                        _logger.LogInformation(
                            "EmailDeliveryAttempt Provider=SMTP SMTPProfile={Profile} Host={Host} Port={Port} StartTls={StartTls} AttachmentBytes={AttachmentBytes}",
                            config.nombre,
                            config.host,
                            config.port,
                            config.ssl,
                            adjunto?.Length ?? 0);

                        var message = new MimeMessage();
                        message.From.Add(MailboxAddress.Parse(username));
                        message.To.Add(MailboxAddress.Parse(destinatario));
                        message.Subject = asunto;
                        var builder = new BodyBuilder { HtmlBody = cuerpo };
                        if (adjunto is { Length: > 0 } && !string.IsNullOrWhiteSpace(nombreAdjunto))
                            builder.Attachments.Add(nombreAdjunto, adjunto);
                        message.Body = builder.ToMessageBody();

                        using var client = new MailKit.Net.Smtp.SmtpClient();
                        client.Timeout = 30000;

                        var secureSocket = config.ssl
                            ? SecureSocketOptions.StartTls
                            : SecureSocketOptions.None;

                        await ConnectWithAddressFallbackAsync(client, config.host, config.port, secureSocket);
                        client.AuthenticationMechanisms.Remove("XOAUTH2");
                        await client.AuthenticateAsync(username, password);
                        await client.SendAsync(message);
                        await client.DisconnectAsync(true);

                        _logger.LogInformation(
                            "EmailDeliverySucceeded Provider=SMTP SMTPProfile={Profile} AttachmentBytes={AttachmentBytes}",
                            config.nombre,
                            adjunto?.Length ?? 0);
                        return;
                    }
                    catch (Exception ex)
                    {
                        ultimoError = ex;
                        _logger.LogWarning(
                            ex,
                            "EmailDeliveryFailed Provider=SMTP SMTPProfile={Profile} Category={Category}",
                            config.nombre,
                            DescribeSmtpError(ex));
                        continue;
                    }
                }

                _logger.LogInformation(
                    "EmailFallbackStarted FromProvider=SMTP ToProvider=SendGrid AttachmentBytes={AttachmentBytes}",
                    adjunto?.Length ?? 0);
                var sendGridResult = await TrySendWithSendGridAsync(destinatario, asunto, cuerpo, username, adjunto, nombreAdjunto);
                if (sendGridResult.Success)
                {
                    _logger.LogInformation(
                        "EmailDeliverySucceeded Provider=SendGrid Mode=Fallback AttachmentBytes={AttachmentBytes}",
                        adjunto?.Length ?? 0);
                    return;
                }
                sendGridError = DescribeSendGridError(sendGridResult.Error);

                var smtpError = ultimoError == null ? "Error SMTP desconocido" : DescribeSmtpError(ultimoError);
                var sendGridErrorFinal = string.IsNullOrWhiteSpace(sendGridError)
                    ? "SendGrid no configurado o rechazado."
                    : sendGridError;

                throw new Exception($"No se pudo enviar el email. SMTP: {smtpError}. SendGrid: {sendGridErrorFinal}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmailDeliveryFailed Provider=All");
                throw;
            }
        }

        private static string DescribeSmtpError(Exception ex)
        {
            var detail = ex?.ToString() ?? string.Empty;
            if (detail.Contains("535", StringComparison.OrdinalIgnoreCase)
                || detail.Contains("Authentication unsuccessful", StringComparison.OrdinalIgnoreCase)
                || detail.Contains("authentication", StringComparison.OrdinalIgnoreCase))
            {
                return "Autenticación SMTP rechazada (535). Verifica EmailSettings:Username/Password o los secretos del despliegue.";
            }

            if (detail.Contains("timed out", StringComparison.OrdinalIgnoreCase)
                || detail.Contains("timeout", StringComparison.OrdinalIgnoreCase))
            {
                return "Tiempo de espera agotado al conectar o enviar por SMTP.";
            }

            return string.IsNullOrWhiteSpace(ex?.Message) ? "Error SMTP desconocido." : ex.Message;
        }

        private static string DescribeSendGridError(string error)
        {
            var detail = error ?? string.Empty;
            if (detail.Contains("413", StringComparison.OrdinalIgnoreCase))
                return "SendGrid rechazó el tamaño del mensaje o adjunto (413).";
            if (detail.Contains("401", StringComparison.OrdinalIgnoreCase)
                || detail.Contains("403", StringComparison.OrdinalIgnoreCase))
                return "SendGrid rechazó la autenticación o autorización de la API.";
            return string.IsNullOrWhiteSpace(detail) ? "SendGrid no configurado o rechazado." : detail;
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

        private async Task<(bool Success, string Error)> TrySendWithSendGridAsync(string destinatario, string asunto, string cuerpoHtml, string defaultFrom, byte[] adjunto = null, string nombreAdjunto = null)
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
                _logger.LogWarning("EmailDeliverySkipped Provider=SendGrid Reason=MissingConfiguration");
                return (false, "Falta EmailSettings:SendGrid:ApiKey o EmailSettings:SendGrid:From.");
            }

            try
            {
                _logger.LogInformation(
                    "EmailDeliveryAttempt Provider=SendGrid Mode=Fallback AttachmentBytes={AttachmentBytes}",
                    adjunto?.Length ?? 0);
                var client = new SendGridClient(apiKey);
                var fromEmail = new EmailAddress(from, fromName);
                var toEmail = new EmailAddress(destinatario);
                var plainText = "Notificación institucional SNIER.";
                var msg = MailHelper.CreateSingleEmail(fromEmail, toEmail, asunto, plainText, cuerpoHtml);
                if (adjunto is { Length: > 0 } && !string.IsNullOrWhiteSpace(nombreAdjunto))
                {
                    var tipo = nombreAdjunto.EndsWith(".pptx", StringComparison.OrdinalIgnoreCase)
                        ? "application/vnd.openxmlformats-officedocument.presentationml.presentation"
                        : "application/pdf";
                    msg.AddAttachment(nombreAdjunto, Convert.ToBase64String(adjunto), tipo, "attachment");
                }
                var response = await client.SendEmailAsync(msg);

                if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
                {
                    _logger.LogInformation(
                        "EmailDeliveryAccepted Provider=SendGrid StatusCode={StatusCode} AttachmentBytes={AttachmentBytes}",
                        (int)response.StatusCode,
                        adjunto?.Length ?? 0);
                    return (true, string.Empty);
                }

                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogWarning(
                    "EmailDeliveryRejected Provider=SendGrid StatusCode={StatusCode} AttachmentBytes={AttachmentBytes} Body={Body}",
                    (int)response.StatusCode,
                    adjunto?.Length ?? 0,
                    responseBody);
                return (false, $"Status {(int)response.StatusCode}: {responseBody}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmailDeliveryFailed Provider=SendGrid Mode=Fallback");
                return (false, ex.Message);
            }
        }
    }
}
