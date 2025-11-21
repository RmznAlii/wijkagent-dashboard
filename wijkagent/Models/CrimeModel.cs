namespace WijkAgent.Models;

public class CrimeModel
{
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
    public string Address { get; set; } = "";
    public string DateTimeString { get; set; } = "";
    public double Lat { get; set; }
    public double Lng { get; set; }
}