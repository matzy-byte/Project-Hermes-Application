using shared;

namespace Z;

public class Package : PackageData
{
    public Package(string destinationId, string stationId)
    {
        DestinationId = destinationId;
        RobotId = -1;
        StationId = stationId;
    }
}