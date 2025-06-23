namespace shared;

public class PackageData
{
    public string DestinationId { get; set; } = "";
    public int RobotId { get; set; }
    public string StationId { get; set; } = "";

    /// <summary>
    /// Creates a new package with the given destination and current station.
    /// </summary>
    public PackageData(string destinationId, string stationId)
    {
        DestinationId = destinationId;
        RobotId = -1;
        StationId = stationId;
    }
}