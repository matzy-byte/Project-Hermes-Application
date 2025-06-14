using shared;

namespace Packages;

/// <summary>
/// Represents a package with destination and station information.
/// </summary>
public class Package : PackageData
{
    /// <summary>
    /// Creates a new package with the given destination and current station.
    /// </summary>
    public Package(string destinationId, string stationId)
    {
        DestinationId = destinationId;
        RobotId = -1;
        StationId = stationId;
    }
}
