using Microsoft.AspNetCore.Http;
using System.Net;

namespace NSIE.Servicios
{
    public static class ClienteIpHelper
    {
        public static string ObtenerIpCliente(HttpContext httpContext)
        {
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].ToString();
            var realIp = httpContext.Request.Headers["X-Real-IP"].ToString();

            var candidate = ObtenerPrimerValor(forwardedFor)
                ?? ObtenerPrimerValor(realIp)
                ?? httpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrWhiteSpace(candidate))
            {
                return string.Empty;
            }

            if (!IPAddress.TryParse(candidate, out var parsedIp))
            {
                return candidate;
            }

            if (IPAddress.IsLoopback(parsedIp))
            {
                return IPAddress.Loopback.ToString();
            }

            if (parsedIp.IsIPv4MappedToIPv6)
            {
                return parsedIp.MapToIPv4().ToString();
            }

            return parsedIp.ToString();
        }

        private static string ObtenerPrimerValor(string headerValue)
        {
            if (string.IsNullOrWhiteSpace(headerValue))
            {
                return null;
            }

            var values = headerValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return values.Length > 0 ? values[0] : null;
        }
    }
}
