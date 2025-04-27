using json;
using TrainLines;
public class Coordinate()
{
    public float latitude;
    public float longetude;

    public Coordinate(CoordPositionWGS84Wrapper coordPositionWGS84Wrapper) : this()
    {
        latitude = float.Parse(coordPositionWGS84Wrapper.Lat, System.Globalization.CultureInfo.InvariantCulture);
        longetude = float.Parse(coordPositionWGS84Wrapper.Long, System.Globalization.CultureInfo.InvariantCulture);
    }

    public Coordinate(float latitude, float longetude) : this()
    {
        this.latitude = latitude;
        this.longetude = longetude;
    }

    public Coordinate(double latitude, double longetude) : this()
    {
        this.latitude = (float)latitude;
        this.longetude = (float)longetude;
    }
}