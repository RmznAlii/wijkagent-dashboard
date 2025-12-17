using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WijkAgent.Core.Data;
using WijkAgent.Core.Models;
using WijkAgent.Core.Services;

namespace WijkAgent.Tests;

/// <summary>
/// Integratietesten voor de <see cref="CrimeService"/>.
/// Testen de volledige samenwerking tussen CrimeService, EF Core en SQLite InMemory.
/// </summary>
public class CrimeServiceIntegrationTests
{
    private DbContextOptions<WijkAgentDbContext> CreateOptions(SqliteConnection connection)
        => new DbContextOptionsBuilder<WijkAgentDbContext>()
            .UseSqlite(connection)
            .Options;

    [Fact]
    public async Task UpdateAsync_WritesUpdatedCrimeToDatabase()
    {
        // Arrange
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = CreateOptions(connection);

        int crimeId;
        await using (var context = new WijkAgentDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var crime = new Crime
            {
                Type = "Diefstal",
                Description = "Oude beschrijving",
                Street = "Kalverstraat",
                City = "Amsterdam",
                IncidentDateTime = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            context.Crimes.Add(crime);
            await context.SaveChangesAsync();
            crimeId = crime.Id;
        }

        // Act
        await using (var context = new WijkAgentDbContext(options))
        {
            var service = new CrimeService(context);
            var updatedCrime = new Crime
            {
                Id = crimeId,
                Type = "Vandalisme",
                Description = "Nieuwe beschrijving",
                Street = "Damrak",
                City = "Amsterdam",
                IncidentDateTime = DateTime.UtcNow
            };

            var result = await service.UpdateAsync(updatedCrime);
            Assert.NotNull(result);
        }

        // Assert
        await using (var context = new WijkAgentDbContext(options))
        {
            var crimeFromDb = await context.Crimes.FirstAsync(c => c.Id == crimeId);
            Assert.Equal("Vandalisme", crimeFromDb.Type);
            Assert.Equal("Nieuwe beschrijving", crimeFromDb.Description);
            Assert.Equal("Damrak", crimeFromDb.Street);
        }
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenCrimeDoesNotExist()
    {
        // Arrange
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = CreateOptions(connection);

        await using var context = new WijkAgentDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var service = new CrimeService(context);

        // Act
        var result = await service.UpdateAsync(new Crime
        {
            Id = 999,
            Type = "Overlast",
            Description = "Bestaat niet",
            IncidentDateTime = DateTime.UtcNow
        });

        // Assert
        Assert.Null(result);
    }
}
