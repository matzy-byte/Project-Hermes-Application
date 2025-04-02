using json;
public class Coordinate()
{
    public float latitude;
    public float longetude;

    public Coordinate(CoordPositionWGS84Wrapper coordPositionWGS84Wrapper) : this()
    {
        latitude = float.Parse(coordPositionWGS84Wrapper.Lat);
        longetude = float.Parse(coordPositionWGS84Wrapper.Long);
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