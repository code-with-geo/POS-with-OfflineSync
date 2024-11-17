using Microsoft.Extensions.Hosting;
using POS.Services;
using System.Threading;
using System.Threading.Tasks;


namespace POS.Class
{
    public class SyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public SyncBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var syncService = scope.ServiceProvider.GetRequiredService<ProductSyncService>();
                await syncService.SyncOfflineProducts();
            }
        }
    }
}
