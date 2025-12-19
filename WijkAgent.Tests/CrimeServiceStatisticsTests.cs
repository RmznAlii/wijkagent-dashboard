using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WijkAgent.Core.Data;
using WijkAgent.Core.Models;
using WijkAgent.Core.Services;
using Xunit;

namespace WijkAgent.Tests;

/// <summary>
/// Unit tests voor statistiekfunctionaliteit van CrimeService.
/// Testen met SQLite InMemory database.
/// </summary>
public class CrimeServiceStatisticsTests
{
    // Helper: maak DbContextOptions aan
    private DbContextOptions<WijkAgentDbContext> CreateOptions(SqliteConnection connection)
        => new DbContextOptionsBuilder<WijkAgentDbContext>()
            .UseSqlite(connection)
            .Options;

    // Helper: seed testdata voor statistieken
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

            // Er moeten 4 delicten in totaal zijn
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

            // Diefstal: 2, Overlast: 1
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

            // Amsterdam heeft 2 delicten, Rotterdam 1
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

            // Controleer dat er ten minste één dag met delicten is
            Assert.Contains(perDay, x => x.Count > 0);
        }
    }
}
