using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WijkAgent.Core.Data;
using WijkAgent.Core.Models;
using WijkAgent.Core.Services;
using Xunit;

namespace WijkAgent.Tests;

public class CrimeServiceStatisticsTests
{
    private DbContextOptions<WijkAgentDbContext> CreateOptions(SqliteConnection connection)
        => new DbContextOptionsBuilder<WijkAgentDbContext>()
            .UseSqlite(connection)
            .Options;

    private async Task SeedDataAsync(WijkAgentDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        var now = DateTime.Now;

        context.Crimes.AddRange(
            new Crime { Type = "Diefstal", City = "Amsterdam", IncidentDateTime = now.AddDays(-1), CreatedAt = now },
            new Crime { Type = "Overlast", City = "Amsterdam", IncidentDateTime = now.AddDays(-2), CreatedAt = now },
            new Crime { Type = "Diefstal", City = "Rotterdam", IncidentDateTime = now.AddDays(-1), CreatedAt = now },
            new Crime { Type = "Vandalisme", City = "Den Haag", IncidentDateTime = now.AddDays(-3), CreatedAt = now }
        );

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetTotalCountAsync_ReturnsCorrectNumber()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = CreateOptions(connection);

        await using (var ctx = new WijkAgentDbContext(options))
        {
            await SeedDataAsync(ctx);
            var service = new CrimeService(ctx);

            var total = await service.GetTotalCountAsync();
            Assert.Equal(4, total);
        }
    }

    [Fact]
    public async Task GetCountsByTypeAsync_ReturnsCorrectCounts()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = CreateOptions(connection);

        await using (var ctx = new WijkAgentDbContext(options))
        {
            await SeedDataAsync(ctx);
            var service = new CrimeService(ctx);

            var counts = await service.GetCountsByTypeAsync();
            Assert.Contains(counts, x => x.Type == "Diefstal" && x.Count == 2);
            Assert.Contains(counts, x => x.Type == "Overlast" && x.Count == 1);
        }
    }

    [Fact]
    public async Task GetTopCitiesAsync_ReturnsCorrectCounts()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = CreateOptions(connection);

        await using (var ctx = new WijkAgentDbContext(options))
        {
            await SeedDataAsync(ctx);
            var service = new CrimeService(ctx);

            var topCities = await service.GetTopCitiesAsync();
            Assert.Contains(topCities, x => x.City == "Amsterdam" && x.Count == 2);
            Assert.Contains(topCities, x => x.City == "Rotterdam" && x.Count == 1);
        }
    }

    [Fact]
    public async Task GetCountsPerDayAsync_ReturnsCorrectCounts()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = CreateOptions(connection);

        await using (var ctx = new WijkAgentDbContext(options))
        {
            await SeedDataAsync(ctx);
            var service = new CrimeService(ctx);

            var perDay = await service.GetCountsPerDayAsync(7);
            Assert.Contains(perDay, x => x.Count > 0); // Minimaal één entry
        }
    }
}
