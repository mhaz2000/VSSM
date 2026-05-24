using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSSM.Domain.Enums;
using VSSM.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace VSSM.Application.Services.BackgroundServices
{
    public class OrdersInquiryBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrdersInquiryBackgroundService> _logger;

        public OrdersInquiryBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<OrdersInquiryBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
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
            var unprocessedOrders = await (from o in context.TblOrderInfo
                                           where o.Status == OrderInfoStatus.READ.ToString()
                                           join u in context.TblUidInfo on o.PkOrderInfo equals u.FkOrderInfo
                                           select new
                                           {
                                               Order = o,
                                               UidInfo = u
                                           }).ToListAsync(stoppingToken);

            if (!unprocessedOrders.Any())
            {
                _logger.LogInformation("No unprocessed orders found");
                return;
            }

            _logger.LogInformation("Found {Count} unprocessed orders", unprocessedOrders.Count);

            foreach (var item in unprocessedOrders)
            {
                _logger.LogInformation("Processing order {OrderId}", item.Order.PkOrderInfo);
            }

            await context.SaveChangesAsync(stoppingToken);
        }
    }
}