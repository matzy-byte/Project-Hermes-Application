namespace shared;

public class TransitData
{
    public string LineName { get; set; } = "";
    public string StartStationId { get; set; } = "";
    public string DestinationStationId { get; set; } = "";
    public float TravelTime { get; set; }
    public float TravelTimeReverse { get; set; }
}