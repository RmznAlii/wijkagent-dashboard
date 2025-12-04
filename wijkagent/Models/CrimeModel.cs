namespace WijkAgent.Models;

/// <summary>
/// Model dat één delict representeert binnen de applicatie.
/// Bevat alle gegevens die nodig zijn voor weergave op de kaart
/// en opslag in de in-memory service.
/// </summary>
public class CrimeModel
{
    /// <summary>
    /// Unieke ID van het delict. Wordt automatisch toegewezen.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Het type delict (bijv. "Diefstal")
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    /// Een beschrijving of toelichting van wat er is gebeurd.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Het adres waar het delict heeft plaatsgevonden.
    /// </summary>
    public string Address { get; set; } = "";

    /// <summary>
    /// Tijdstip van het incident in stringformaat 
    /// </summary>
    public string DateTimeString { get; set; } = "";

    /// <summary>
    /// De breedtegraad (latitude) voor plaatsing van de marker op de kaart.
    /// </summary>
    public double Lat { get; set; }

    /// <summary>
    /// De lengtegraad (longitude) voor plaatsing van de marker op de kaart.
    /// </summary>
    public double Lng { get; set; }
}