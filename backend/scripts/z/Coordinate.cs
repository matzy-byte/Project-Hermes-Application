using shared;

namespace Z;

public class Coordinate : CoordinateData
{
    public Coordinate(CoordPositionWGS84Wrapper coordPositionWGS84Wrapper)
    {
        Latitude = float.Parse(coordPositionWGS84Wrapper.Lat, System.Globalization.CultureInfo.InvariantCulture);
        Longitude = float.Parse(coordPositionWGS84Wrapper.Long, System.Globalization.CultureInfo.InvariantCulture);
    }

    public Coordinate(float latitude, float longetude)
    {
        Latitude = latitude;
        Longitude = longetude;
    }

    public Coordinate(double latitude, double longetude)
    {
        Latitude = (float)latitude;
        Longitude = (float)longetude;
    }

    public static float CvtStringToFloat(string data)
    {
        return float.Parse(data, System.Globalization.CultureInfo.InvariantCulture);
    }
}