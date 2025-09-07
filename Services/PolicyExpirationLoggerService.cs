using CarInsurance.Api.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

public class PolicyExpirationLoggerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PolicyExpirationLoggerService> _logger;
    private readonly HashSet<long> _processedPolicies = new(); 

    public PolicyExpirationLoggerService(IServiceScopeFactory scopeFactory, ILogger<PolicyExpirationLoggerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var expiredPolicies = await db.Policies
                .Where(p => p.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
                .ToListAsync(stoppingToken);

            foreach (var policy in expiredPolicies)
            {
                if (_processedPolicies.Add(policy.Id)) 
                {
                    _logger.LogInformation($"Policy {policy.Id} expired for car {policy.CarId}");
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken); 
        }
    }
}
