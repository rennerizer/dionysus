using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;

namespace HealthCheck
{
    public class CustomHealthCheckOptions : HealthCheckOptions
    {
        public CustomHealthCheckOptions() : base()
        {
            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = MediaTypeNames.Application.Json;
                context.Response.StatusCode = StatusCodes.Status200OK;

                var result = JsonSerializer.Serialize(new
                {
                    checks = report.Entries.Select(entry => new
                    {
                        name = entry.Key,

                        responseTime = entry.Value.Duration.TotalMilliseconds,

                        status = entry.Value.Status.ToString(),

                        description = entry.Value.Description
                    }),

                    totalStatus = report.Status,

                    totalResponseTime = report.TotalDuration.TotalMilliseconds
                }, jsonSerializerOptions);

                await context.Response.WriteAsync(result);
            };
        }
    }
}
