using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthCheck
{
    public class ICMPHealthCheck : IHealthCheck
    {
        private string _host;
        private int _timeout;

        public ICMPHealthCheck(string host, int timeout)
        {
            _host = host;
            _timeout = timeout;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(_host);

                    switch (reply.Status)
                    {
                        case IPStatus.Success:
                            var message = $"ICMP to {_host} took {reply.RoundtripTime} ms.";

                            return (reply.RoundtripTime) > _timeout ? HealthCheckResult.Degraded(message) : HealthCheckResult.Healthy(message);

                        default:
                            var error = $"ICMP to {_host} failed: {reply.Status}.";

                            return HealthCheckResult.Unhealthy(error);
                    }
                }
            }

            catch (Exception exception)
            {
                var error = $"ICMP to {_host} failed: {exception.Message}.";

                return HealthCheckResult.Unhealthy();
            }
        }
    }
}
