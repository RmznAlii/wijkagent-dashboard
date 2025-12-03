namespace WijkAgent.Models
{
    public class Crime
    {
        public int Id { get; set; }
        public string Type { get; set; } = "";
        public string Description { get; set; } = "";
        public string Street { get; set; } = "";
        public string HouseNumber { get; set; } = "";
        public string Postcode { get; set; } = "";
        public string City { get; set; } = "";
        public string Province { get; set; } = "";
        public double Lat { get; set; }
        public double Lng { get; set; }
        public DateTime IncidentDateTime { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}