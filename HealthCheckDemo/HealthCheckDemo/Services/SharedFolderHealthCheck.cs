using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheckDemo.Services
{
    public class SharedFolderHealthCheck : IHealthCheck
    {
        private string _folderPath;
        private Dictionary<string, object> _healthCheckData;

        public SharedFolderHealthCheck(string folderPath)
        {
            _folderPath = folderPath;
            _healthCheckData = new Dictionary<string, object>
            {
                { "filePath", _folderPath }
            };
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var testFile = $"{_folderPath}\\test.txt";
                var fs = File.Create(testFile);
                fs.Close();
                File.Delete(testFile);

                return Task.FromResult(HealthCheckResult.Healthy());
            }
            catch (Exception e)
            {
                switch (context.Registration.FailureStatus)
                {
                    case HealthStatus.Degraded:
                        return Task.FromResult(HealthCheckResult.Degraded($"error when writing to file path", e, _healthCheckData));
                    case HealthStatus.Healthy:
                        return Task.FromResult(HealthCheckResult.Healthy($"error when writing to file path", _healthCheckData));
                    default:
                        return Task.FromResult(HealthCheckResult.Unhealthy($"error when writing to file path", e, _healthCheckData));
                }
            }
        }
    }
}
