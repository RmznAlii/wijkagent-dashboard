using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WijkAgent.Core.Data;
using WijkAgent.Core.Models;
using WijkAgent.Core.Services;

namespace WijkAgent.Tests;

/// <summary>
/// Integratietesten voor de <see cref="CrimeService"/>.
/// 
/// Deze testen controleren de samenwerking tussen:
/// - CrimeService
/// - Entity Framework Core
/// - SQLite InMemory database
/// 
/// Er worden geen mocks gebruikt, zodat de volledige dataketen wordt getest.
/// </summary>
public class CrimeServiceIntegrationTests
{
    /// <summary>
    /// Maakt DbContextOptions aan voor een SQLite InMemory database.
    /// 
    /// Dezelfde open verbinding moet worden gedeeld tussen DbContexts
    /// om de InMemory database actief te houden gedurende de test.
    /// </summary>
    /// <param name="connection">Open SQLite InMemory connectie.</param>
    /// <returns>Geconfigureerde <see cref="DbContextOptions{WijkAgentDbContext}"/>.</returns>
    private DbContextOptions<WijkAgentDbContext> CreateOptions(SqliteConnection connection)
    {
        return new DbContextOptionsBuilder<WijkAgentDbContext>()
            .UseSqlite(connection)
            .Options;
    }

    /// <summary>
    /// Controleert of het bewerken van een bestaand delict
    /// daadwerkelijk wordt opgeslagen in de database.
    /// 
    /// Scenario:
    /// - Een delict wordt eerst toegevoegd aan de database.
    /// - Daarna wordt hetzelfde delict aangepast via <see cref="CrimeService.UpdateAsync"/>.
    /// - Tot slot wordt gecontroleerd of de wijzigingen correct zijn opgeslagen.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WritesUpdatedCrimeToDatabase()
    {
        // Arrange – SQLite InMemory
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = CreateOptions(connection);
        int crimeId;

        // Database + seed data
        await using (var context = new WijkAgentDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var crime = new Crime
            {
                Type = "Diefstal",
                Description = "Oude beschrijving",
                Street = "Kalverstraat",
                City = "Amsterdam",
                IncidentDateTime = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            context.Crimes.Add(crime);
            await context.SaveChangesAsync();

            crimeId = crime.Id;
        }

        // Act – update via service
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
                IncidentDateTime = DateTime.Now
            };

            var result = await service.UpdateAsync(updatedCrime);

            Assert.NotNull(result);
        }

        // Assert – controleer echte database-status
        await using (var context = new WijkAgentDbContext(options))
        {
            var fromDb = await context.Crimes.FirstAsync(c => c.Id == crimeId);

            Assert.Equal("Vandalisme", fromDb.Type);
            Assert.Equal("Nieuwe beschrijving", fromDb.Description);
            Assert.Equal("Damrak", fromDb.Street);
        }
    }

    /// <summary>
    /// Controleert of het bewerken van een niet-bestaand delict
    /// resulteert in een <c>null</c> waarde.
    /// 
    /// Dit test het foutscenario waarbij een ongeldig ID wordt meegegeven.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenCrimeDoesNotExist()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<WijkAgentDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var ctx = new WijkAgentDbContext(options);
        await ctx.Database.EnsureCreatedAsync();

        var service = new CrimeService(ctx);

        var result = await service.UpdateAsync(new Crime
        {
            Id = 999,
            Type = "Overlast",
            Description = "Bestaat niet",
            IncidentDateTime = DateTime.Now
        });

        Assert.Null(result);
    }
}
