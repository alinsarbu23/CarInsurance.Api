using CarInsurance.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class PolicyExpirationLoggerServiceTests
{
    [Fact]
    public async Task LogsExpiredPolicies()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        // Creează ServiceCollection pentru DI
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        var provider = services.BuildServiceProvider();

        // Seed policy expirată
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Policies.Add(new CarInsurance.Api.Models.InsurancePolicy
            {
                CarId = 1,
                StartDate = new DateOnly(2024, 1, 1),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
            });
            db.SaveChanges();
        }

        var mockLogger = new Mock<ILogger<PolicyExpirationLoggerService>>();

        var service = new PolicyExpirationLoggerService(provider.GetRequiredService<IServiceScopeFactory>(), mockLogger.Object);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(200); // stop după 200ms

        await service.StartAsync(cts.Token);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("expired")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }
}
