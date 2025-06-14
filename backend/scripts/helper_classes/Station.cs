using shared;

namespace Helper;

public class Station : StationData
{
    /// <summary>
    /// Creates a Station from a StationWrapper object.
    /// </summary>
    public Station(StationWrapper stationWrapper)
    {
        StationId = stationWrapper.TriasID;
        StationName = stationWrapper.Name;
        Longitude = Coordinate.CvtStringToFloat(stationWrapper.CoordPositionWGS84.Long);
        Latitude = Coordinate.CvtStringToFloat(stationWrapper.CoordPositionWGS84.Lat);
    }
}
