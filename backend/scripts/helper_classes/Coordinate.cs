using shared;

namespace Helper;

public class Coordinate : CoordinateData
{
    /// <summary>
    /// Creates a Coordinate from a WGS84 wrapper object.
    /// </summary>
    public Coordinate(CoordPositionWGS84Wrapper coordPositionWGS84Wrapper)
    {
        Latitude = float.Parse(coordPositionWGS84Wrapper.Lat, System.Globalization.CultureInfo.InvariantCulture);
        Longitude = float.Parse(coordPositionWGS84Wrapper.Long, System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Creates a Coordinate from float latitude and longitude.
    /// </summary>
    public Coordinate(float latitude, float longetude)
    {
        Latitude = latitude;
        Longitude = longetude;
    }

    /// <summary>
    /// Creates a Coordinate from double latitude and longitude.
    /// </summary>
    public Coordinate(double latitude, double longetude)
    {
        Latitude = (float)latitude;
        Longitude = (float)longetude;
    }

    /// <summary>
    /// Converts a string representation of a number to a float.
    /// </summary>
    public static float CvtStringToFloat(string data)
    {
        return float.Parse(data, System.Globalization.CultureInfo.InvariantCulture);
    }
}
