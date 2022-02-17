using AdvertApi.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AdvertApi.HealthChecks
{
    public class StorageHealthCheck : IHealthCheck
    {
        private readonly IAdvertStorageService advertStorageService;

        public StorageHealthCheck(IAdvertStorageService advertStorageService)
        {
            this.advertStorageService = advertStorageService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var isStorageOk = await advertStorageService.CheckHealthAsync();
            return isStorageOk ? HealthCheckResult.Healthy("Storage Health Healthy") : HealthCheckResult.Unhealthy("Storage Health UnHealthy");
        }
    }
}
