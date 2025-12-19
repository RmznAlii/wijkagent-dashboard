using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WijkAgent.Core.Data;
using WijkAgent.Core.Models;
using WijkAgent.Core.Services;
using Xunit;

namespace WijkAgent.Tests;

/// <summary>
/// Unit tests voor CRUD- en filterfunctionaliteit van CrimeService.
/// Gebruik SQLite InMemory database voor integratietesten.
/// </summary>
public class CrimeServiceCrimeListTests
{
    // Helper: maak DbContextOptions aan
    private DbContextOptions<WijkAgentDbContext> CreateOptions(SqliteConnection connection)
        => new DbContextOptionsBuilder<WijkAgentDbContext>()
            .UseSqlite(connection)
            .Options;

    // Helper: seed testdata
    private async Task SeedDataAsync(WijkAgentDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        context.Crimes.AddRange(
            new Crime { Type = "Diefstal", City = "Amsterdam", IncidentDateTime = DateTime.Today, CreatedAt = DateTime.Now },
            new Crime { Type = "Overlast", City = "Rotterdam", IncidentDateTime = DateTime.Today.AddDays(-1), CreatedAt = DateTime.Now }
        );

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCrimes()
    {
        // Arrange: maak database en seed data
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = CreateOptions(connection);

        await using (var ctx = new WijkAgentDbContext(options))
        {
            await SeedDataAsync(ctx);
            var service = new CrimeService(ctx);

            // Act: haal alle delicten op
            var crimes = await service.GetAllAsync();

            // Assert: er zijn precies 2 delicten
            Assert.Equal(2, crimes.Count);
        }
    }

    [Fact]
    public async Task GetFilteredAsync_ReturnsCorrectCrime()
    {
        // Arrange: maak database en seed data
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = CreateOptions(connection);

        await using (var ctx = new WijkAgentDbContext(options))
        {
            await SeedDataAsync(ctx);
            var service = new CrimeService(ctx);

            // Act: filter op type "Diefstal"
            var filtered = await service.GetFilteredAsync(null, null, "Diefstal", null);

            // Assert: er is precies 1 resultaat en het type klopt
            Assert.Single(filtered);
            Assert.Equal("Diefstal", filtered[0].Type);
        }
    }
}
