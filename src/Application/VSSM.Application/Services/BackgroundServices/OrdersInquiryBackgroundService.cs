using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSSM.Domain.Enums;
using VSSM.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.Integrations.Behdasht;

namespace VSSM.Application.Services.BackgroundServices
{
    public class OrdersInquiryBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrdersInquiryBackgroundService> _logger;
        private readonly BehdashtApiClient _behdashtClient;

        public OrdersInquiryBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<OrdersInquiryBackgroundService> logger,
            BehdashtApiClient behdashtClient)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _behdashtClient = behdashtClient;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var nextRun = now.Date.AddHours(2);

                    if (now > nextRun)
                        nextRun = nextRun.AddDays(1);

                    var delay = nextRun - now;
                    _logger.LogInformation("Next execution at {NextRun}, waiting {Delay} hours", nextRun, delay.TotalHours);
                    await Task.Delay(delay, stoppingToken);

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<VSSMDbContext>();
                        await ProcessUnprocessedOrders(context, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing orders");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private async Task ProcessUnprocessedOrders(VSSMDbContext context, CancellationToken stoppingToken)
        {
            var orders = await context.TblOrderInfo
                .Where(o => o.Status == OrderInfoStatus.READ.ToString())
                .ToListAsync(stoppingToken);

            if (!orders.Any())
            {
                _logger.LogInformation("No unprocessed orders found");
                return;
            }

            _logger.LogInformation("Found {Count} unprocessed orders", orders.Count);

            foreach (var order in orders)
            {
                var uids = await context.TblUidInfo
                    .Where(u => u.FkOrderInfo == order.PkOrderInfo)
                    .ToListAsync(stoppingToken);

                if (!uids.Any())
                {
                    _logger.LogInformation("Order {OrderId} has no UIDs; skipping", order.PkOrderInfo);
                    continue;
                }

                order.Status = OrderInfoStatus.PROCESS_VALIDATING.ToString();
                await context.SaveChangesAsync(stoppingToken);

                var hasError = false;
                foreach (var uid in uids)
                {
                    try
                    {
                        var ok = await _behdashtClient.ValidateUidAsync(uid.SecLevelUid, stoppingToken);
                        if (!ok)
                        {
                            hasError = true;
                            _logger.LogWarning("UID {Uid} validation failed for order {OrderId}", uid.SecLevelUid, order.PkOrderInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        hasError = true;
                        _logger.LogError(ex, "Error validating UID {Uid} for order {OrderId}", uid.SecLevelUid, order.PkOrderInfo);
                    }
                }

                order.Status = hasError ? OrderInfoStatus.HAS_ERROR.ToString() : OrderInfoStatus.IS_VALID.ToString();
                await context.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Order {OrderId} processed. Status set to {Status}", order.PkOrderInfo, order.Status);
            }
        }
    }
}
