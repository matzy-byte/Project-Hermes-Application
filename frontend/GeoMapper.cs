using Godot;

namespace helper;

public static class GeoMapper
{
    private const double MINLON = 8.2;
    private const double MAXLON = 8.7;
    private const double MINLAT = 48.7;
    private const double MAXLAT = 49.2;
    private const float MINX = -2000f;
    private const float MAXX = 2000f;
    private const float MINZ = -2000f;
    private const float MAXZ = 2000f;

    public static Vector3 LatLonToGameCoords(double lat, double lon)
    {
        float x = Remap((float)lon, (float)MINLON, (float)MAXLON, MINX, MAXX);
        float z = Remap((float)lat, (float)MINLAT, (float)MAXLAT, MINZ, MAXZ);

        return new Vector3(x, 0f, z);
    }

    private static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}