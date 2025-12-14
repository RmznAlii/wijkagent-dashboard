using Microsoft.EntityFrameworkCore;
using WijkAgent.Core.Models;

namespace WijkAgent.Core.Data;


/// <summary>
/// De centrale databasecontext voor de WijkAgent-applicatie.
/// 
/// Verantwoordelijkheden:
/// - Maakt verbinding met de database (via MySQL / Entity Framework Core).
/// - Bevat DbSets voor alle database-entiteiten.
/// - Wordt gebruikt door services (zoals CrimeService) om CRUD-operaties uit te voeren.
/// 
/// Mapping naar database:
/// - DbSet<Crime> → Tabel 'Crimes'
/// </summary>
public class WijkAgentDbContext : DbContext
{
    /// <summary>
    /// Constructor voor dependency injection.
    /// Ontvangt via DI de databaseconfiguratie (connectionstring + provider).
    /// </summary>
    public WijkAgentDbContext(DbContextOptions<WijkAgentDbContext> options) : base(options) { }

    /// <summary>
    /// Representatie van de database-tabel 'Crimes'.
    /// Hiermee kunnen alle delicten worden opgehaald, toegevoegd, aangepast of verwijderd.
    /// </summary>
    public DbSet<Crime> Crimes { get; set; } = null!;
}