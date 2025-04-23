using Godot;

public static class GeoMapper
{
    // Geographic bounds (A)
    private const double MinLon = 8.0;
    private const double MaxLon = 8.6;
    private const double MinLat = 48.7;
    private const double MaxLat = 49.3;

    // Game space bounds (B)
    private const float MinX = -1000f;
    private const float MaxX = 1000f;
    private const float MinZ = -750f;
    private const float MaxZ = 750f;

    public static Vector3 LatLonToGameCoords(double lat, double lon)
    {
        float x = Remap((float)lon, (float)MinLon, (float)MaxLon, MinX, MaxX);
        float z = Remap((float)lat, (float)MinLat, (float)MaxLat, MinZ, MaxZ);

        return new Vector3(x, 0f, z);
    }

    private static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
