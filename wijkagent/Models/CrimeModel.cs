namespace WijkAgent.Models
{
    /// <summary>
    /// Lightweight DTO (Data Transfer Object) gebruikt in de UI-laag.
    /// 
    /// Doel:
    /// - Wordt gebruikt om delictgegevens door te geven vanuit JavaScript
    ///   of andere niet-DB gerelateerde processen.
    /// - Bevat enkel de informatie die direct nodig is voor weergave of kaartinteractie.
    ///
    /// Dit model wordt niet als database-entiteit gebruikt (in tegenstelling tot <see cref="Crime"/>).
    /// </summary>
    public class CrimeModel
    {
        /// <summary>
        /// Type van het delict, zoals "Diefstal", "Vandalisme", "Overlast".
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// Korte beschrijving van het incident.
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Volledig adres in één enkele string (bijv. "Kalverstraat 12, Amsterdam").
        /// </summary>
        public string Address { get; set; } = "";

        /// <summary>
        /// Datum en tijd van het incident, weergegeven als string.
        /// Wordt vaak gebruikt voor UI-weergave of JS-interop.
        /// </summary>
        public string DateTimeString { get; set; } = "";

        /// <summary>
        /// Latitude-coördinaat van de incidentlocatie.
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// Longitude-coördinaat van de incidentlocatie.
        /// </summary>
        public double Lng { get; set; }
    }
}