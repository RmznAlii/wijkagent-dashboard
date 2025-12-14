namespace WijkAgent.Core.Models
{
    /// <summary>
    /// Modelklasse die één delict (Crime) vertegenwoordigt in de WijkAgent-applicatie.
    /// 
    /// Deze klasse wordt opgeslagen in de database via Entity Framework Core
    /// en gebruikt in de UI (Blazor) voor het tonen, toevoegen en bewerken van delicten.
    /// </summary>
    public class Crime
    {
        /// <summary>
        /// Unieke identificatie van het delict (primary key in de database).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Het type delict, bijvoorbeeld: "Diefstal", "Vandalisme", "Overlast".
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// Beschrijving van het incident, doorgaans door de melder of agent ingevuld.
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Straat waar het incident plaatsvond.
        /// </summary>
        public string Street { get; set; } = "";

        /// <summary>
        /// Huisnummer van de locatie van het delict.
        /// </summary>
        public string HouseNumber { get; set; } = "";

        /// <summary>
        /// Postcode van het incident.
        /// </summary>
        public string Postcode { get; set; } = "";

        /// <summary>
        /// Stad waar het incident plaatsvond.
        /// </summary>
        public string City { get; set; } = "";

        /// <summary>
        /// Provincie waarin het incident is geregistreerd.
        /// </summary>
        public string Province { get; set; } = "";

        /// <summary>
        /// De latitude-coördinaat (breedtegraad) van de exacte locatie van het delict.
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// De longitude-coördinaat (lengtegraad) van de exacte locatie van het delict.
        /// </summary>
        public double Lng { get; set; }

        /// <summary>
        /// Datum en tijd waarop het incident daadwerkelijk heeft plaatsgevonden.
        /// </summary>
        public DateTime IncidentDateTime { get; set; }

        /// <summary>
        /// Datum en tijd waarop het delict in het systeem is geregistreerd.
        /// Wordt meestal automatisch gezet bij het toevoegen.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
