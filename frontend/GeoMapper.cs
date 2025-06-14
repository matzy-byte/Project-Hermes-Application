using Godot;

namespace helper;

public static class GeoMapper
{
    private const double MINLON = 8.28327;
    private const double MAXLON = 8.50511;
    private const double MINLAT = 48.80172;
    private const double MAXLAT = 49.13373;
    private const float MINX = -8000f;
    private const float MAXX = 8000f;
    private const float MINZ = -12000f;
    private const float MAXZ = 12000f;

    private static readonly float LON_SCALE_CORRECTION = Mathf.Cos(Mathf.DegToRad(49.0f));

    public static Vector3 LatLonToGameCoords(double lat, double lon)
    {
        float x = Remap((float)lon, (float)MINLON, (float)MAXLON, MAXX, MINX) * LON_SCALE_CORRECTION;
        float z = Remap((float)lat, (float)MINLAT, (float)MAXLAT, MINZ, MAXZ);

        return new Vector3(x, 0f, z);
    }

    private static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}